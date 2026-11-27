// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortfolioStudies
{
    public sealed class GameSession:MonoBehaviour
    {
        public static GameSession Instance{get;private set;}
        public Definition Def{get;private set;}public RunState State=RunState.Ready;
        public bool Playing{get{return State==RunState.Playing;}}
        public Vitals Stats=new Vitals();public Objectives Flags=new Objectives();
        public Camera View;public PlayerMotor Motor;public PlayerCombat Combat;public ActorHealth SelectedEnemy;
        public int Coins,Gems,Wood,Defeated;public float Elapsed,Wind=.6f;public bool ShowGrass=true,Sound;
        public string Message="";public IStudyRoom Room;public string Address="127.0.0.1";
        readonly Dictionary<int,GameObject> remotePlayers=new Dictionary<int,GameObject>();
        AudioSource audioSource;Light torch;float messageUntil;
        void Awake()
        {
            Instance=this;Def=ProjectDefinition.Create();UnityEngine.Random.InitState(Def.Seed);
            World.LightScene(Def.Mode==StudyMode.Horror?new Color(.04f,.05f,.08f):new Color(.48f,.65f,.78f),Def.Mode==StudyMode.Horror||Def.Mode==StudyMode.Castle||Def.Mode==StudyMode.Dungeon);
            var cameraObject=new GameObject("Player camera");cameraObject.tag="MainCamera";View=cameraObject.AddComponent<Camera>();View.nearClipPlane=.08f;View.farClipPlane=250;View.backgroundColor=RenderSettings.fogColor;View.clearFlags=CameraClearFlags.SolidColor;cameraObject.AddComponent<AudioListener>();
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=.16f;
            Vector3 spawn=Layouts.Build(this);var player=World.Body("Player",spawn,new Color(.1f,.68f,.82f));
            Motor=player.AddComponent<PlayerMotor>();Combat=player.AddComponent<PlayerCombat>();var health=player.AddComponent<ActorHealth>();health.Player=true;
            if(Def.Mode==StudyMode.Horror){Motor.OverShoulder=false;Combat.Ammo=new Magazine(8,24);}
            if(Def.Mode==StudyMode.Duel||Def.Mode==StudyMode.Castle||Def.Mode==StudyMode.ClickRpg)
            {World.Part(player,"Blade",PrimitiveType.Cube,new Vector3(.5f,1,.5f),new Vector3(.12f,.12f,1.3f),new Color(.7f,.75f,.8f));World.Part(player,"Shield",PrimitiveType.Cube,new Vector3(-.5f,1,.2f),new Vector3(.12f,.8f,.6f),TealShield());}
            if(Def.Mode==StudyMode.Horror||Def.Mode==StudyMode.Survival||Def.Mode==StudyMode.Shooter)
            {torch=new GameObject("Flashlight").AddComponent<Light>();torch.type=LightType.Spot;torch.range=28;torch.spotAngle=58;torch.intensity=2;torch.shadows=LightShadows.Soft;torch.transform.SetParent(View.transform,false);}
            if(Motor.FirstPerson)foreach(var renderer in player.GetComponentsInChildren<Renderer>())renderer.enabled=false;
            View.transform.position=spawn+new Vector3(0,6,-8);View.transform.LookAt(spawn+Vector3.up);
            Motor.ClickDestination=spawn;Message=Def.Objective;
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.M))Sound=!Sound;
            if(Input.GetKeyDown(KeyCode.F)&&torch!=null)torch.enabled=!torch.enabled;
            if(Input.GetKeyDown(KeyCode.Escape))
            {if(State==RunState.Playing){State=RunState.Paused;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}else if(State==RunState.Paused)Begin();}
            if(Room!=null)TickNetwork();
            if(!Playing)return;Elapsed+=Time.deltaTime;
            if(Def.Mode==StudyMode.Survival){var health=Motor.GetComponent<ActorHealth>();health.Current=Mathf.Min(health.Current,Stats.Health);if(health.Current<=0)Finish(false,"You did not survive.");}
            if(Input.GetKeyDown(KeyCode.E))InteractNearest();
            if(Def.Mode==StudyMode.Meadow)
            {if(Input.GetKeyDown(KeyCode.G))ShowGrass=!ShowGrass;if(Input.GetKeyDown(KeyCode.T))RenderSettings.sun.transform.Rotate(30,0,0);}
            if(Time.time>messageUntil)Message=Def.Objective;
        }
        public void InteractNearest(){Interactable nearest=null;float range=3.3f;foreach(var item in FindObjectsOfType<Interactable>()){float d=Vector3.Distance(item.transform.position,Motor.transform.position+Vector3.up);if(d<range){nearest=item;range=d;}}if(nearest!=null)nearest.Use();}
        public void Begin()
        {
            State=RunState.Playing;bool pointer=Motor.TopView||Motor.SideView;
#if UNITY_WEBGL && !UNITY_EDITOR
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
#else
            Cursor.lockState=pointer?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=pointer;
#endif
        }
        public void Finish(bool won,string reason){if(State==RunState.Won||State==RunState.Lost)return;State=won?RunState.Won:RunState.Lost;Message=reason;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        public void Notify(string text){Message=text;messageUntil=Time.time+3;}
        public ActorHealth NearestEnemy(Vector3 p,float range)
        {ActorHealth nearest=null;foreach(var h in FindObjectsOfType<ActorHealth>())if(!h.Player&&!h.Dead){float d=Vector3.Distance(p,h.transform.position);if(d<range){range=d;nearest=h;}}return nearest;}
        static Color TealShield(){return new Color(.15f,.5f,.55f);}
        public void EnemyDefeated(ActorHealth actor)
        {Defeated++;if(actor.Boss)Flags.Set("boss");if((Def.Mode==StudyMode.ClickRpg&&Defeated>=4)||(Def.Mode==StudyMode.Shooter&&Defeated>=6))Flags.Set("clear");if(Def.Mode==StudyMode.Duel)Finish(true,"Duel won.");Tone(440,.08f);}
        public void Flash(Vector3 p,Color color)
        {var g=World.Shape("Impact",PrimitiveType.Sphere,p,Vector3.one*.25f,color,false);Destroy(g,.1f);}
        public void Tone(float frequency,float duration)
        {
            if(!Sound)return;int samples=(int)(22050*duration);var data=new float[samples];for(int i=0;i<samples;i++)data[i]=Mathf.Sin(i*2*Mathf.PI*frequency/22050)*(1f-i/(float)samples)*.3f;
            var clip=AudioClip.Create("Synth effect",samples,1,22050,false);clip.SetData(data,0);audioSource.PlayOneShot(clip);Destroy(clip,duration+.3f);
        }
        public void Connect(bool host)
        {
            try{if(Room!=null)Room.Dispose();
#if UNITY_WEBGL && !UNITY_EDITOR
Room=new BrowserRoom();
#else
Room=new LanRoom();
#endif
if(host)Room.Host(7777);else Room.Join(Address,7777);Motor.Controller.enabled=false;Begin();}
            catch(Exception e){if(Room!=null)Room.Dispose();Room=null;Notify(e.Message);}
        }
        void TickNetwork()
        {
            Room.SetInput(Playing?Input.GetAxisRaw("Horizontal"):0,Playing?Input.GetAxisRaw("Vertical"):0);Room.Update(Time.unscaledDeltaTime);
            var present=new HashSet<int>();foreach(var pair in Room.Players)
            {
                var p=pair.Value;present.Add(p.Id);
                if(p.Id==Room.LocalId){Motor.transform.position=new Vector3(p.X,0,p.Z);Motor.transform.rotation=Quaternion.Euler(0,p.Yaw,0);continue;}
                GameObject actor;if(!remotePlayers.TryGetValue(p.Id,out actor)){actor=World.Body("Peer "+p.Id,Vector3.zero,Color.HSVToRGB((p.Id*.23f)%1,.7f,.9f));remotePlayers[p.Id]=actor;}
                actor.transform.position=Vector3.Lerp(actor.transform.position,new Vector3(p.X,0,p.Z),1-Mathf.Exp(-Time.unscaledDeltaTime*18));actor.transform.rotation=Quaternion.Euler(0,p.Yaw,0);
            }
            var gone=new List<int>();foreach(var pair in remotePlayers)if(!present.Contains(pair.Key))gone.Add(pair.Key);foreach(int id in gone){Destroy(remotePlayers[id]);remotePlayers.Remove(id);}
        }
        void OnDestroy(){if(Room!=null)Room.Dispose();if(Instance==this)Instance=null;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        void OnGUI()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(StudyWebBridge.Ready)return;
#endif
            if(Def==null)return;float scale=Mathf.Min(Screen.width/1100f,Screen.height/700f);var old=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));float width=Screen.width/scale,height=Screen.height/scale;
            GUI.skin.label.fontSize=18;GUI.skin.label.wordWrap=true;GUI.skin.button.fontSize=18;GUI.skin.box.fontSize=20;
            GUILayout.BeginArea(new Rect(20,16,width-40,130),GUI.skin.box);GUILayout.Label(Def.Title);GUILayout.Label(Message);GUILayout.Label("Health "+Mathf.CeilToInt(Stats.Health)+"  |  Ammo "+Combat.Ammo.Loaded+" / "+Combat.Ammo.Reserve+(Combat.Reloading?"  Reloading":"")+"  |  Tokens "+Coins+"  Gems "+Gems+"  |  Defeated "+Defeated);GUILayout.EndArea();
            if(Def.Mode==StudyMode.Survival){GUI.Box(new Rect(20,155,300,65),"Food "+(int)Stats.Food+"   Water "+(int)Stats.Water+"\nWood "+Wood+"   Time "+(int)Elapsed+" s");}
            if(Def.Mode==StudyMode.Infiltration||Def.Mode==StudyMode.Horror)
            {GUI.Box(new Rect(20,155,350,70),"Key: "+Flags.Has("key")+"  Terminal: "+Flags.Has("terminal")+"\nGenerators: "+Flags.Has("gen-a")+" / "+Flags.Has("gen-b"));}
            if(Playing)
            {
                GUI.Label(new Rect(width/2-8,height/2-16,30,30),"+");GUI.Label(new Rect(20,height-55,width-40,45),"Esc pause  |  E interact  |  R reload  |  Tab cursor  |  M sound "+(Sound?"on":"off"));
                if(Def.Mode==StudyMode.Meadow){GUI.Label(new Rect(20,155,300,30),"Wind");Wind=GUI.HorizontalSlider(new Rect(95,165,180,20),Wind,0,2);}
                var items=FindObjectsOfType<Interactable>();foreach(var item in items)if(Vector3.Distance(item.transform.position,Motor.transform.position+Vector3.up)<3.3f){GUI.Box(new Rect(width/2-230,height-120,460,45),"E — "+item.Label);break;}
            }
            else
            {
                GUILayout.BeginArea(new Rect(width/2-250,height/2-140,500,320),GUI.skin.box);
                GUILayout.Label(State==RunState.Won?"COMPLETE":State==RunState.Lost?"TRY AGAIN":State==RunState.Paused?"PAUSED":"READY");GUILayout.Label(Def.Instructions);GUILayout.Space(14);
                if(State==RunState.Won||State==RunState.Lost){if(GUILayout.Button("Restart"))SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
                else if(Def.Mode==StudyMode.Multiplayer&&Room==null){GUILayout.Label("Host address");Address=GUILayout.TextField(Address,64);if(GUILayout.Button("Host LAN room"))Connect(true);if(GUILayout.Button("Join room"))Connect(false);}
                else if(GUILayout.Button(State==RunState.Paused?"Resume":"Start"))Begin();
                GUILayout.Label("Sound "+(Sound?"on":"off")+" — press M to toggle");GUILayout.EndArea();
            }
            if(Room!=null)GUI.Label(new Rect(20,height-85,width-40,28),Room.Status+" | Players "+Room.Players.Count+" | Room port "+Room.Port);
            GUI.matrix=old;
        }
    }



}
