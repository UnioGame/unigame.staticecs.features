using System;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>
    /// Asynchronous leaf step that waits <see cref="Duration"/> seconds of <c>EcsTime</c>
    /// before reporting <see cref="StepStatus.Success"/>. A non-positive duration completes
    /// synchronously inside the activator.
    /// </summary>
    [Serializable]
    public sealed class WaitStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeField] private float _duration;

        public WaitStepConfig() { }

        public WaitStepConfig(float duration, string nodeGuid = null) {
            _duration = duration;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.Wait;
        public string NodeGuid => _nodeGuid;
        public float Duration => _duration;
    }
}
