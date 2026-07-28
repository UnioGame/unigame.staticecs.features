namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using FFS.Libraries.StaticEcs;
    using UniGame.ViewSystem.Runtime;
    using UnityEngine.Scripting;

    internal static class ViewModelTypeRegistration
    {
        private static readonly MethodInfo RegisterComponentMethod =
            typeof(ViewModelTypeRegistration).GetMethod(
                nameof(RegisterComponent),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo RegisterBinderMethod =
            typeof(ViewModelTypeRegistration).GetMethod(
                nameof(RegisterBinder),
                BindingFlags.Static | BindingFlags.NonPublic);

        public static void RegisterComponents<TWorld>(
            World<TWorld>.TypeRegistrar types,
            IReadOnlyList<Type> modelTypes)
            where TWorld : struct, IWorldType
        {
            foreach (var modelType in modelTypes)
            {
                if (!IsViewModel(modelType))
                    continue;

                RegisterComponentMethod
                    .MakeGenericMethod(typeof(TWorld), modelType)
                    .Invoke(null, new object[] { types });
            }
        }

        public static ViewModelBinderRegistry<TWorld> CreateBinders<TWorld>(
            IReadOnlyList<Type> modelTypes)
            where TWorld : struct, IWorldType
        {
            var registry = new ViewModelBinderRegistry<TWorld>();
            foreach (var modelType in modelTypes)
            {
                if (!IsViewModel(modelType))
                    continue;

                RegisterBinderMethod
                    .MakeGenericMethod(typeof(TWorld), modelType)
                    .Invoke(null, new object[] { registry });
            }

            return registry;
        }

        private static bool IsViewModel(Type modelType)
        {
            return modelType is { IsClass: true, IsAbstract: false } &&
                   typeof(IViewModel).IsAssignableFrom(modelType);
        }

        [Preserve]
        private static void RegisterComponent<TWorld, TModel>(
            World<TWorld>.TypeRegistrar types)
            where TWorld : struct, IWorldType
            where TModel : class, IViewModel
        {
            types.Component<ViewModelComponent<TModel>>();
        }

        [Preserve]
        private static void RegisterBinder<TWorld, TModel>(
            ViewModelBinderRegistry<TWorld> registry)
            where TWorld : struct, IWorldType
            where TModel : class, IViewModel
        {
            registry.Register<TModel>();
        }
    }
}
