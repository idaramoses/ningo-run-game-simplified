using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

[InitializeOnLoad]
internal static class PlayModeTestRunner
{
    private static BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly List<string> Logs = new List<string>();
    private static Component runner, game, city, ui, spawner, traffic;
    private static GameObject prefab;
    private static Component[] taxis;
    private static double started, phaseTime;
    private static int phase;
    private static Vector3 pausePosition;
    private static int activations;
    private static bool captured;
    private static string oldCity;
    static PlayModeTestRunner()
    {
        string state = SessionState.GetString("PlayModeTest.State", "Idle");
        if (state == "WaitingForCompile") EditorApplication.delayCall += () => {
            SessionState.SetString("PlayModeTest.State", "EnteringPlayMode");
            EditorApplication.playModeStateChanged += Changed;
            EditorApplication.isPlaying = true;
        };
        else if (state == "EnteringPlayMode" || state == "InPlayMode") {
            EditorApplication.playModeStateChanged += Changed;
            if (EditorApplication.isPlaying) Begin();
        }
    }
    private static void Changed(PlayModeStateChange s) { if (s == PlayModeStateChange.EnteredPlayMode) Begin(); }
    private static void Begin() { EditorApplication.playModeStateChanged -= Changed; SessionState.SetString("PlayModeTest.State", "InPlayMode"); started = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
    private static Component Locate(string name) {
        foreach (var m in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            if (m != null && m.GetType().Name == name && m.gameObject.activeInHierarchy) return m;
        throw new Exception("Missing runtime " + name);
    }
    private static object Call(Component c, string name, params object[] args) {
        foreach(var m in c.GetType().GetMethods(Flags)) if(m.Name==name && m.GetParameters().Length==args.Length) {
            bool compatible=true;var ps=m.GetParameters();for(int i=0;i<args.Length;i++)if(args[i]!=null&&!ps[i].ParameterType.IsInstanceOfType(args[i]))compatible=false;
            if(compatible)return m.Invoke(c,args);
        }
        throw new Exception("Method missing "+name);
    }
    private static object Prop(Component c, string name) => c.GetType().GetProperty(name,Flags).GetValue(c);
    private static void Field(Component c,string name,object value) { c.GetType().GetField(name,Flags).SetValue(c,value); }
    private static void Check(bool ok,string message) { Logs.Add(message+" = "+ok);Debug.Log("[Test] "+message+" = "+ok);if(!ok)throw new Exception(message); }
    private static Component[] TaxiComponents(GameObject root) {
        var list=new List<Component>();foreach(var m in root.GetComponentsInChildren<MonoBehaviour>(true))if(m!=null&&m.GetType().Name=="JunctionTaxiController")list.Add(m);return list.ToArray();
    }
    private static void Tick()
    {
        try {
            double now=EditorApplication.timeSinceStartup;
            if(now-started>28)throw new Exception("Test timeout phase "+phase+" activations "+activations);
            if(phase==0) {
                if(now-started<3) return;
                runner=Locate("SimplePlayerController");game=Locate("GameStateController");city=Locate("CityManager");ui=Locate("UImanager");spawner=Locate("RoadSpawner");
                oldCity=PlayerPrefs.GetString("SelectedCity","");
                prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Road/Abuja/Abuja Road3 junction.prefab");
                var preview=UnityEngine.Object.Instantiate(prefab,new Vector3(0,0,350),Quaternion.identity);
                var ts=TaxiComponents(preview);Check(ts.Length==2,"Exactly two taxi controllers");
                foreach(var t in ts) {
                    Check(!(bool)((Behaviour)t.GetComponent("VehicleMover")).enabled,"Original mover disabled");
                    Check((bool)Call(runner,"IsObstacle",t.GetComponent<Collider>()),"Existing collision classifier recognizes taxi");
                    for(int lane=0;lane<3;lane++)for(int dir=-1;dir<=1;dir+=2) {
                        Call(t,"ResetEncounter");Check((bool)Call(t,"BeginRoute",lane,(lane-1)*2.3f,dir),"Begin route "+t.transform.parent.name+"/"+lane+"/"+dir);
                        Vector3 previous=t.transform.position;
                        for(int f=0;f<450;f++) {
                            Call(t,"Advance",.01f,null);
                            if(Vector3.Distance(previous,t.transform.position)>.052f)throw new Exception("Movement discontinuity");previous=t.transform.position;
                            if(Prop(t,"State").ToString()=="Cruising")break;
                        }
                        Check(Prop(t,"State").ToString()=="Cruising"&&Mathf.Abs(t.transform.position.x-(lane-1)*2.3f)<.01f,"Cruise lane and constant world speed");
                    }
                }
                UnityEngine.Object.Destroy(preview);
                Call(city,"SetCity","Abuja");Field(spawner,"roadTilePrefabs",new GameObject[]{prefab});Call(spawner,"ResetSpawner");
                Call(ui,"StartRunTransition");Call(runner,"MakeInvulnerable",100f);
                phase=1;phaseTime=now;return;
            }
            if(phase==1) {
                if(!(bool)Prop(runner,"IsMoving"))return;
                Check((bool)Call(game,"IsPlaying"),"Single scene gameplay started");
                traffic=null;float z=float.PositiveInfinity;
                foreach(var m in UnityEngine.Object.FindObjectsByType<MonoBehaviour>())if(m!=null&&m.GetType().Name=="JunctionTrafficController"&&m.transform.root.position.z>runner.transform.position.z+20&&m.transform.root.position.z<z){traffic=m;z=m.transform.root.position.z;}
                if(traffic==null)return;
                taxis=TaxiComponents(traffic.transform.root.gameObject);
                runner.transform.position=new Vector3(0,0,z-45);Field(runner,"forwardSpeed",10f);
                Check(Prop(taxis[0],"State").ToString()=="Waiting"&&Prop(taxis[1],"State").ToString()=="Waiting","Distant taxis remain parked");
                phase=2;phaseTime=now;return;
            }
            if(phase==2) {
                foreach(var t in taxis)if(Prop(t,"IsActive").Equals(true)){
                    activations++;pausePosition=t.transform.position;Time.timeScale=0f;phase=3;phaseTime=now;return;
                }
                return;
            }
            if(phase==3) {
                if(now-phaseTime<.35)return;
                bool still=false;foreach(var t in taxis)if((t.transform.position-pausePosition).sqrMagnitude<.00001f)still=true;
                Check(still,"Pause freezes active taxi");Time.timeScale=1f;phase=4;phaseTime=now;return;
            }
            if(phase==4) {
                foreach(var t in taxis) {
                    if(Prop(t,"State").ToString()=="Turning"&&!captured) {
                        ScreenCapture.CaptureScreenshot("Assets/JunctionTaxi_RuntimeVerification.png");SessionState.SetString("PlayModeTest.ScreenshotPath","Assets/JunctionTaxi_RuntimeVerification.png");captured=true;
                        Logs.Add("Captured turning taxi "+t.transform.parent.name+" at "+t.transform.position);
                    }
                }
                if(now-phaseTime<5)return;
                int committed=0;foreach(var t in taxis) {
                    if((int)Prop(t,"SelectedLane")>=0)committed++;
                    Logs.Add("Taxi "+t.transform.parent.name+" state="+Prop(t,"State")+" lane="+Prop(t,"SelectedLane")+" runnerZ="+runner.transform.position.z+" entryZ="+Prop(t,"EntryWorldZ"));
                    if((int)Prop(t,"SelectedLane")<0)for(int lane=0;lane<3;lane++)for(int dir=-1;dir<=1;dir+=2)Logs.Add("Candidate "+lane+"/"+dir+" clear="+Call(t,"IsRouteClear",(lane-1)*2.3f,dir,runner.transform)+" duration="+Call(t,"EstimateEntryTime",(lane-1)*2.3f));
                }
                Check(committed==2,"Both nearby taxis activate and commit routes");
                var old=traffic.transform.root.gameObject;Call(city,"SetCity","Lagos");phase=5;phaseTime=now;traffic=old.GetComponentInChildren<MonoBehaviour>();return;
            }
            if(phase==5) {
                if(now-phaseTime<.2)return;
                Check(traffic==null,"City switch destroys previous tile-owned traffic");
                Call(city,"SetCity","Abuja");Field(spawner,"roadTilePrefabs",new GameObject[]{prefab});Call(spawner,"ResetSpawner");Field(runner,"forwardSpeed",20f);runner.transform.position=new Vector3(0,0,6.5f);
                phase=6;phaseTime=now;return;
            }
            if(phase==6) {
                if(now-phaseTime<.2)return;
                traffic=null;float z=float.PositiveInfinity;
                foreach(var m in UnityEngine.Object.FindObjectsByType<MonoBehaviour>())if(m!=null&&m.GetType().Name=="JunctionTrafficController"&&m.transform.root.position.z>runner.transform.position.z+20&&m.transform.root.position.z<z){traffic=m;z=m.transform.root.position.z;}
                if(traffic==null)return;taxis=TaxiComponents(traffic.transform.root.gameObject);
                runner.transform.position=new Vector3(0,0,z-60);phase=7;phaseTime=now;return;
            }
            if(phase==7) {
                int active=0;foreach(var t in taxis)if((int)Prop(t,"SelectedLane")>=0)active++;
                if(active<2&&now-phaseTime<6)return;
                Check(active==2,"Both taxis activate at increased runner speed");
                var root=traffic.transform.root.gameObject;root.SetActive(false);root.SetActive(true);phase=8;phaseTime=now;return;
            }
            if(phase==8) {
                Call(game,"SetPlaying",false);
                if(now-phaseTime<.1)return;
                foreach(var t in taxis)Check(t.gameObject.activeSelf&&Prop(t,"State").ToString()=="Waiting","Re-enable resets taxi pose and state");
                Finish(true,null);
            }
        }catch(Exception e){Finish(false,e.ToString());}
    }
    [Serializable] private class Result { public bool success;public string error;public string[] logs;public bool screenshotCaptured; }
    private static void Finish(bool success,string error) {
        EditorApplication.update-=Tick;Time.timeScale=1f;
        if(oldCity!=null){if(oldCity.Length==0)PlayerPrefs.DeleteKey("SelectedCity");else PlayerPrefs.SetString("SelectedCity",oldCity);PlayerPrefs.Save();}
        SessionState.SetString("PlayModeTest.Result",JsonUtility.ToJson(new Result{success=success,error=error,logs=Logs.ToArray(),screenshotCaptured=captured}));SessionState.SetString("PlayModeTest.State","Done");EditorApplication.isPlaying=false;
    }
}
