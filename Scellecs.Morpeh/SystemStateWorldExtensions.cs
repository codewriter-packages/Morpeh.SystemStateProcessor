using System.Collections.Generic;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    public static class SystemStateWorldExtensions
    {
        private static readonly Dictionary<int, bool> TypeIdToSystemStateComponentLookup = new();

        [PublicAPI]
        public static void MigrateSystemStateComponents(this World world, Entity entity)
        {
            Entity migratedEntity = null;

            foreach (var typeId in entity.currentArchetype.typeIds)
            {
                if (!world.stashes.TryGetValue(typeId, out var index))
                {
                    continue;
                }

                if (!TypeIdToSystemStateComponentLookup.TryGetValue(typeId, out var isSystemState))
                {
                    var type = CommonTypeIdentifier.intTypeAssociation[typeId].type;

                    isSystemState = typeof(ISystemStateComponent).IsAssignableFrom(type);

                    TypeIdToSystemStateComponentLookup[typeId] = isSystemState;
                }

                if (!isSystemState)
                {
                    continue;
                }

                migratedEntity ??= world.CreateEntity();

                Stash.stashes.data[index].Migrate(entity, migratedEntity, false);
            }
        }
    }
}