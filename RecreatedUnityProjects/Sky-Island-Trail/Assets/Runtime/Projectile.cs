// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class Projectile:MonoBehaviour
    {
        Vector3 direction;float speed,damage,life=4;bool friendly;
        public static void Spawn(Vector3 origin,Vector3 aim,float speed,float damage,bool friendly)
        {
            var g=World.Shape("Energy bolt",PrimitiveType.Sphere,origin,Vector3.one*.18f,friendly?Color.cyan:new Color(1,.3f,.1f),false);
            var p=g.AddComponent<Projectile>();p.direction=aim.normalized;p.speed=speed;p.damage=damage;p.friendly=friendly;
        }
        void Update()
        {
            var s=GameSession.Instance;if(s==null||!s.Playing)return;float step=speed*Time.deltaTime;RaycastHit hit;
            if(Physics.Raycast(transform.position,direction,out hit,step+.1f,(1<<World.Ground)|(1<<World.Actor)))
            {var h=hit.collider.GetComponentInParent<ActorHealth>();if(h!=null&&h.Player!=friendly)h.Damage(damage,transform.position-direction);s.Flash(hit.point,friendly?Color.cyan:Color.red);Destroy(gameObject);return;}
            transform.position+=direction*step;life-=Time.deltaTime;if(life<=0)Destroy(gameObject);
        }
    }
}
