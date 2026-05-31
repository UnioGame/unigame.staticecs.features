using System;

namespace unigame.staticecs.features.Editor.AbilityGraph {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    internal sealed class AbilityGraphContextCommandAttribute : Attribute {
        public AbilityGraphContextCommandAttribute(AbilityGraphContextTarget target, string path) {
            Target = target;
            Path = path;
        }

        public AbilityGraphContextTarget Target { get; }
        public string Path { get; }
        public int Order { get; set; }
        public Type NodeType { get; set; }
    }
}