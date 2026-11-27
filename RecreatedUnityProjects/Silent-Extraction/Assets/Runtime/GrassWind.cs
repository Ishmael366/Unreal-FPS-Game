// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System.Collections.Generic;
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class GrassWind:MonoBehaviour
    {
        Material material; Renderer view;
        void Awake(){view=GetComponent<Renderer>();material=view.sharedMaterial;}
        void Update(){var s=GameSession.Instance;if(s==null)return;view.enabled=s.ShowGrass;material.SetFloat("_WindStrength",s.Wind);}
    }
}
