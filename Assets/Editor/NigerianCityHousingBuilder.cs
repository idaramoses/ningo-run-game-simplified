using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class NigerianCityHousingBuilder
{
    public const string Folder = "Assets/Art/CityScenery/Housing";
    private static readonly string[] Cities = { "Lagos", "Abuja", "PortHarcourt" };
    private static readonly string[] Houses = { "BalconyApartments", "ModernCompound", "VerandaBungalow" };
    private static readonly List<CombineInstance> Parts = new List<CombineInstance>();
    private static readonly List<Mesh> Temporary = new List<Mesh>();
    private static Mesh _cube;

    public static void BuildVariants()
    {
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube = primitive.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(primitive);
        for (int city = 0; city < 3; city++)
        {
            string prefabFolder = "Assets/Prefab/CityScenery/" + Cities[city] + "/Housing";
            Directory.CreateDirectory(prefabFolder);
            Parts.Clear(); Temporary.Clear();
            if (city == 0) Apartments(); else if (city == 1) Compound(); else Bungalow();
            var mesh = new Mesh { name = Houses[city] };
            mesh.CombineMeshes(Parts.ToArray(), true, true);
            mesh.RecalculateBounds();
            string meshPath = Folder + "/" + Houses[city] + ".asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null) throw new System.InvalidOperationException("Housing already authored; refusing to overwrite.");
            AssetDatabase.CreateAsset(mesh, meshPath);
            foreach (var m in Temporary) Object.DestroyImmediate(m);
            for (int variant = 0; variant < 2; variant++)
            {
                var colors = new[] { new Color(.82f,.65f,.41f), new Color(.36f,.18f,.12f), new Color(.15f,.23f,.27f), new Color(.91f,.89f,.77f), new Color(.18f,.36f,.29f), new Color(.27f,.48f,.58f), new Color(.11f,.13f,.14f), new Color(.53f,.31f,.16f) };
                if (city == 0) { colors[0] = variant == 0 ? new Color(.86f,.63f,.29f) : new Color(.64f,.76f,.65f); colors[1] = new Color(.53f,.21f,.13f); }
                if (city == 1) { colors[0] = variant == 0 ? new Color(.88f,.85f,.75f) : new Color(.69f,.74f,.76f); colors[1] = new Color(.23f,.28f,.30f); }
                if (city == 2) { colors[0] = variant == 0 ? new Color(.77f,.84f,.66f) : new Color(.88f,.72f,.54f); colors[1] = variant == 0 ? new Color(.62f,.23f,.13f) : new Color(.20f,.36f,.39f); }
                var texture = new Texture2D(8,1,TextureFormat.RGB24,false); texture.SetPixels(colors); texture.Apply();
                string texPath = Folder + "/" + Cities[city] + "_" + variant + ".png";
                File.WriteAllBytes(texPath,texture.EncodeToPNG());Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(texPath,ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(texPath); importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.SaveAndReimport();
                var mat = new Material(Shader.Find("Custom/CurvedDiffuse")) { name = Cities[city]+"Housing"+variant, mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath), enableInstancing = true };
                AssetDatabase.CreateAsset(mat,Folder+"/"+Cities[city]+"_"+variant+".mat");
                var root = new GameObject(Houses[city]+"_"+variant);
                root.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=root.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                PrefabUtility.SaveAsPrefabAsset(root,prefabFolder+"/"+root.name+".prefab");Object.DestroyImmediate(root);
            }
        }
        AssetDatabase.SaveAssets();
    }
    private static void Box(Vector3 position, Vector3 size, int color, Vector3 rotation = default(Vector3))
    {
        var mesh = Object.Instantiate(_cube);var uv = new Vector2[mesh.vertexCount];
        for(int i=0;i<uv.Length;i++)uv[i]=new Vector2((color+.5f)/8f,.5f);
        mesh.uv=uv;Temporary.Add(mesh);
        Parts.Add(new CombineInstance { mesh=mesh, transform=Matrix4x4.TRS(position,Quaternion.Euler(rotation),size) });
    }
    private static void Window(float x,float y,float z,float width=1.1f,float height=1.2f)
    {
        Box(new Vector3(x,y,z),new Vector3(width+.14f,height+.14f,.10f),3);
        Box(new Vector3(x,y,z-.065f),new Vector3(width,height,.05f),5);
        Box(new Vector3(x,y,z-.10f),new Vector3(.07f,height,.05f),6);
        Box(new Vector3(x,y,z-.10f),new Vector3(width,.07f,.05f),6);
    }
    private static void Apartments()
    {
        Box(new Vector3(0,3.7f,.5f),new Vector3(8,7.4f,5.8f),0);
        SideWindows(4,3,2.4f);
        for(int level=0;level<3;level++)
        {
            float y=1.25f+level*2.4f;
            for(int j=0;j<4;j++)Window(-2.8f+j*1.85f,y,-2.43f,1.2f,1.45f);
            Box(new Vector3(0,.12f+level*2.4f,-2.8f),new Vector3(8.4f,.18f,1.2f),3);
            if(level>0)
            {
                Box(new Vector3(0,.9f+level*2.4f,-3.35f),new Vector3(8.2f,.08f,.08f),6);
                for(int j=0;j<17;j++)Box(new Vector3(-4+j*.5f,.52f+level*2.4f,-3.35f),new Vector3(.055f,.8f,.055f),6);
            }
        }
        Box(new Vector3(0,1.1f,-2.55f),new Vector3(1.25f,2.2f,.16f),7);
        Box(new Vector3(0,7.45f,.3f),new Vector3(8.5f,.22f,6.7f),1);
        Box(new Vector3(2.35f,8.1f,1.1f),new Vector3(1.5f,1.2f,1.5f),6);
        Box(new Vector3(2.35f,8.74f,1.1f),new Vector3(1.65f,.12f,1.65f),6);
        Box(new Vector3(-3.9f,3.7f,-2.52f),new Vector3(.3f,7.4f,.25f),1);
        Box(new Vector3(3.9f,3.7f,-2.52f),new Vector3(.3f,7.4f,.25f),1);
    }
    private static void Compound()
    {
        Box(new Vector3(0,2.9f,.6f),new Vector3(7.5f,5.8f,5.2f),0);
        SideWindows(3.75f,2,2.8f);
        Box(new Vector3(-1.9f,3.1f,-2.12f),new Vector3(1,6.2f,.38f),1);
        Box(new Vector3(0,5.85f,.6f),new Vector3(8,.22f,5.7f),3);
        for(int level=0;level<2;level++)for(int j=0;j<3;j++)Window(-2.65f+j*2.55f,1.5f+level*2.8f,-2.04f,1.5f,1.6f);
        Box(new Vector3(.8f,3,-2.55f),new Vector3(3.8f,.18f,1.2f),3);
        Box(new Vector3(.8f,3.65f,-3.1f),new Vector3(3.8f,.75f,.12f),5);
        Box(new Vector3(0,1.1f,-2.15f),new Vector3(1.1f,2.2f,.18f),7);
        Box(new Vector3(0,.18f,-3),new Vector3(2.5f,.36f,1.4f),3);
        for(int side=-1;side<=1;side+=2)
        {
            Box(new Vector3(side*4.2f,.65f,.3f),new Vector3(.22f,1.3f,7.4f),3);
            Box(new Vector3(side*2.9f,.65f,-3.3f),new Vector3(2.8f,1.3f,.22f),3);
            Box(new Vector3(side*1.5f,.9f,-3.3f),new Vector3(.3f,1.8f,.4f),1);
        }
        Box(new Vector3(0,.7f,-3.3f),new Vector3(2.7f,1.4f,.12f),6);
        for(int j=0;j<8;j++)Box(new Vector3(-1.2f+j*.35f,.75f,-3.39f),new Vector3(.08f,1.3f,.08f),3);
    }
    private static void SideWindows(float halfWidth, int floors, float spacing)
    {
        for(int side=-1;side<=1;side+=2)for(int level=0;level<floors;level++)for(int j=0;j<2;j++)
        {
            var p=new Vector3(side*(halfWidth+.025f),1.45f+level*spacing,.2f+j*1.9f);
            Box(p,new Vector3(.08f,1.3f,1.25f),3);
            Box(p+Vector3.right*side*.045f,new Vector3(.035f,1.15f,1.1f),5);
            Box(p+Vector3.right*side*.07f,new Vector3(.035f,1.15f,.06f),6);
        }
    }
    private static void Gable(float z)
    {
        var mesh=new Mesh();
        mesh.vertices=new[]{new Vector3(-4,3,z),new Vector3(4,3,z),new Vector3(0,4.5f,z)};
        mesh.triangles=z<0?new[]{0,2,1}:new[]{0,1,2};
        mesh.uv=new[]{new Vector2(.0625f,.5f),new Vector2(.0625f,.5f),new Vector2(.0625f,.5f)};
        mesh.RecalculateNormals();Temporary.Add(mesh);Parts.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.identity});
    }
    public static void RefreshGeometry()
    {
        var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);_cube=primitive.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(primitive);
        for(int city=0;city<3;city++)
        {
            Parts.Clear();Temporary.Clear();
            if(city==0)Apartments();else if(city==1)Compound();else Bungalow();
            var mesh=new Mesh{name=Houses[city]};mesh.CombineMeshes(Parts.ToArray(),true,true);mesh.RecalculateBounds();
            var target=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+Houses[city]+".asset");EditorUtility.CopySerialized(mesh,target);EditorUtility.SetDirty(target);Object.DestroyImmediate(mesh);
            foreach(var m in Temporary)Object.DestroyImmediate(m);
        }
        AssetDatabase.SaveAssets();
    }
    [System.Serializable]
    public class HousingChange
    {
        public string city, asset, objectName, originalMesh;
        public Vector3 originalPosition, originalScale, originalEuler;
        public int oldTriangles, newTriangles;
    }
    [System.Serializable] public class HousingReport { public List<HousingChange> changes = new List<HousingChange>(); }
    public static readonly HousingReport Report = new HousingReport();

    public static int ApplyHousing(GameObject root, int city, string assetLabel)
    {
        if(root.transform.Find("CityHousing") != null) return 0;
        var targets = new List<Transform>();
        int eligible = 0;
        foreach(Transform child in root.transform)
        {
            // Only exact, previously inspected residential-name families. Shared anha/nha2,
            // landmarks, office buildings, roads and unknown objects are never candidates.
            string name = child.name;
            int suffix = name.LastIndexOf(" (");
            if(suffix >= 0 && name.EndsWith(")")) name = name.Substring(0,suffix);
            bool all = (city == 0 || city == 2) && name == "modern house abuja";
            bool family = city == 0 ? name == "house-2" || name == "modern house" || name == "beige two-story house 3d model" || name == "water-tank+rooftop+house+3d+model"
                : city == 1 ? name == "modern house 3d model" || name == "modern house abuja" || name == "residential house 3d model"
                : name == "house-2" || name == "mini+compound+house+3d+model" || name == "water-tank+rooftop+house+3d+model";
            if(!all && !family) continue;
            if(child.childCount != 0 || child.GetComponents<Component>().Length != 3 || child.GetComponent<MeshFilter>() == null || child.GetComponent<MeshRenderer>() == null) continue;
            if(all || eligible++ % 2 == 0) targets.Add(child);
        }
        if(targets.Count == 0) return 0;
        var group = new GameObject("CityHousing");
        Undo.RegisterCreatedObjectUndo(group,"City housing group"); group.transform.SetParent(root.transform,false);
        int replaced=0;
        foreach(var target in targets)
        {
            var filter=target.GetComponent<MeshFilter>();var renderer=target.GetComponent<MeshRenderer>();
            if(filter.sharedMesh == null) continue;
            var oldBounds = GeometryBounds(filter,root.transform);
            if(oldBounds.min.x < 4.1f && oldBounds.max.x > -4.1f) continue;
            string prefabPath="Assets/Prefab/CityScenery/"+Cities[city]+"/Housing/"+Houses[city]+"_"+(replaced%2)+".prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
            float yaw=oldBounds.center.x<0?-90:90;
            var rotation=Quaternion.Euler(0,yaw,0);
            var shape=mesh.bounds;
            float scale=Mathf.Min(oldBounds.size.x/shape.size.z,oldBounds.size.z/shape.size.x,oldBounds.size.y/shape.size.y)*.96f;
            if(scale <= .1f) continue;
            var change=new HousingChange{city=Cities[city],asset=assetLabel,objectName=target.name,originalMesh=AssetDatabase.GetAssetPath(filter.sharedMesh),originalPosition=target.localPosition,originalScale=target.localScale,originalEuler=target.localEulerAngles,oldTriangles=filter.sharedMesh.triangles.Length/3,newTriangles=mesh.triangles.Length/3};
            Undo.RecordObjects(new Object[]{target,filter,renderer},"Replace residential house visual");
            Undo.SetTransformParent(target,group.transform,"Group city house");
            Vector3 baseCenter=new Vector3(oldBounds.center.x,Mathf.Clamp(oldBounds.min.y,0,.25f),oldBounds.center.z);
            Vector3 offset=rotation*new Vector3(shape.center.x,shape.min.y,shape.center.z)*scale;
            target.localPosition=baseCenter-offset;target.localRotation=rotation;target.localScale=Vector3.one*scale;
            filter.sharedMesh=mesh;renderer.sharedMaterial=prefab.GetComponent<MeshRenderer>().sharedMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            target.name=Houses[city]+"_"+replaced;
            EditorUtility.SetDirty(filter);EditorUtility.SetDirty(renderer);
            Report.changes.Add(change);replaced++;
        }
        return replaced;
    }
    public static Bounds GeometryBounds(MeshFilter filter, Transform relative)
    {
        var vertices=filter.sharedMesh.vertices;
        var matrix=relative.worldToLocalMatrix*filter.transform.localToWorldMatrix;
        var bounds=new Bounds(matrix.MultiplyPoint3x4(vertices[0]),Vector3.zero);
        foreach(var vertex in vertices)bounds.Encapsulate(matrix.MultiplyPoint3x4(vertex));
        return bounds;
    }
    public static void SaveReport()
    {
        string path=Folder+"/HousingReplacementReport.json";
        File.WriteAllText(path,JsonUtility.ToJson(Report,true));AssetDatabase.ImportAsset(path);AssetDatabase.SaveAssets();
    }
    private static void Bungalow()
    {
        Box(new Vector3(0,1.5f,.5f),new Vector3(8,3,5.5f),0);
        Gable(-2.26f);Gable(3.26f);SideWindows(4,1,0);
        // Pitched roof: two planes thick enough to show eaves and ridge.
        Box(new Vector3(-2.25f,3.75f,.2f),new Vector3(4.9f,.16f,7.2f),1,new Vector3(0,0,19));
        Box(new Vector3(2.25f,3.75f,.2f),new Vector3(4.9f,.16f,7.2f),1,new Vector3(0,0,-19));
        Box(new Vector3(0,4.52f,.2f),new Vector3(.18f,.18f,7.2f),1);
        Box(new Vector3(0,.15f,-2.85f),new Vector3(8.6f,.3f,2),3);
        for(int j=0;j<4;j++)Box(new Vector3(-3.9f+j*2.6f,1.55f,-3.35f),new Vector3(.18f,2.9f,.18f),3);
        Box(new Vector3(0,2.85f,-3.35f),new Vector3(8.3f,.18f,.25f),3);
        for(int side=-1;side<=1;side+=2){Window(side*2.4f,1.65f,-2.29f,1.6f,1.35f);Box(new Vector3(side*2.7f,.8f,-3.35f),new Vector3(2.6f,.18f,.15f),7);}
        Box(new Vector3(0,1.1f,-2.35f),new Vector3(1.1f,2.2f,.18f),7);
        for(int j=0;j<2;j++)Box(new Vector3(0,.1f+j*.08f,-4+j*.3f),new Vector3(2.4f,.18f,.6f),3);
    }
}
