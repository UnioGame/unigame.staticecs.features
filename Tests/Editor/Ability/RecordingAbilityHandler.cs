using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features.Tests {
    public sealed class RecordingAbilityHandler<TWorld> : IAbilityHandler<TWorld>
        where TWorld : struct, IWorldType {
        public readonly List<Invocation> Invocations = new();

        public void OnCast(EntityGID caster, ReadOnlySpan<EntityGID> targets) {
            var copy = targets.Length == 0 ? Array.Empty<EntityGID>() : targets.ToArray();
            Invocations.Add(new Invocation { Caster = caster, Targets = copy });
        }

        public struct Invocation {
            public EntityGID Caster;
            public EntityGID[] Targets;
        }
    }
}
