using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Persistent movement destination for a navigating entity.</summary>
    [Serializable]
    public struct MovementDestinationComponent : IComponent {
        /// <summary>World-space destination the agent is navigating towards.</summary>
        public Vector3 Destination;
        /// <summary>When <c>false</c> the agent should stop; when <c>true</c> it navigates to <see cref="Destination"/>.</summary>
        public bool IsActive;
    }
}
