namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>Contains the data required to create and associate a View System view.</summary>
    [Serializable]
    public struct ViewRequest
    {
        /// <summary>View System identifier.</summary>
        public string ViewId;

        /// <summary>Optional View System skin tag.</summary>
        public string Skin;

        /// <summary>Optional instance name.</summary>
        public string Name;

        /// <summary>Optional FIFO queue name.</summary>
        public string Queue;

        /// <summary>Entity that controls view lifetime.</summary>
        public EntityGID Owner;

        /// <summary>Gameplay entity synchronized into the model.</summary>
        public EntityGID Source;

        /// <summary>Nested placement configuration.</summary>
        public ViewPlacement Placement;

        /// <summary>Sibling order within the selected parent.</summary>
        public int Order;

        /// <summary>Whether the view is shown as part of opening.</summary>
        public bool ShowOnOpen;
    }
}
