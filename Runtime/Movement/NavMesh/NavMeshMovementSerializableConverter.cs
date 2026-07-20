using System;
using FFS.Libraries.StaticEcs;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Creates a NavMesh movement component from the host GameObject.</summary>
    [Serializable]
    public class NavMeshMovementSerializableConverter<TWorld> :
        EcsComponentSerializableConverter<TWorld, NavMeshAgentComponent>
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc />
        protected override NavMeshAgentComponent Build(GameObject host)
        {
            return NavMeshMovementConverterUtility.Build(host);
        }
    }
}
