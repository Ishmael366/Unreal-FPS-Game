using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortfolioStudies
{
    public interface IStudyRoom:IDisposable
    {
        int LocalId{get;} int Port{get;} bool Connected{get;} string Status{get;}
        IReadOnlyDictionary<int,PeerState> Players{get;}
        void Host(int port);void Join(string address,int port);void SetInput(float x,float z);void Update(float dt);
    }
    [Serializable] public sealed class WebState
    {
        public string title,mode,state,message,instructions,prompt,network,invite,answer;
        public float health,food,water,stamina,time,wind;public int ammo,reserve,coins,gems,wood,defeated,players;
        public bool sound,pointer,grass;
    }
    public sealed class StudyWebBridge:MonoBehaviour
    {
        public static bool Ready;float nextUpdate;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]static extern void StudyWebStatus(string json);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Ensure(){if(FindObjectOfType<StudyWebBridge>()!=null)return;var o=new GameObject("Study Web Bridge");o.AddComponent<StudyWebBridge>();DontDestroyOnLoad(o);WebGLInput.captureAllKeyboardInput=false;}
#endif
        void Awake(){Ready=true;}
        void OnDestroy(){Ready=false;}
        void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var s=GameSession.Instance;if(s==null||s.Motor==null||Time.unscaledTime<nextUpdate)return;nextUpdate=Time.unscaledTime+.15f;
            var st=new WebState{title=s.Def.Title,mode=s.Def.Mode.ToString(),state=s.State.ToString(),message=s.Message,instructions=s.Def.Instructions,health=s.Stats.Health,food=s.Stats.Food,water=s.Stats.Water,stamina=s.Stats.Stamina,time=s.Elapsed,wind=s.Wind,ammo=s.Combat.Ammo.Loaded,reserve=s.Combat.Ammo.Reserve,coins=s.Coins,gems=s.Gems,wood=s.Wood,defeated=s.Defeated,sound=s.Sound,pointer=!s.Motor.SideView&&!s.Motor.TopView,grass=s.ShowGrass,network=s.Room==null?"":s.Room.Status,players=s.Room==null?0:s.Room.Players.Count};
            foreach(var item in FindObjectsOfType<Interactable>())if(Vector3.Distance(item.transform.position,s.Motor.transform.position+Vector3.up)<3.3f){st.prompt=item.Label;break;}
            var room=s.Room as BrowserRoom;if(room!=null){st.invite=room.Invite;st.answer=room.Answer;}
            StudyWebStatus(JsonUtility.ToJson(st));
#endif
        }
        public void StartGame(){var s=GameSession.Instance;if(s!=null)s.Begin();}
        public void PauseGame(){var s=GameSession.Instance;if(s==null)return;s.State=RunState.Paused;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        public void Restart(){SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
        public void Interact(){var s=GameSession.Instance;if(s!=null)s.InteractNearest();}
        public void ToggleSound(){var s=GameSession.Instance;if(s!=null)s.Sound=!s.Sound;}
        public void SetWind(string value){float f;if(float.TryParse(value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out f))GameSession.Instance.Wind=Mathf.Clamp(f,0,2);}
        public void HostRoom(){GameSession.Instance.Connect(true);}
        public void JoinRoom(string invitation){GameSession.Instance.Address=invitation;GameSession.Instance.Connect(false);}
        public void AcceptAnswer(string answer){var room=GameSession.Instance.Room as BrowserRoom;if(room!=null)room.AcceptAnswer(answer);}
    }
    [Serializable]public sealed class BrowserRoomState{public int localId=-1;public string status,invite,answer;public PeerState[] players;}
    public sealed class BrowserRoomReceiver:MonoBehaviour
    {
        public BrowserRoom Owner;
        public void Receive(string json){if(Owner!=null)Owner.Receive(json);}
    }
    public sealed class BrowserRoom:IStudyRoom
    {
        readonly Dictionary<int,PeerState> players=new Dictionary<int,PeerState>();BrowserRoomReceiver receiver;
        public int LocalId{get;private set;}=-1;public int Port{get{return 0;}}public bool Connected{get{return Status=="Connected";}}
        public string Status{get;private set;}="Not connected";public string Invite="",Answer="";
        public IReadOnlyDictionary<int,PeerState> Players{get{return players;}}
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]static extern void StudyNetHost(string receiver);
        [DllImport("__Internal")]static extern void StudyNetJoin(string receiver,string invitation);
        [DllImport("__Internal")]static extern void StudyNetAnswer(string answer);
        [DllImport("__Internal")]static extern void StudyNetInput(float x,float z);
        [DllImport("__Internal")]static extern void StudyNetDispose();
#endif
        void Receiver(){var o=new GameObject("Study Network Receiver");receiver=o.AddComponent<BrowserRoomReceiver>();receiver.Owner=this;}
        public void Host(int port){Dispose();Receiver();Status="Creating invitation";
#if UNITY_WEBGL && !UNITY_EDITOR
            StudyNetHost(receiver.gameObject.name);
#endif
        }
        public void Join(string invitation,int port){Dispose();Receiver();Status="Connecting";
#if UNITY_WEBGL && !UNITY_EDITOR
            StudyNetJoin(receiver.gameObject.name,invitation);
#endif
        }
        public void AcceptAnswer(string answer){
#if UNITY_WEBGL && !UNITY_EDITOR
            StudyNetAnswer(answer);
#endif
        }
        public void SetInput(float x,float z){
#if UNITY_WEBGL && !UNITY_EDITOR
            StudyNetInput(x,z);
#endif
        }
        public void Update(float dt){}
        public void Receive(string json)
        {
            var data=JsonUtility.FromJson<BrowserRoomState>(json);if(data==null)return;LocalId=data.localId;Status=data.status??"";Invite=data.invite??"";Answer=data.answer??"";players.Clear();if(data.players!=null)foreach(var p in data.players)players[p.Id]=p;
        }
        public void Dispose(){
#if UNITY_WEBGL && !UNITY_EDITOR
            StudyNetDispose();
#endif
            if(receiver!=null){receiver.Owner=null;UnityEngine.Object.Destroy(receiver.gameObject);}receiver=null;players.Clear();LocalId=-1;Status="Not connected";Invite=Answer="";
        }
    }
}
