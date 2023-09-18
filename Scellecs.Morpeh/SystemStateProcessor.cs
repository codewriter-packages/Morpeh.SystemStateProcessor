using System;
using JetBrains.Annotations;

namespace Scellecs.Morpeh
{
    public class SystemStateProcessor<TSystemStateComponent> : IDisposable
        where TSystemStateComponent : struct, ISystemStateComponent
    {
        internal readonly SetupDelegate setupDelegate;
        internal readonly World world;

        internal readonly Filter entitiesWithoutStateFilter;
        internal readonly Filter stateOnlyFilterFilter;

        internal readonly Stash<TSystemStateComponent> stateStash;
        internal readonly Stash<Info> infoStash;

        internal int frame;

        public readonly Filter Entities;

        internal SystemStateProcessor(FilterBuilder filter, SetupDelegate setup)
        {
            Entities = filter.Build();

            setupDelegate = setup;
            world = filter.world;

            entitiesWithoutStateFilter = filter.Without<TSystemStateComponent>().Build();
            stateOnlyFilterFilter = world.Filter.With<TSystemStateComponent>().Build();

            stateStash = world.GetStash<TSystemStateComponent>();
            infoStash = world.GetStash<Info>();
        }

        public virtual void Dispose()
        {
            DestroyAllStates();
        }

        [PublicAPI]
        public void Process()
        {
            var currentFrame = ++frame;

            foreach (var entity in Entities)
            {
                infoStash.Set(entity, new Info
                {
                    frame = currentFrame
                });
            }

            foreach (var entity in entitiesWithoutStateFilter)
            {
                var state = setupDelegate.Invoke(entity);
                stateStash.Set(entity, state);
            }

            foreach (var entity in stateOnlyFilterFilter)
            {
                var lastFrame = infoStash.Get(entity, out var exists).frame;

                if (exists && lastFrame == currentFrame)
                {
                    continue;
                }

                infoStash.Remove(entity);
                stateStash.Remove(entity);
            }

            world.Commit();
        }

        [PublicAPI]
        public void DestroyAllStates()
        {
            if (stateOnlyFilterFilter.IsEmpty())
            {
                return;
            }

            foreach (var entity in stateOnlyFilterFilter)
            {
                infoStash.Remove(entity);
                stateStash.Remove(entity);
            }

            world.Commit();
        }

        public delegate TSystemStateComponent SetupDelegate(Entity entity);

        public delegate void DisposeDelegate(ref TSystemStateComponent data);

        [Serializable]
        internal struct Info : IComponent
        {
            public int frame;
        }
    }
}