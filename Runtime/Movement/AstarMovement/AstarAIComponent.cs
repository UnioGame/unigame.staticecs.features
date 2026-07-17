using System;
using FFS.Libraries.StaticEcs;
using Pathfinding;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>
    /// Holds references to the A* Pathfinding Project components
    /// that drive navigation for this entity.
    /// </summary>
    [Serializable]
    public struct AstarAIComponent : IComponent {
        /// <summary>The <see cref="IAstarAI"/> implementation (e.g. <c>AIPath</c> or <c>RichAI</c>) on the entity's GameObject.</summary>
        public IAstarAI AI;
        /// <summary>The destination used for the last explicit path request.</summary>
        public Vector3 LastRequestedDestination;
        /// <summary>Whether an explicit path request has been issued for the current active movement.</summary>
        public bool HasRequestedDestination;
    }
}
