namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Projects UI Toolkit mouse state onto element entities.</summary>
    public class UpdateHoveredElementsSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var filter = default(All<UiToolkitElementComponent>);
            foreach (var entity in World<TWorld>.Query(filter).Entities())
            {
                var element = entity.Read<UiToolkitElementComponent>().Element;
                entity.Apply<IsHoveredTag>(element != null && element.IsPointerHovered);
            }
        }
    }
}
