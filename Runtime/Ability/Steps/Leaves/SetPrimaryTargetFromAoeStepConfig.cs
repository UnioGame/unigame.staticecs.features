namespace UniGame.StaticEcs.Features
{
    using System;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(
        true,
        sourceNamespace: "unigame.staticecs.features",
        sourceAssembly: "unigame.staticecs.features"
    )]
    public sealed class SetPrimaryTargetFromAoeStepConfig : IAbilityStepConfig
    {
        [SerializeField]
        private string _nodeGuid;

        [SerializeField]
        private AoeTargetSelector _selector;

        public SetPrimaryTargetFromAoeStepConfig() { }

        public SetPrimaryTargetFromAoeStepConfig(AoeTargetSelector selector, string nodeGuid = null)
        {
            _selector = selector;
            _nodeGuid = nodeGuid;
        }

        public AbilityStepKind Kind => AbilityStepKind.SetPrimaryTargetFromAoe;
        public string NodeGuid => _nodeGuid;
        public AoeTargetSelector Selector => _selector;
    }
}
