namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates one characteristic component from inline settings.</summary>
    [Serializable]
    public class CharacteristicSerializableConverter<TWorld, TCharacteristic>
        : EcsComponentSerializableConverter<TWorld, CharacteristicComponent<TCharacteristic>>
        where TWorld : struct, IWorldType
        where TCharacteristic : struct, ICharacteristicType
    {
        [SerializeField]
        private CharacteristicSettings _settings = new CharacteristicSettings(0f, 0f, 100f);

        /// <summary>Gets or sets the initial characteristic value and bounds.</summary>
        public CharacteristicSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        /// <inheritdoc />
        protected override CharacteristicComponent<TCharacteristic> Build(GameObject host)
        {
            return CharacteristicConverterUtility.Build<TCharacteristic>(_settings);
        }
    }
}
