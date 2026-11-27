// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class PlayerMotor : MonoBehaviour
    {
        public CharacterController Controller { get; private set; }
        public Vector3 ClickDestination;
        public float Vertical, Yaw, Pitch = 18;
        public bool Blocking;
        public bool SideView { get { return Session.Def.Mode == StudyMode.SideRunner || Session.Def.Mode == StudyMode.Castle; } }
        public bool TopView { get { return Session.Def.Mode == StudyMode.ClickRpg || Session.Def.Mode == StudyMode.Dungeon; } }
        public bool FirstPerson { get { return Session.Def.Mode == StudyMode.Survival || Session.Def.Mode == StudyMode.Meadow || Session.Def.Mode == StudyMode.Shooter; } }
        public bool OverShoulder = true;
        GameSession Session { get { return GameSession.Instance; } }
        Vector3 dashDirection;
        float dashTime, dashCooldown, facing = 1;
        int jumps;
        public Vector3 Aim { get; private set; } = Vector3.forward;
        void Awake()
        {
            Controller=gameObject.AddComponent<CharacterController>();Controller.height=1.8f;Controller.radius=.34f;Controller.center=Vector3.up*.9f;Controller.stepOffset=.3f;Controller.slopeLimit=55;
            gameObject.layer=World.Actor;ClickDestination=transform.position;
        }
        void Update()
        {
            var s=Session;if(s==null||!s.Playing)return;
            var mode=s.Def.Mode;if(mode==StudyMode.Multiplayer)return;
            if(Input.GetKeyDown(KeyCode.Tab)&&FirstPerson){bool locked=Cursor.lockState==CursorLockMode.Locked;Cursor.lockState=locked?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=locked;}
            if(Input.GetKeyDown(KeyCode.C)&&mode==StudyMode.Horror)OverShoulder=!OverShoulder;
            Vector3 movement=Vector3.zero;
            float h=Input.GetAxisRaw("Horizontal"),v=Input.GetAxisRaw("Vertical");
            if(SideView)
            {
                movement=Vector3.right*h;if(Mathf.Abs(h)>.01f)facing=Mathf.Sign(h);Aim=Vector3.right*facing;
                transform.rotation=Quaternion.LookRotation(Aim);var p=transform.position;p.z=0;transform.position=p;
            }
            else if(TopView)
            {
                RaycastHit hit;Ray ray=s.View.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray,out hit,300,1<<World.Ground)){Vector3 aim=hit.point-transform.position;aim.y=0;if(aim.sqrMagnitude>.02f)Aim=aim.normalized;}
                if(mode==StudyMode.ClickRpg)
                {
                    if(Input.GetMouseButtonDown(0)&&Physics.Raycast(ray,out hit,300,(1<<World.Ground)|(1<<World.Actor)))
                    {
                        var health=hit.collider.GetComponentInParent<ActorHealth>();
                        s.SelectedEnemy=health!=null&&!health.Player?health:null;
                        ClickDestination=hit.point;ClickDestination.y=transform.position.y;
                    }
                    Vector3 d=ClickDestination-transform.position;d.y=0;
                    if(s.SelectedEnemy!=null){d=s.SelectedEnemy.transform.position-transform.position;d.y=0;if(d.magnitude<1.9f)d=Vector3.zero;}
                    movement=d.magnitude>.18f?d.normalized:Vector3.zero;
                }
                else movement=new Vector3(h,0,v).normalized;
                if(Aim.sqrMagnitude>.1f)transform.rotation=Quaternion.LookRotation(Aim);
            }
            else
            {
                if(Cursor.lockState==CursorLockMode.Locked||Input.GetMouseButton(1)){Yaw+=Input.GetAxis("Mouse X")*2.2f;Pitch=Mathf.Clamp(Pitch-Input.GetAxis("Mouse Y")*1.8f,-35,70);}
                movement=Quaternion.Euler(0,Yaw,0)*new Vector3(h,0,v).normalized;
                Aim=Quaternion.Euler(0,Yaw,0)*Vector3.forward;
                if(mode==StudyMode.Duel)
                {
                    var target=s.NearestEnemy(transform.position,30);if(target!=null){Vector3 face=target.transform.position-transform.position;face.y=0;Aim=face.normalized;}
                }
                if(FirstPerson||mode==StudyMode.Duel||mode==StudyMode.Horror)transform.rotation=Quaternion.LookRotation(Aim);
                else if(movement.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(movement),Time.deltaTime*14);
            }
            Blocking=mode==StudyMode.Duel&&Input.GetMouseButton(1)&&s.Stats.Stamina>1;
            bool sprint=Input.GetKey(KeyCode.LeftShift)&&s.Stats.Stamina>3&&movement.sqrMagnitude>.1f&&!SideView&&!Blocking;
            if(mode!=StudyMode.Survival)s.Stats.Stamina=Vitals.Clamp(s.Stats.Stamina+Time.deltaTime*(sprint?-16:Blocking?-7:15),0,100);
            bool grounded=Controller.isGrounded;
            if(grounded){if(Vertical<0)Vertical=-2;jumps=0;}
            bool canJump=mode!=StudyMode.Horror&&mode!=StudyMode.ClickRpg&&mode!=StudyMode.Dungeon;
            if(canJump&&Input.GetButtonDown("Jump")&&(grounded||(mode==StudyMode.Castle&&s.Flags.Has("double-jump")&&jumps<2))){Vertical=9.2f;jumps++;}
            dashCooldown-=Time.deltaTime;
            if((mode==StudyMode.Dungeon||(mode==StudyMode.Castle&&s.Flags.Has("dash")))&&Input.GetKeyDown(KeyCode.LeftShift)&&dashCooldown<=0)
            {dashDirection=movement.sqrMagnitude>.01f?movement:Aim;dashTime=.16f;dashCooldown=.9f;}
            float speed=Blocking?2.2f:sprint?8:5;
            if(dashTime>0){dashTime-=Time.deltaTime;movement=dashDirection*3.5f;}
            Vertical-=21*Time.deltaTime;
            Vector3 step=movement*speed*Time.deltaTime+Vector3.up*Vertical*Time.deltaTime;
            RaycastHit groundHit;
            if(grounded&&Physics.Raycast(transform.position+Vector3.up*.2f,Vector3.down,out groundHit,.6f,1<<World.Ground))
            {var platform=groundHit.collider.GetComponent<MovingPlatform>();if(platform!=null)step+=platform.Delta;}
            Controller.Move(step);
            if(transform.position.y<-18)s.Finish(false,"You fell. Try a different route.");
            if(mode==StudyMode.Survival){s.Stats.Tick(Time.deltaTime,sprint);if(s.Stats.Health<=0)s.Finish(false,"Survival supplies exhausted.");}
        }
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var box=hit.collider.GetComponent<PrizeBlock>();if(box!=null&&hit.normal.y<-.4f)box.Open();
            var target=hit.collider.GetComponent<ActorHealth>();
            if(SideView&&target!=null&&!target.Player&&Vertical<0&&hit.normal.y>.4f){target.Damage(100,transform.position);Vertical=7;}
        }
        void LateUpdate()
        {
            var s=Session;if(s==null||s.View==null)return;
            Vector3 p=transform.position;Camera camera=s.View;
            if(SideView){camera.transform.position=Vector3.Lerp(camera.transform.position,new Vector3(p.x+3,5,-18),1-Mathf.Exp(-Time.deltaTime*7));camera.transform.rotation=Quaternion.Euler(8,0,0);camera.orthographic=true;camera.orthographicSize=6;}
            else if(TopView){camera.transform.position=Vector3.Lerp(camera.transform.position,p+new Vector3(0,17,-12),1-Mathf.Exp(-Time.deltaTime*8));camera.transform.rotation=Quaternion.Euler(55,0,0);}
            else if(s.Def.Mode==StudyMode.Horror&&!OverShoulder)
            {
                Vector3 focus=p.z<8?new Vector3(-2,1,-1):new Vector3(1,1,18);Vector3 anchor=p.z<8?new Vector3(-12,11,-12):new Vector3(12,10,27);
                camera.transform.position=anchor;camera.transform.LookAt(focus);
            }
            else if(FirstPerson){camera.transform.position=p+Vector3.up*1.6f;camera.transform.rotation=Quaternion.Euler(Pitch,Yaw,0);}
            else
            {
                Quaternion rotation=Quaternion.Euler(Pitch,Yaw,0);Vector3 focus=p+Vector3.up*1.4f;Vector3 desired=focus-rotation*Vector3.forward*6+rotation*Vector3.right*.55f;RaycastHit hit;
                if(Physics.SphereCast(focus,.2f,(desired-focus).normalized,out hit,Vector3.Distance(desired,focus),1<<World.Ground))desired=hit.point+hit.normal*.25f;
                camera.transform.position=Vector3.Lerp(camera.transform.position,desired,1-Mathf.Exp(-Time.deltaTime*12));camera.transform.rotation=rotation;
            }
        }
    }
}
