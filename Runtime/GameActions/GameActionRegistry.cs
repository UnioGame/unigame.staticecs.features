namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    /// <summary>Assigns one explicit stable action bit to each declared action type.</summary>
    public class GameActionRegistry<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        private readonly Dictionary<Type, byte> _typeToId = new();
        private readonly Type[] _idToType = new Type[32];

        /// <summary>Registers an action type with an explicit stable ID in the range 0–31.</summary>
        public void Register<TAction>(byte id)
            where TAction : struct, IGameAction
        {
            if (id >= 32)
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    id,
                    "Game action IDs must be in the range 0–31.");

            var actionType = typeof(TAction);
            if (_typeToId.TryGetValue(actionType, out var existing))
            {
                if (existing != id)
                    throw new InvalidOperationException(
                        $"Action `{actionType.FullName}` is already registered with ID {existing}, " +
                        $"not {id}.");

                return;
            }

            var conflictingType = _idToType[id];
            if (conflictingType != null)
                throw new InvalidOperationException(
                    $"Game action ID {id} is used by both `{conflictingType.FullName}` and " +
                    $"`{actionType.FullName}`.");

            _typeToId.Add(actionType, id);
            _idToType[id] = actionType;
        }

        /// <summary>Returns the stable bit mask for a declared action.</summary>
        public uint GetMask<TAction>()
            where TAction : struct, IGameAction
        {
            if (!_typeToId.TryGetValue(typeof(TAction), out var id))
                throw new InvalidOperationException(
                    $"Game action `{typeof(TAction).FullName}` was not declared for world " +
                    $"`{typeof(TWorld).Name}`.");

            return 1u << id;
        }

        /// <summary>Returns whether an action type has been declared.</summary>
        public bool Contains<TAction>()
            where TAction : struct, IGameAction
        {
            return _typeToId.ContainsKey(typeof(TAction));
        }
    }

    /// <summary>Controls GameActions system composition.</summary>
    public sealed class GameActionsConfig : IResource
    {
        /// <summary>Whether action mask maintenance is installed.</summary>
        public bool RegisterMaintenance = true;

        /// <summary>Execution order of action mask maintenance.</summary>
        public short MaintenanceOrder = 25;
    }
}
