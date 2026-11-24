namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>Resolves generated view ID enum members to configured View System keys.</summary>
    public static class ViewSystemIdResolver
    {
        /// <summary>Finds the configured key represented by an annotated enum member.</summary>
        public static bool TryResolve<TViewId>(TViewId viewId, out string value)
            where TViewId : struct, Enum
        {
            var name = Enum.GetName(typeof(TViewId), viewId);
            if (name == null)
            {
                value = null;
                return false;
            }

            return EnumViewSystemIdMap<TViewId>.Values.TryGetValue(name, out value);
        }

        private static class EnumViewSystemIdMap<TViewId>
            where TViewId : struct, Enum
        {
            public static readonly Dictionary<string, string> Values = Create();

            private static Dictionary<string, string> Create()
            {
                var values = new Dictionary<string, string>(StringComparer.Ordinal);
                var fields = typeof(TViewId).GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    var attribute = Attribute.GetCustomAttribute(
                        field,
                        typeof(ViewSystemIdAttribute)) as ViewSystemIdAttribute;
                    if (attribute == null || string.IsNullOrWhiteSpace(attribute.Value))
                        continue;

                    values.Add(field.Name, attribute.Value);
                }

                return values;
            }
        }
    }
}
