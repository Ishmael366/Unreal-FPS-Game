// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortfolioStudies
{
    public sealed class PrizeBlock:MonoBehaviour
    {
        bool opened;public void Open(){if(opened)return;opened=true;GameSession.Instance.Coins+=2;GetComponent<Renderer>().sharedMaterial=World.Mat(new Color(.4f,.3f,.16f));GameSession.Instance.Notify("Bonus block: +2 tokens");}
    }
}
