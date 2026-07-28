namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Process-global registry that maps effect-type structs to a stable <see cref="EffectId"/>.
    /// Stored as the world resource of the same type, populated by every
    /// <c>EffectFeature&lt;TWorld, TEffect&gt;</c> on registration.
    ///
    /// Ids are deterministic per type within a process but not stable across builds — do not
    /// persist them to long-term storage without a remap layer.
    /// </summary>
    public sealed class EffectIdRegistry : IResource
    {
        private readonly Dictionary<Type, EffectId> _byType = new();
        private readonly Dictionary<int, Type> _byId = new();
        private int _next = 1;

        public EffectId Register<TEffect>()
            where TEffect : struct, IEffectType
        {
            var key = typeof(TEffect);
            if (_byType.TryGetValue(key, out var id))
                return id;

            id = new EffectId(_next++);
            _byType[key] = id;
            _byId[id.Value] = key;
            return id;
        }

        public bool TryGet<TEffect>(out EffectId id)
            where TEffect : struct, IEffectType
        {
            return _byType.TryGetValue(typeof(TEffect), out id);
        }

        public EffectId Get<TEffect>()
            where TEffect : struct, IEffectType
        {
            return _byType[typeof(TEffect)];
        }

        public bool TryGetType(EffectId id, out Type type)
        {
            return _byId.TryGetValue(id.Value, out type);
        }

        public string GetTypeName(EffectId id)
        {
            return _byId.TryGetValue(id.Value, out var type) ? type.Name : $"Effect#{id.Value}";
        }
    }
}
