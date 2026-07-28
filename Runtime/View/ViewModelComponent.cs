namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.ViewSystem.Runtime;
    using UnityEngine.Scripting;

    /// <summary>Associates an ECS view entity with its View System model.</summary>
    [Preserve]
    public struct ViewModelComponent<TModel> : IComponent
        where TModel : class, IViewModel
    {
        /// <summary>View System model associated with the ECS view entity.</summary>
        public TModel Model;
    }
}
