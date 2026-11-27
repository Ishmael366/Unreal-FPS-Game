// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System.Collections.Generic;
using UnityEngine;

namespace PortfolioStudies
{
    public static class World
    {
        public const int Ground = 8, Actor = 9;
        static readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        public static Material Mat(Color color)
        {
            Material m;
            if (!materials.TryGetValue(color, out m) || m == null)
            {
                m = new Material(Shader.Find("Standard")); m.color = color; m.SetFloat("_Glossiness", .2f); materials[color] = m;
            }
            return m;
        }
        public static GameObject Shape(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, bool solid = true)
        {
            var g = GameObject.CreatePrimitive(primitive); g.name = name; g.transform.position = position; g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(color); g.layer = solid ? Ground : 2;
            if (!solid) { var c = g.GetComponent<Collider>(); c.enabled = false; Object.Destroy(c); }
            return g;
        }
        public static GameObject Box(string name, Vector3 p, Vector3 size, Color color, bool solid = true) { return Shape(name, PrimitiveType.Cube, p, size, color, solid); }
        public static void Floor(Vector3 center, Vector3 size, Color color) { Box("Ground", center - Vector3.up * .5f, new Vector3(size.x, 1, size.z), color); }
        public static GameObject Body(string name, Vector3 p, Color color, float size = 1)
        {
            var root = new GameObject(name); root.transform.position = p;
            Part(root, "Torso", PrimitiveType.Capsule, new Vector3(0,.85f,0), new Vector3(.65f,.65f,.48f) * size, color);
            Part(root, "Head", PrimitiveType.Sphere, new Vector3(0,1.6f,0), Vector3.one * .42f * size, Color.Lerp(color, Color.white, .5f));
            Part(root, "Visor", PrimitiveType.Cube, new Vector3(0,1.65f,.21f), new Vector3(.32f,.13f,.08f), new Color(.1f,.9f,1));
            Part(root, "Boot L", PrimitiveType.Cube, new Vector3(-.18f,.16f,0), new Vector3(.22f,.32f,.35f), color * .5f);
            Part(root, "Boot R", PrimitiveType.Cube, new Vector3(.18f,.16f,0), new Vector3(.22f,.32f,.35f), color * .5f);
            return root;
        }
        public static GameObject Part(GameObject parent, string name, PrimitiveType type, Vector3 p, Vector3 scale, Color color)
        {
            var g = Shape(name, type, Vector3.zero, scale, color, false); g.transform.SetParent(parent.transform, false); g.transform.localPosition = p; return g;
        }
        public static void LightScene(Color sky, bool dark = false)
        {
            RenderSettings.fog = true; RenderSettings.fogColor = sky; RenderSettings.fogDensity = dark ? .025f : .009f;
            RenderSettings.ambientLight = dark ? new Color(.16f,.18f,.27f) : new Color(.42f,.47f,.56f);
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(40,-30,0);
            sun.color = new Color(1,.88f,.68f); sun.intensity = dark ? .35f : 1.15f; sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun; QualitySettings.shadowDistance = 90;
        }
        public static float Height(float x, float z) { return Mathf.PerlinNoise((x + 137) * .035f, (z + 89) * .035f) * 4.5f - 1.3f; }
        public static void Landscape(bool grass)
        {
            const int n = 97; var vertices = new Vector3[n*n]; var uv = new Vector2[n*n]; var triangles = new int[(n-1)*(n-1)*6]; int t = 0;
            for (int z=0;z<n;z++) for(int x=0;x<n;x++)
            {
                int i=z*n+x; float px=(x-48)*1.25f, pz=(z-48)*1.25f; vertices[i]=new Vector3(px,Height(px,pz),pz); uv[i]=new Vector2(x/8f,z/8f);
                if(x<n-1&&z<n-1){triangles[t++]=i;triangles[t++]=i+n;triangles[t++]=i+1;triangles[t++]=i+1;triangles[t++]=i+n;triangles[t++]=i+n+1;}
            }
            var mesh = new Mesh { name="Rolling meadow", vertices=vertices, triangles=triangles, uv=uv }; mesh.RecalculateNormals();
            var g=new GameObject("Landscape");g.layer=Ground;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=Mat(new Color(.24f,.34f,.16f));g.AddComponent<MeshCollider>().sharedMesh=mesh;
            for(int i=0;i<75;i++)
            {
                float x=Random.Range(-52f,52f),z=Random.Range(-52f,52f); if(new Vector2(x,z).magnitude<8)continue; float y=Height(x,z);
                if(i%3==0) Shape("Weathered stone",PrimitiveType.Sphere,new Vector3(x,y+.35f,z),new Vector3(Random.Range(1f,3f),1.3f,1.4f),new Color(.37f,.39f,.36f));
                else Tree(new Vector3(x,y,z),Random.Range(3f,6f));
            }
            if(grass) for(int z=-2;z<=2;z++)for(int x=-2;x<=2;x++)GrassPatch(new Vector3(x*20,0,z*20));
        }
        public static void Tree(Vector3 p,float height)
        {
            Shape("Tree trunk",PrimitiveType.Cylinder,p+Vector3.up*height*.3f,new Vector3(.45f,height*.3f,.45f),new Color(.23f,.15f,.1f));
            Shape("Tree crown",PrimitiveType.Sphere,p+Vector3.up*height*.8f,new Vector3(height*.65f,height*.9f,height*.6f),new Color(.12f,.26f,.18f),false);
        }
        static void GrassPatch(Vector3 center)
        {
            var vs=new List<Vector3>();var uv=new List<Vector2>();var ts=new List<int>();
            for(int b=0;b<300;b++)
            {
                float x=center.x+Random.Range(-10f,10f),z=center.z+Random.Range(-10f,10f),h=Random.Range(.3f,.85f),w=.07f;
                if(Mathf.Abs(x)<1.5f)continue;
                for(int c=0;c<2;c++)
                {
                    Vector3 p=new Vector3(x,Height(x,z),z);Vector3 right=c==0?Vector3.right*w:Vector3.forward*w;int k=vs.Count;
                    vs.Add(p-right);vs.Add(p+right);vs.Add(p+Vector3.up*h+right*.25f);vs.Add(p+Vector3.up*h-right*.25f);
                    uv.Add(new Vector2(0,0));uv.Add(new Vector2(1,0));uv.Add(new Vector2(1,1));uv.Add(new Vector2(0,1));
                    ts.Add(k);ts.Add(k+2);ts.Add(k+1);ts.Add(k);ts.Add(k+3);ts.Add(k+2);
                }
            }
            var mesh=new Mesh{name="Wind grass",vertices=vs.ToArray(),triangles=ts.ToArray(),uv=uv.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();
            var g=new GameObject("Wind grass patch");g.layer=2;g.AddComponent<MeshFilter>().sharedMesh=mesh;
            Shader shader=Shader.Find("Portfolio/WindGrass");var m=new Material(shader==null?Shader.Find("Standard"):shader);m.SetColor("_BottomColor",new Color(.08f,.18f,.05f));m.SetColor("_TopColor",new Color(.46f,.63f,.2f));g.AddComponent<MeshRenderer>().sharedMaterial=m;g.AddComponent<GrassWind>();
        }
        public static void Pillar(Vector3 p,float h,Color color)
        {
            Shape("Pillar",PrimitiveType.Cylinder,p+Vector3.up*h*.5f,new Vector3(.9f,h*.5f,.9f),color);
            Box("Capital",p+Vector3.up*h,new Vector3(1.4f,.3f,1.4f),color);
        }
        public static void Room(Vector3 center,float width,float depth,Color color,bool doorway=true)
        {
            Floor(center,new Vector3(width,1,depth),color*.7f);
            Box("North wall",center+new Vector3(0,2.5f,depth*.5f),new Vector3(width,5,.7f),color);
            Box("West wall",center+new Vector3(-width*.5f,2.5f,0),new Vector3(.7f,5,depth),color);
            Box("East wall",center+new Vector3(width*.5f,2.5f,0),new Vector3(.7f,5,depth),color);
            if(!doorway)Box("South wall",center+new Vector3(0,2.5f,-depth*.5f),new Vector3(width,5,.7f),color);
            else foreach(int sign in new[]{-1,1})Box("Door wing",center+new Vector3(sign*(width+3)*.25f,2.5f,-depth*.5f),new Vector3((width-3)*.5f,5,.7f),color);
        }
    }


}
