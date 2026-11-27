// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class EnemyBrain:MonoBehaviour
    {
        public bool Ranged,Drone,Boss,Dormant,Side;public float Speed=2.3f,AttackDamage=9;public Vector3 Home;
        CharacterController motor;ActorHealth health;float nextAttack,vertical;Vector3 patrol;
        void Start(){health=GetComponent<ActorHealth>();Home=transform.position;patrol=Home;motor=gameObject.AddComponent<CharacterController>();motor.center=Vector3.up*.9f;motor.height=1.8f;motor.radius=.4f;gameObject.layer=World.Actor;}
        void Update()
        {
            var s=GameSession.Instance;if(s==null||!s.Playing||health.Dead)return;if(Dormant&&s.Motor.transform.position.z<5)return;
            Vector3 d=s.Motor.transform.position-transform.position;float distance=d.magnitude;Vector3 flat=d;flat.y=0;
            bool canSee=false;RaycastHit sight;
            Vector3 eye=transform.position+Vector3.up*1.2f,target=s.Motor.transform.position+Vector3.up;
            if(distance<28&&Physics.Raycast(eye,(target-eye).normalized,out sight,40,(1<<World.Ground)|(1<<World.Actor)))canSee=sight.collider.GetComponentInParent<PlayerMotor>()!=null;
            Vector3 movement=Vector3.zero;
            if(distance<22&&(canSee||distance<3))
            {
                float range=Ranged?11:1.8f;if(flat.magnitude>range)movement=flat.normalized*Speed;
                else if(Time.time>=nextAttack&&canSee)
                {
                    nextAttack=Time.time+(Boss?.7f:Ranged?1.2f:1.0f);
                    if(Ranged)
                    {for(int i=0;i<(Boss?3:1);i++)Projectile.Spawn(eye+flat.normalized*.7f,Quaternion.Euler(0,(i-1)*(Boss?12:0),0)*(target-eye).normalized,Drone?12:9,AttackDamage,false);}
                    else s.Motor.GetComponent<ActorHealth>().Damage(AttackDamage,transform.position);
                }
            }
            else
            {
                if((patrol-transform.position).sqrMagnitude<1)patrol=Home+new Vector3(Random.Range(-3f,3f),0,Side?0:Random.Range(-3f,3f));
                movement=(patrol-transform.position);movement.y=0;movement=movement.normalized*Speed*.35f;
            }
            if(Side)movement.z=0;
            if(movement.sqrMagnitude>.1f)
            {
                Vector3 rayOrigin=transform.position+Vector3.up*.8f;
                if(Physics.SphereCast(rayOrigin,.3f,movement.normalized,out sight,.9f,1<<World.Ground))movement=Vector3.Cross(Vector3.up,movement).normalized*Speed;
                if(Side)movement.z=0;transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(flat.sqrMagnitude>.1f?flat:movement),Time.deltaTime*8);
            }
            if(Drone){movement.y=Mathf.Sin(Time.time*2)*.4f;vertical=0;}else{if(motor.isGrounded&&vertical<0)vertical=-2;vertical-=20*Time.deltaTime;movement.y=vertical;}
            motor.Move(movement*Time.deltaTime);
        }
    }
}
