using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    [MovedFrom(true, sourceNamespace: "unigame.staticecs.features", sourceAssembly: "unigame.staticecs.features")]
    public sealed class AoeQueryStepConfig : IAbilityStepConfig {
        [SerializeField] private string _nodeGuid;
        [SerializeField] private float _radius;
        [SerializeField] private int _maxTargets = 16;
        [SerializeField] private bool _excludeCaster = true;

        public AoeQueryStepConfig() { }

        public AoeQueryStepConfig(float radius, int maxTargets = 16, bool excludeCaster = true, string nodeGuid = null) {
            _radius = radius;
            _maxTargets = maxTargets;
            _excludeCaster = excludeCaster;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.AoeQuery;
        public string NodeGuid => _nodeGuid;
        public float Radius => _radius;
        public int MaxTargets => _maxTargets;
        public bool ExcludeCaster => _excludeCaster;
    }
}
