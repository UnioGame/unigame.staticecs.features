using System.Collections.Generic;
using UnityEngine;

namespace unigame.staticecs.features {
    [CreateAssetMenu(menuName = "UniGame/Static ECS/Ability/Ability Database")]
    public sealed class AbilityDatabase : ScriptableObject {
        [SerializeField] private List<AbilityAsset> _abilities = new();

        public IReadOnlyList<AbilityAsset> Abilities => _abilities;
        public int Count => _abilities?.Count ?? 0;

        public AbilityAsset GetAbility(int index) {
            return _abilities[index];
        }
    }
}
