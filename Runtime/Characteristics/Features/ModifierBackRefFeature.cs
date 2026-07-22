namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Modifiers;

    public class ModifierBackRefFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types.Component<ModifierTrackerComponent>().Multi<ModifierTargetComponent>();

            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                World<TWorld>.SetResource(new ModifierRegistry());
            }
        }
    }
}
