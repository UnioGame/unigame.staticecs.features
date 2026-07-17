using System;
using FFS.Libraries.StaticEcs;
using UnityEngine.AI;

namespace unigame.staticecs.features {
    /// <summary>
    /// Holds a reference to the Unity <see cref="NavMeshAgent"/> managed component
    /// that drives navigation for this entity.
    /// </summary>
    [Serializable]
    public struct NavMeshAgentComponent : IComponent {
        /// <summary>The <see cref="NavMeshAgent"/> attached to the entity's GameObject.</summary>
        public NavMeshAgent Agent;
    }
}
