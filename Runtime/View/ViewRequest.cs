namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.ViewSystem.Runtime;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>Contains the data required to create and associate a View System view.</summary>
    [Serializable]
    public struct ViewRequest
    {
        /// <summary>View System identifier.</summary>
        public string ViewId;

        /// <summary>Optional View System skin tag.</summary>
        public string Skin;

        /// <summary>Optional FIFO queue name.</summary>
        public string Queue;

        /// <summary>Entity that controls view lifetime.</summary>
        public EntityGID Owner;

        /// <summary>Gameplay entity synchronized into the model.</summary>
        public EntityGID Source;

        /// <summary>Nested placement configuration.</summary>
        public ViewPlacement Placement;

        [SerializeField, FormerlySerializedAs("Order")]
        private int _order;

        [SerializeField]
        private bool _hasOrder;

        /// <summary>Optional sorting order for the view presentation.</summary>
        public int Order
        {
            get => _order;
            set
            {
                _order = value;
                _hasOrder = true;
            }
        }

        /// <summary>Whether Order was explicitly set, including zero.</summary>
        public bool HasOrder => _hasOrder || _order != 0;

        /// <summary>Whether the view is shown as part of opening.</summary>
        public bool ShowOnOpen;

        /// <summary>Expected model type for a typed request, or empty for legacy requests.</summary>
        public string ExpectedViewModelType;

        /// <summary>Expected view type for a typed request, or empty for legacy requests.</summary>
        public string ExpectedViewType;

        /// <summary>Creates a visible request using the registered view type and its configured model.</summary>
        public static ViewRequest For<TView>()
            where TView : IView
        {
            var request = BindViewType<TView>(default);
            request.ShowOnOpen = true;
            return request;
        }

        /// <summary>Creates a request keyed by the view type name or an explicit View System ID.</summary>
        public static ViewRequest For<TView, TViewModel>(string viewId = null)
            where TView : IView
            where TViewModel : class, IViewModel
        {
            var request = Bind<TView, TViewModel>(default, viewId);
            request.ShowOnOpen = true;
            return request;
        }

        /// <summary>Applies compile-time view and model types to request settings.</summary>
        public static ViewRequest Bind<TView, TViewModel>(
            ViewRequest request,
            string viewId = null)
            where TView : IView
            where TViewModel : class, IViewModel
        {
            request.ViewId = string.IsNullOrWhiteSpace(viewId)
                ? string.IsNullOrWhiteSpace(request.ViewId)
                    ? typeof(TView).Name
                    : request.ViewId
                : viewId;
            request.ExpectedViewModelType = typeof(TViewModel).AssemblyQualifiedName;
            request.ExpectedViewType = typeof(TView).AssemblyQualifiedName;
            return WithDefaultPlacement(request);
        }

        internal static ViewRequest BindViewType<TView>(ViewRequest request)
            where TView : IView
        {
            request.ViewId = typeof(TView).Name;
            request.ExpectedViewType = typeof(TView).AssemblyQualifiedName;
            request.ExpectedViewModelType = null;
            return WithDefaultPlacement(request);
        }

        internal static ViewRequest WithDefaultPlacement(ViewRequest request)
        {
            if (request.Placement.Mode == ViewPlacementMode.Layout &&
                string.IsNullOrWhiteSpace(request.Placement.Layout))
            {
                request.Placement = ViewPlacement.InLayout(
                    UniModules.UniGame.UiSystem.Runtime.ViewType.Window);
            }

            return request;
        }

        /// <summary>Checks that a created model matches the type selected by a typed request.</summary>
        public bool MatchesExpectedViewModel(Type modelType)
        {
            return string.IsNullOrWhiteSpace(ExpectedViewModelType) ||
                   modelType != null && string.Equals(
                       ExpectedViewModelType,
                       modelType.AssemblyQualifiedName,
                       StringComparison.Ordinal);
        }

        /// <summary>Checks that a loaded view matches the type selected by a typed request.</summary>
        public bool MatchesExpectedView(Type viewType)
        {
            return string.IsNullOrWhiteSpace(ExpectedViewType) ||
                   viewType != null && string.Equals(
                       ExpectedViewType,
                       viewType.AssemblyQualifiedName,
                       StringComparison.Ordinal);
        }
    }
}
