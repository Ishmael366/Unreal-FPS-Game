// Copyright (c) 2026 Daniel Nguyen. MIT License.
using UnityEngine;

namespace PortfolioStudies
{
    public sealed class StudyBootstrap:MonoBehaviour
    {
        void Awake(){if(GameSession.Instance==null)gameObject.AddComponent<GameSession>();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureScene(){if(GameSession.Instance==null)new GameObject("Study bootstrap").AddComponent<GameSession>();}
    }
}
