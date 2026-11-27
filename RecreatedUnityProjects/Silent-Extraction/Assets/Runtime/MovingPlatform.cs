// Copyright (c) 2026 Daniel Nguyen. MIT License.
using System.Collections.Generic;
using UnityEngine;

namespace PortfolioStudies
{
    [DefaultExecutionOrder(-50)]
    public sealed class MovingPlatform:MonoBehaviour
    {
        public Vector3 Travel=new Vector3(3,0,0);public float Speed=.8f;Vector3 start,last;public Vector3 Delta{get;private set;}
        void Start(){start=last=transform.position;}
        void Update(){transform.position=start+Travel*Mathf.Sin(Time.time*Speed);Delta=transform.position-last;last=transform.position;}
    }
}
