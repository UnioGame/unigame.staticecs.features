using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    [CreateAssetMenu(menuName = "UniGame/Static ECS/Ability/Ability Asset")]
    public sealed class AbilityAsset : ScriptableObject
    {
        [SerializeField]
        private int _id;

        [SerializeField]
        private bool _isChannel;

        [SerializeField]
        private string _displayName;

        [SerializeReference]
        private IAbilityStepConfig _root;

        public AbilityId Id => new(_id);
        public bool IsChannel => _isChannel;
        public string DisplayName => _displayName;
        public IAbilityStepConfig Root => _root;

        public AbilityDefinition BuildDefinition()
        {
            return new AbilityDefinition(Id, _isChannel, _displayName);
        }
    }
}