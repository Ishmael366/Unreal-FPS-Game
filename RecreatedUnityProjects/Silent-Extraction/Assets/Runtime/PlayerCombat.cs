// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class PlayerCombat:MonoBehaviour
    {
        public Magazine Ammo=new Magazine(18,90);public int Weapon;float nextShot,reloadUntil;public bool Reloading{get{return reloadUntil>0;}}
        void Update()
        {
            var s=GameSession.Instance;if(s==null||!s.Playing||s.Def.Mode==StudyMode.Multiplayer)return;
            var mode=s.Def.Mode;bool melee=mode==StudyMode.Duel||mode==StudyMode.Castle||mode==StudyMode.ClickRpg;
            if(reloadUntil>0&&Time.time>=reloadUntil){Ammo.Reload();reloadUntil=0;}
            if(Input.GetKeyDown(KeyCode.R)&&!Reloading&&Ammo.Loaded<Ammo.Capacity&&Ammo.Reserve>0)reloadUntil=Time.time+1.3f;
            if(mode==StudyMode.Dungeon){if(Input.GetKeyDown(KeyCode.Alpha1))Weapon=0;if(Input.GetKeyDown(KeyCode.Alpha2))Weapon=1;if(Input.GetKeyDown(KeyCode.Alpha3))Weapon=2;}
            if(mode==StudyMode.ClickRpg&&s.SelectedEnemy!=null)
            {if(Vector3.Distance(s.SelectedEnemy.transform.position,transform.position)<2.1f&&Time.time>=nextShot)Strike(s);return;}
            if(!Input.GetMouseButton(0)||Time.time<nextShot||Reloading)return;
            if(melee){Strike(s);return;}
            if(mode==StudyMode.Meadow||mode==StudyMode.SkyIslands||mode==StudyMode.SideRunner||mode==StudyMode.Survival)return;
            if(!Ammo.Fire()){s.Message="Out of ammunition — press R to reload.";return;}
            nextShot=Time.time+(Weapon==1?.7f:Weapon==2?.1f:.25f);int pellets=Weapon==1?6:1;
            s.Tone(Weapon==1?90:180,.05f);
            for(int i=0;i<pellets;i++)
            {
                Vector3 origin,direction;
                if(mode==StudyMode.Dungeon){origin=transform.position+Vector3.up+ s.Motor.Aim*.8f;direction=Quaternion.Euler(0,Random.Range(-5,5),0)*s.Motor.Aim;Projectile.Spawn(origin,direction,24,Weapon==1?12:24,true);}
                else
                {
                    Ray ray=s.View.ViewportPointToRay(new Vector3(.5f,.5f,0));origin=ray.origin;direction=ray.direction;
                    if(mode==StudyMode.Horror&&!s.Motor.OverShoulder){origin=transform.position+Vector3.up*1.3f;direction=s.Motor.Aim;}
                    var hits=Physics.RaycastAll(origin,direction,100,(1<<World.Ground)|(1<<World.Actor));System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                    foreach(var hit in hits){var target=hit.collider.GetComponentInParent<ActorHealth>();if(target!=null&&target.Player)continue;if(target!=null)target.Damage(25,transform.position);s.Flash(hit.point,Color.yellow);break;}
                }
            }
        }
        void Strike(GameSession s)
        {
            nextShot=Time.time+.45f;s.Tone(260,.05f);var enemies=Object.FindObjectsOfType<ActorHealth>();
            foreach(var e in enemies)if(!e.Player&&!e.Dead){Vector3 d=e.transform.position-transform.position;d.y=0;if(d.magnitude<2.6f&&Vector3.Dot(d.normalized,s.Motor.Aim)>.25f)e.Damage(s.Def.Mode==StudyMode.Duel?18:28,transform.position);}
            s.Flash(transform.position+Vector3.up+s.Motor.Aim,Color.cyan);
        }
    }
}
