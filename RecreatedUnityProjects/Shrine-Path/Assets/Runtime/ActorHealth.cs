// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class ActorHealth:MonoBehaviour
    {
        public bool Player,Boss;public float Maximum=100,Current=100;public bool Dead{get{return Current<=0;}}
        public void Damage(float amount,Vector3 attacker)
        {
            if(Dead||amount<=0)return;var s=GameSession.Instance;if(s==null||!s.Playing)return;
            if(Player&&s.Motor.Blocking&&s.Stats.Stamina>0&&Vector3.Dot((attacker-transform.position).normalized,s.Motor.Aim)>.1f){amount*=.08f;s.Stats.Stamina=Mathf.Max(0,s.Stats.Stamina-10);}
            Current=Mathf.Max(0,Current-amount);if(Player)s.Stats.Health=Current;
            s.Flash(transform.position+Vector3.up,Player?new Color(1,.3f,.2f):Color.yellow);
            if(Dead){if(Player)s.Finish(false,"Health depleted.");else{s.EnemyDefeated(this);gameObject.SetActive(false);}}
        }
        void OnGUI()
        {
            var s=GameSession.Instance;if(Player||Dead||s==null||s.View==null||!s.Playing)return;
            Vector3 p=s.View.WorldToScreenPoint(transform.position+Vector3.up*2.3f);if(p.z<0||p.x<0||p.x>Screen.width)return;
            Rect bar=new Rect(p.x-30,Screen.height-p.y,60,5);GUI.color=Color.black;GUI.DrawTexture(bar,Texture2D.whiteTexture);bar.width*=Current/Maximum;GUI.color=Boss?Color.magenta:Color.red;GUI.DrawTexture(bar,Texture2D.whiteTexture);GUI.color=Color.white;
        }
    }
}
