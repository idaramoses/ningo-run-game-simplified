using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class NigerianCitySceneryBuilder
{
    private const string Art = "Assets/Art/CityScenery";
    private const string Prefabs = "Assets/Prefab/CityScenery";
    private static readonly Color[] Colors = { new Color(.12f,.18f,.19f), new Color(.94f,.70f,.12f), new Color(.80f,.32f,.12f), new Color(.12f,.42f,.30f), new Color(.80f,.78f,.65f), new Color(.20f,.42f,.57f), new Color(.43f,.26f,.15f), new Color(.93f,.92f,.83f) };
    private static readonly List<CombineInstance> Parts = new List<CombineInstance>();
    private static readonly List<Mesh> TemporaryMeshes = new List<Mesh>();
    private static Mesh _cube;
    private static Material _palette;

    [MenuItem("Tools/Ningo/Build Nigerian Scenery Prefabs")]
    public static void Build()
    {
        Directory.CreateDirectory(Art);
        Directory.CreateDirectory(Prefabs + "/Shared");
        foreach (string city in new[] { "Lagos", "Abuja", "PortHarcourt" }) Directory.CreateDirectory(Prefabs + "/" + city);
        AssetDatabase.Refresh();
        var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(temp);
        var texture = new Texture2D(8, 1, TextureFormat.RGB24, false);
        texture.SetPixels(Colors); texture.Apply();
        string texturePath = Art + "/StreetPalette.png";
        File.WriteAllBytes(texturePath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false; importer.SaveAndReimport();
        _palette = new Material(Shader.Find("Custom/CurvedDiffuse"));
        _palette.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        _palette.enableInstancing = true;
        AssetDatabase.CreateAsset(_palette, Art + "/StreetPalette.mat");
        for (int city = 0; city < 3; city++) for (int kind = 0; kind < 3; kind++) BuildCluster(city, kind);
        AssetDatabase.SaveAssets();
    }

    private static void Box(Vector3 p, Vector3 size, int color, float yaw = 0)
    {
        var m = Object.Instantiate(_cube);
        var uv = new Vector2[m.vertexCount];
        for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2((color + .5f) / 8f, .5f);
        m.uv = uv;
        TemporaryMeshes.Add(m);
        Parts.Add(new CombineInstance { mesh = m, transform = Matrix4x4.TRS(p, Quaternion.Euler(0, yaw, 0), size) });
    }

    private static void BuildCluster(int city, int kind)
    {
        string cityName = new[] { "Lagos", "Abuja", "PortHarcourt" }[city];
        string[] names = city == 0 ? new[] { "StreetFoodCorner", "DanfoBusStop", "LagoonPromenade" } : city == 1 ? new[] { "BoulevardShelter", "NeighbourhoodKiosk", "GardenCityGate" } : new[] { "RiversMarket", "PortContainerYard", "WaterfrontJetty" };
        string name = names[kind];
        var root = new GameObject(name);
        Parts.Clear(); TemporaryMeshes.Clear();
        if (city == 0 && kind == 0)
        {
            CopyVisual(root, "Assets/Model/Lagos/road-side-corn/road-side-shop.prefab", new Vector3(0,0,0), 2.7f);
            Box(new Vector3(2, .6f, 0), new Vector3(1.2f,1.2f,1), 6);
            Box(new Vector3(2,1.24f,0), new Vector3(1.4f,.12f,1.2f), 1);
            Sign(root, "LAGOS\nSUYA & ROASTED CORN", new Vector3(0,3.1f,-1.4f), new Vector2(4.5f,1.1f), city);
        }
        else if ((city == 0 && kind == 1) || (city == 1 && kind == 0))
        {
            Shelter(city == 0 ? 1 : 3);
            Sign(root, city == 0 ? "LAGOS\nDANFO BUS STOP" : "ABUJA\nCITY BUS STOP", new Vector3(0,2.95f,-.95f),new Vector2(4.4f,1),city);
        }
        else if (city == 1 && kind == 1)
        {
            Kiosk(3);
            Sign(root,"ABUJA\nSUYA & PROVISIONS",new Vector3(0,2.7f,-1.26f),new Vector2(3.8f,.95f),city);
        }
        else if (city == 1 && kind == 2)
        {
            Box(new Vector3(-2,1.6f,0),new Vector3(.7f,3.2f,.8f),4);
            Box(new Vector3(2,1.6f,0),new Vector3(.7f,3.2f,.8f),4);
            Box(new Vector3(0,3.2f,0),new Vector3(4.8f,.7f,1),3);
            CopyVisual(root,"Assets/Model/Abuja/planter 3d model/planter 3d model.prefab",new Vector3(-3.4f,0,0),1.2f);
            CopyVisual(root,"Assets/Model/Abuja/planter 3d model/planter 3d model.prefab",new Vector3(3.4f,0,0),1.2f);
            Sign(root,"ABUJA\nWELCOME TO THE FCT",new Vector3(0,3.2f,-.52f),new Vector2(4.2f,.65f),city);
        }
        else if (city == 2 && kind == 0)
        {
            Kiosk(5);
            Box(new Vector3(-2.8f,.65f,0),new Vector3(1.5f,1.3f,1.1f),6);
            for(int i=0;i<3;i++)Box(new Vector3(-3.25f+i*.45f,1.36f,0),new Vector3(.38f,.15f,.8f),i==0?1:3);
            Sign(root,"PORT HARCOURT\nRIVERS MARKET",new Vector3(0,2.7f,-1.26f),new Vector2(3.8f,.95f),city);
        }
        else if (city == 2 && kind == 1)
        {
            Container(new Vector3(-2.4f,1.3f,0),5);
            Container(new Vector3(2.4f,1.3f,0),2);
            Container(new Vector3(-2.4f,3.9f,0),3);
            Box(new Vector3(4.9f,4.5f,1),new Vector3(.45f,9,.45f),1);
            Box(new Vector3(1.7f,8.8f,1),new Vector3(7,.35f,.45f),1);
            Box(new Vector3(-1.4f,6.9f,1),new Vector3(.08f,3.4f,.08f),0);
            Sign(root,"PORT HARCOURT\nPORT LOGISTICS",new Vector3(2.4f,1.8f,-1.46f),new Vector2(3.6f,.8f),city);
        }
        else
        {
            for(int i=0;i<8;i++)Box(new Vector3(0,.16f,-1.75f+i*.5f),new Vector3(5,.2f,.43f),6);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++)Box(new Vector3(side*2.35f,.75f,-1.7f+i*1.7f),new Vector3(.14f,1.5f,.14f),city==0?7:6);
            Box(new Vector3(-2.35f,1.4f,0),new Vector3(.12f,.12f,3.6f),city==0?7:6);
            Box(new Vector3(2.35f,1.4f,0),new Vector3(.12f,.12f,3.6f),city==0?7:6);
            Sign(root,city==0?"LAGOS\nLAGOON WALK":"PORT HARCOURT\nWATERFRONT",new Vector3(0,1.6f,-1.9f),new Vector2(3.4f,.8f),city);
        }
        var mesh = new Mesh { name = name + "Geometry" };
        mesh.CombineMeshes(Parts.ToArray(),true,true); mesh.RecalculateBounds();
        // Expand vertical culling bounds to accommodate the existing curved-world shader.
        var bounds=mesh.bounds; bounds.Expand(new Vector3(0,50,0)); mesh.bounds=bounds;
        AssetDatabase.CreateAsset(mesh,Art+"/"+cityName+"_"+name+".asset");
        var geometry=new GameObject("CombinedStreetProps"); geometry.transform.SetParent(root.transform,false);
        geometry.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=geometry.AddComponent<MeshRenderer>();renderer.sharedMaterial=_palette;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        foreach(var m in TemporaryMeshes)Object.DestroyImmediate(m);
        PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"/"+cityName+"/"+name+".prefab");
        Object.DestroyImmediate(root);
    }
    private static void Shelter(int color)
    {
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box(new Vector3(x*2,1.35f,z*.8f),new Vector3(.12f,2.7f,.12f),0);
        Box(new Vector3(0,2.75f,0),new Vector3(4.6f,.18f,2.2f),color);
        Box(new Vector3(0,.6f,.55f),new Vector3(3.6f,.15f,.6f),6);
        Box(new Vector3(0,1,.8f),new Vector3(3.6f,.65f,.1f),6);
        for(int x=-1;x<=1;x+=2)Box(new Vector3(x*1.4f,.3f,.55f),new Vector3(.12f,.6f,.5f),0);
    }
    private static void Kiosk(int color)
    {
        Box(new Vector3(0,1.1f,0),new Vector3(3.5f,2.2f,2.4f),4);
        Box(new Vector3(0,2.45f,-.4f),new Vector3(4.3f,.2f,3.5f),color);
        Box(new Vector3(0,1.6f,-1.21f),new Vector3(2.8f,.8f,.05f),0);
        Box(new Vector3(0,1.12f,-1.45f),new Vector3(3.8f,.15f,.6f),6);
        for(int i=0;i<5;i++)Box(new Vector3(-1.1f+i*.55f,1.3f,-1.4f),new Vector3(.4f,.22f,.3f),i%2==0?1:2);
    }
    private static void Container(Vector3 p,int color)
    {
        Box(p,new Vector3(4.4f,2.6f,2.8f),color);
        for(int i=0;i<12;i++)Box(p+new Vector3(-2.1f+i*.38f,0,-1.42f),new Vector3(.05f,2.45f,.06f),4);
    }
    private static void CopyVisual(GameObject root,string path,Vector3 center,float width)
    {
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var rs=source.GetComponentsInChildren<MeshRenderer>(true);
        var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
        float scale=width/b.size.x;
        foreach(var r in rs)
        {
            var mf=r.GetComponent<MeshFilter>();if(mf==null||mf.sharedMesh==null)continue;
            var mesh=Object.Instantiate(mf.sharedMesh);
            var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++){var v=r.transform.TransformPoint(vertices[i]);vertices[i]=(v-new Vector3(b.center.x,b.min.y,b.center.z))*scale;}
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,Art+"/"+root.name+"_"+root.transform.childCount+"_Visual.asset");
            var go=new GameObject("Reused_"+source.name);go.transform.SetParent(root.transform,false);go.transform.localPosition=center;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();mr.sharedMaterials=r.sharedMaterials;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
    public static void DecorateConfiguredCities()
    {
        var manager=Object.FindAnyObjectByType<CityManager>();
        if(manager==null)throw new System.InvalidOperationException("CityManager is missing.");
        string[][] clusters={new[]{"StreetFoodCorner","DanfoBusStop","LagoonPromenade"},new[]{"BoulevardShelter","NeighbourhoodKiosk","GardenCityGate"},new[]{"RiversMarket","PortContainerYard","WaterfrontJetty"}};
        for(int i=0;i<3;i++)
        {
            var city=manager.GetCity(i);
            for(int tile=0;tile<city.roadTiles.Length;tile++)
            {
                string path=AssetDatabase.GetAssetPath(city.roadTiles[tile]);
                var contents=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if(contents.transform.Find("CityScenery")==null)
                    {
                        var decoration=new GameObject("CityScenery");decoration.transform.SetParent(contents.transform,false);
                        int first=(i==2&&tile==0)?2:tile%3;
                        PlaceCluster(contents,decoration,city.cityName,clusters[i][first],-1,12f);
                        if(!(i==2&&tile==0))PlaceCluster(contents,decoration,city.cityName,clusters[i][(first+1)%3],1,27f);
                        PrefabUtility.SaveAsPrefabAsset(contents,path);
                    }
                }
                finally {PrefabUtility.UnloadPrefabContents(contents);}
            }
            var starter=city.starterRoadGroup;
            if(starter.transform.Find("CityScenery")==null)
            {
                Undo.RegisterFullObjectHierarchyUndo(starter,"Decorate city starter");
                var decoration=new GameObject("CityScenery");Undo.RegisterCreatedObjectUndo(decoration,"Create city scenery");decoration.transform.SetParent(starter.transform,false);
                PlaceCluster(starter,decoration,city.cityName,clusters[i][0],-1,18f);
                PlaceCluster(starter,decoration,city.cityName,clusters[i][1],1,30f);
            }
        }
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    private static Bounds ActualBounds(GameObject go,Transform relative)
    {
        bool first=true;Bounds bounds=new Bounds();
        foreach(var mf in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if(mf.sharedMesh==null)continue;
            var matrix=relative.worldToLocalMatrix*mf.transform.localToWorldMatrix;
            foreach(var v in mf.sharedMesh.vertices)
            {
                var point=matrix.MultiplyPoint3x4(v);
                if(first){bounds=new Bounds(point,Vector3.zero);first=false;}else bounds.Encapsulate(point);
            }
        }
        return bounds;
    }
    private static void PlaceCluster(GameObject road,GameObject parent,string city,string name,int side,float desiredZ)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"/"+city+"/"+name+".prefab");
        var existing=parent.transform.Find(name);
        var cluster=existing!=null?existing.gameObject:(GameObject)PrefabUtility.InstantiatePrefab(prefab,road.scene);cluster.transform.SetParent(parent.transform,false);
        var shape=ActualBounds(cluster,cluster.transform);
        var occupied=new List<Bounds>();
        foreach(var mf in road.GetComponentsInChildren<MeshFilter>(true))
        {
            if(mf.transform.IsChildOf(parent.transform)||mf.sharedMesh==null)continue;
            var matrix=road.transform.worldToLocalMatrix*mf.transform.localToWorldMatrix;
            bool first=true;Bounds b=new Bounds();
            foreach(var v in mf.sharedMesh.vertices){var point=matrix.MultiplyPoint3x4(v);if(first){b=new Bounds(point,Vector3.zero);first=false;}else b.Encapsulate(point);}
            if(b.size.y>.6f)
            {
                string objectName=mf.name.ToLowerInvariant();
                if((objectName.Contains("tree")||objectName.Contains("palm"))&&b.size.y>4f)b.size=new Vector3(.7f,b.size.y,.7f);
                occupied.Add(b);
            }
        }
        foreach(Transform child in parent.transform)if(child!=cluster.transform)occupied.Add(ActualBounds(child.gameObject,road.transform));
        Vector3 best=new Vector3(side*8,0,desiredZ);float bestScore=float.MaxValue;
        for(float x=4.3f+shape.extents.x;x<65;x+=1.5f)for(float z=4+shape.extents.z;z<36-shape.extents.z;z+=2)
        {
            var position=new Vector3(side*x,0,z);
            var b=new Bounds(shape.center+position,shape.size+new Vector3(.4f,0,.4f));
            float overlap=0;
            foreach(var o in occupied)
            {
                float dx=Mathf.Min(b.max.x,o.max.x)-Mathf.Max(b.min.x,o.min.x);
                float dz=Mathf.Min(b.max.z,o.max.z)-Mathf.Max(b.min.z,o.min.z);
                if(dx>0&&dz>0)overlap+=dx*dz;
            }
            float score=overlap*10000+x*2+Mathf.Abs(z-desiredZ);
            if(score<bestScore){bestScore=score;best=position;}
        }
        cluster.transform.localPosition=best;
        Debug.Log("[CityScenery] "+city+" "+name+" at "+best+" score="+bestScore);
    }
    public static void RefreshPlacement()
    {
        var manager=Object.FindAnyObjectByType<CityManager>();
        for(int i=0;i<3;i++)
        {
            var city=manager.GetCity(i);
            foreach(var tile in city.roadTiles)
            {
                string path=AssetDatabase.GetAssetPath(tile);var contents=PrefabUtility.LoadPrefabContents(path);
                try{RefreshGroup(contents,city.cityName);PrefabUtility.SaveAsPrefabAsset(contents,path);}finally{PrefabUtility.UnloadPrefabContents(contents);}
            }
            Undo.RegisterFullObjectHierarchyUndo(city.starterRoadGroup,"Fit Nigerian scenery");
            RefreshGroup(city.starterRoadGroup,city.cityName);
        }
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    private static void RefreshGroup(GameObject road,string city)
    {
        var parent=road.transform.Find("CityScenery");if(parent==null)return;
        for(int i=0;i<parent.childCount;i++){var child=parent.GetChild(i);PlaceCluster(road,parent.gameObject,city,child.name,i==0?-1:1,i==0?12:27);}
    }
    public static void AddRoadsideMarkers()
    {
        var manager=Object.FindAnyObjectByType<CityManager>();
        string[] signs={"DanfoBusStop","BoulevardShelter","RiversMarket"};
        for(int i=0;i<3;i++)
        {
            var city=manager.GetCity(i);
            var root=new GameObject("CityRoadsideMarker");
            var board=GameObject.CreatePrimitive(PrimitiveType.Quad);Object.DestroyImmediate(board.GetComponent<Collider>());
            board.name="BakedCitySign";board.transform.SetParent(root.transform,false);board.transform.localPosition=new Vector3(0,3.5f,0);board.transform.localScale=new Vector3(2.2f,.75f,1);
            board.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/"+signs[i]+"_Sign.mat");board.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var pole=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(pole.GetComponent<Collider>());
            pole.name="SignSupport";pole.transform.SetParent(root.transform,false);pole.transform.localPosition=new Vector3(0,1.7f,.08f);pole.transform.localScale=new Vector3(.09f,3.4f,.09f);
            pole.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/StreetPalette.mat");pole.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,Prefabs+"/"+city.cityName+"/CityRoadsideMarker.prefab");Object.DestroyImmediate(root);
            foreach(var tile in city.roadTiles)
            {
                string path=AssetDatabase.GetAssetPath(tile);var contents=PrefabUtility.LoadPrefabContents(path);
                try{AddMarkers(contents,prefab);PrefabUtility.SaveAsPrefabAsset(contents,path);}finally{PrefabUtility.UnloadPrefabContents(contents);}
            }
            Undo.RegisterFullObjectHierarchyUndo(city.starterRoadGroup,"Add city roadside markers");
            AddMarkers(city.starterRoadGroup,prefab);
        }
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    private static void AddMarkers(GameObject road,GameObject prefab)
    {
        var parent=road.transform.Find("CityScenery");
        for(int side=-1;side<=1;side+=2)
        {
            string name=side<0?"CityMarkerLeft":"CityMarkerRight";
            if(parent.Find(name)!=null)continue;
            var marker=(GameObject)PrefabUtility.InstantiatePrefab(prefab,road.scene);marker.name=name;marker.transform.SetParent(parent,false);
            marker.transform.localPosition=new Vector3(side*5.1f,0,side<0?10:28);
        }
    }
    public static void RefreshSigns()
    {
        string[] names={"StreetFoodCorner","DanfoBusStop","LagoonPromenade","BoulevardShelter","NeighbourhoodKiosk","GardenCityGate","RiversMarket","PortContainerYard","WaterfrontJetty"};
        string[] texts={"LAGOS\nSUYA & ROASTED CORN","LAGOS\nDANFO BUS STOP","LAGOS\nLAGOON WALK","ABUJA\nCITY BUS STOP","ABUJA\nSUYA & PROVISIONS","ABUJA\nWELCOME TO THE FCT","PORT HARCOURT\nRIVERS MARKET","PORT HARCOURT\nPORT LOGISTICS","PORT HARCOURT\nWATERFRONT"};
        for(int i=0;i<names.Length;i++)
        {
            var root=new GameObject(names[i]);
            Sign(root,texts[i],Vector3.zero,Vector2.one,i/3);
            Object.DestroyImmediate(root);
        }
    }
    private static void Sign(GameObject root,string text,Vector3 p,Vector2 size,int city)
    {
        var label=new GameObject("BakeLabel");label.layer=31;
        var tm=label.AddComponent<TextMesh>();tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.fontSize=96;tm.characterSize=.11f;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.text=text;
        label.GetComponent<MeshRenderer>().sharedMaterial=tm.font.material;
        var cameraObject=new GameObject("BakeCamera");var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.aspect=4;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=city==0?Colors[0]:Colors[3];
        var textBounds=label.GetComponent<MeshRenderer>().bounds;
        camera.orthographicSize=Mathf.Max(textBounds.extents.y,textBounds.extents.x/4f)*1.15f;
        cameraObject.transform.position=new Vector3(textBounds.center.x,textBounds.center.y,-10);
        var rt=new RenderTexture(1024,256,16);camera.targetTexture=rt;camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=rt;
        var texture=new Texture2D(1024,256,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1024,256),0,0);texture.Apply();RenderTexture.active=previous;
        string path=Art+"/"+root.name+"_Sign.png";File.WriteAllBytes(path,texture.EncodeToPNG());
        camera.targetTexture=null;Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(label);Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var mat=AssetDatabase.LoadAssetAtPath<Material>(Art+"/"+root.name+"_Sign.mat");
        if(mat==null){mat=new Material(Shader.Find("Custom/CurvedDiffuse"));AssetDatabase.CreateAsset(mat,Art+"/"+root.name+"_Sign.mat");}
        mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);mat.enableInstancing=true;
        var board=GameObject.CreatePrimitive(PrimitiveType.Quad);Object.DestroyImmediate(board.GetComponent<Collider>());board.name="BakedCitySign";board.transform.SetParent(root.transform,false);board.transform.localPosition=p;board.transform.localScale=new Vector3(size.x,size.y,1);board.GetComponent<MeshRenderer>().sharedMaterial=mat;board.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
}
