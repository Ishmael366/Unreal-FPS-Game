// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortfolioStudies
{
    public sealed class Collectible:MonoBehaviour
    {
        public string Kind="coin";public int Amount=1;float baseY;
        void Start(){baseY=transform.position.y;}
        void Update()
        {
            var s=GameSession.Instance;if(s==null||!s.Playing)return;transform.Rotate(0,80*Time.deltaTime,0);var p=transform.position;p.y=baseY+Mathf.Sin(Time.time*3)*.13f;transform.position=p;
            if(Vector3.Distance(transform.position,s.Motor.transform.position+Vector3.up*.8f)>1.2f)return;
            if(Kind=="coin")s.Coins+=Amount;else if(Kind=="gem")s.Gems+=Amount;else if(Kind=="wood")s.Wood+=Amount;
            else if(Kind=="food")s.Stats.Food=Mathf.Min(100,s.Stats.Food+35);else if(Kind=="water")s.Stats.Water=Mathf.Min(100,s.Stats.Water+45);
            else if(Kind=="health"){var h=s.Motor.GetComponent<ActorHealth>();h.Current=Mathf.Min(h.Maximum,h.Current+35);s.Stats.Health=h.Current;}
            else if(Kind=="ammo")s.Combat.Ammo.Supply(Amount);else s.Flags.Set(Kind);
            s.Notify("Collected "+Kind);s.Tone(660,.08f);Destroy(gameObject);
        }
    }
}
