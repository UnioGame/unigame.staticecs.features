namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    public class AbilityEffectDispatchRegistry<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        private readonly Dictionary<int, AbilityEffectDispatcher<TWorld>> _dispatchers = new();

        public void Register<TEffect>(EffectIdRegistry ids)
            where TEffect : struct, IEffectType
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            Register(ids.Get<TEffect>(), Dispatch<TEffect>);
        }

        public void Register(EffectId id, AbilityEffectDispatcher<TWorld> dispatcher)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException("Effect id must be valid.", nameof(id));
            }
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            _dispatchers[id.Value] = dispatcher;
        }

        public bool TryDispatch(
            EffectId id,
            EntityGID source,
            EntityGID target,
            float duration,
            float period,
            float delay,
            float magnitude
        )
        {
            if (!id.IsValid)
            {
                return false;
            }
            return _dispatchers.TryGetValue(id.Value, out var dispatcher)
                && dispatcher(source, target, duration, period, delay, magnitude);
        }

        public bool IsRegistered(EffectId id)
        {
            return id.IsValid && _dispatchers.ContainsKey(id.Value);
        }

        private static bool Dispatch<TEffect>(
            EntityGID source,
            EntityGID target,
            float duration,
            float period,
            float delay,
            float magnitude
        )
            where TEffect : struct, IEffectType
        {
            return EffectOperations.Apply<TWorld, TEffect>(target, source, duration, period, delay);
        }
    }
}
