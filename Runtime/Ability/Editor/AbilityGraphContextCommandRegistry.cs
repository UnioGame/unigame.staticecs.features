namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal static class AbilityGraphContextCommandRegistry
    {
        private sealed class CommandDefinition
        {
            public AbilityGraphContextCommandAttribute Attribute;
            public MethodInfo Method;
        }

        private static List<CommandDefinition> _commands;

        public static void AppendCommands(
            DropdownMenu menu,
            AbilityGraphContextCommandContext context
        )
        {
            var commands = GetCommands();
            var added = false;

            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                if (!Matches(command.Attribute, context))
                    continue;

                added = true;
                menu.AppendAction(
                    command.Attribute.Path,
                    _ => InvokeCommand(command.Method, context),
                    DropdownMenuAction.Status.Normal
                );
            }

            if (!added)
                menu.AppendAction("No Commands", _ => { }, DropdownMenuAction.Status.Disabled);
        }

        private static List<CommandDefinition> GetCommands()
        {
            if (_commands != null)
                return _commands;

            _commands = new List<CommandDefinition>();
            var methods = TypeCache.GetMethodsWithAttribute<AbilityGraphContextCommandAttribute>();
            for (var methodIndex = 0; methodIndex < methods.Count; methodIndex++)
            {
                var method = methods[methodIndex];
                if (!IsValidCommandMethod(method))
                    continue;

                var attributes = method.GetCustomAttributes(
                    typeof(AbilityGraphContextCommandAttribute),
                    false
                );
                for (var attrIndex = 0; attrIndex < attributes.Length; attrIndex++)
                {
                    if (attributes[attrIndex] is not AbilityGraphContextCommandAttribute attribute)
                        continue;

                    _commands.Add(new CommandDefinition { Attribute = attribute, Method = method });
                }
            }

            _commands = _commands
                .OrderBy(x => x.Attribute.Order)
                .ThenBy(x => x.Attribute.Path, StringComparer.Ordinal)
                .ToList();

            return _commands;
        }

        private static bool IsValidCommandMethod(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 1
                && parameters[0].ParameterType == typeof(AbilityGraphContextCommandContext);
        }

        private static bool Matches(
            AbilityGraphContextCommandAttribute attribute,
            AbilityGraphContextCommandContext context
        )
        {
            if (attribute.Target != context.Target)
                return false;

            if (attribute.NodeType == null)
                return true;

            var nodeConfig = context.NodeConfig;
            return nodeConfig != null && attribute.NodeType.IsInstanceOfType(nodeConfig);
        }

        private static void InvokeCommand(
            MethodInfo method,
            AbilityGraphContextCommandContext context
        )
        {
            try
            {
                method.Invoke(null, new object[] { context });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                Debug.LogException(exception.InnerException);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
