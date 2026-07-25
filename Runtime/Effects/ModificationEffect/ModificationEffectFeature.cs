namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Time-bounded characteristic modification slice. Generic over the target characteristic
    /// type; project code instantiates one feature per stat (e.g.
    /// <c>ModificationEffectFeature&lt;Main, SpeedCharacteristic&gt;</c>).
    ///
    /// Requires the matching <see cref="CharacteristicFeature{TWorld, TCharacteristic}"/> and
    /// <see cref="ModifierBackRefFeature{TWorld}"/> to be registered first.
    /// </summary>
    public class ModificationEffectFeature<TWorld, TStat>
        : EffectFeature<TWorld, ModificationEffect<TStat>>
        where TWorld : struct, IWorldType
        where TStat : struct, ICharacteristicType
    {
        protected override IEffectHandler<TWorld, ModificationEffect<TStat>>
            CreateDefaultHandler()
        {
            return new ModificationEffectHandler<TWorld, TStat>();
        }
    }
}
