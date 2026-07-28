namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UniGame.ViewSystem.Runtime;
    using UnityEngine.Scripting;

    internal interface IViewModelComponentBinder<TWorld>
        where TWorld : struct, IWorldType
    {
        bool Attach(World<TWorld>.Entity entity, IViewModel model);
        void Detach(World<TWorld>.Entity entity);
    }

    [Preserve]
    internal sealed class ViewModelComponentBinder<TWorld, TModel> :
        IViewModelComponentBinder<TWorld>
        where TWorld : struct, IWorldType
        where TModel : class, IViewModel
    {
        public bool Attach(World<TWorld>.Entity entity, IViewModel model)
        {
            if (model is not TModel typed)
                return false;

            entity.Set(new ViewModelComponent<TModel> { Model = typed });
            return true;
        }

        public void Detach(World<TWorld>.Entity entity)
        {
            if (entity.Has<ViewModelComponent<TModel>>())
                entity.Delete<ViewModelComponent<TModel>>();
        }
    }

    internal sealed class ViewModelBinderRegistry<TWorld>
        where TWorld : struct, IWorldType
    {
        private readonly Dictionary<Type, IViewModelComponentBinder<TWorld>> _binders = new();

        public void Register<TModel>()
            where TModel : class, IViewModel
        {
            _binders[typeof(TModel)] = new ViewModelComponentBinder<TWorld, TModel>();
        }

        public bool TryGet(Type modelType, out IViewModelComponentBinder<TWorld> binder)
        {
            return _binders.TryGetValue(modelType, out binder);
        }
    }
}
