// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortfolioStudies
{
    public sealed class Interactable:MonoBehaviour
    {
        public string Label="Activate",Kind="flag",Flag="",Requires="";public int RequiredGems,RequiredCoins;public bool Remove;
        public void Use()
        {
            var s=GameSession.Instance;if(s==null||!s.Playing)return;
            if(!s.Flags.Has(Requires)){s.Notify("Requires "+Requires);return;}
            if(Kind=="campfire")
            {if(s.Wood<3||s.Elapsed<60){s.Notify("Bring 3 wood and survive 60 seconds.");return;}s.Finish(true,"Camp established. You survived.");return;}
            if(Kind=="extract")
            {if(!s.Flags.Has("gen-a")||!s.Flags.Has("gen-b")||!s.Flags.Has("terminal")){s.Notify("Complete the terminal and both generators first.");return;}s.Finish(true,"Mission complete. Extraction secured.");return;}
            if(Kind=="goal")
            {if(s.Gems<RequiredGems||s.Coins<RequiredCoins){s.Notify("Collect the required tokens first.");return;}s.Finish(true,"Objective complete.");return;}
            if(Kind=="supply"){s.Combat.Ammo.Supply(36);Kind="spent";Label="Supply collected";return;}
            if(Kind=="spent")return;
            s.Flags.Set(Flag);s.Notify(Label+" — complete");s.Tone(330,.12f);if(Remove)Destroy(gameObject);else{Kind="spent";Label+=" (done)";}
        }
    }
}
