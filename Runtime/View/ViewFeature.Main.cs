namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using UniGame.StaticEcs.Unity;

    /// <summary>Main-world View Feature.</summary>
    public sealed class ViewFeature : ViewFeature<Main>
    {
        /// <summary>Creates the Main-world View Feature.</summary>
        public ViewFeature(
            IReadOnlyList<Type> modelTypes,
            ViewFeatureConfig config)
            : base(modelTypes, config)
        {
        }
    }
}
