#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using System;
using JetBrains.Annotations;

namespace Scellecs.Morpeh
{
    public struct SystemStateProcessor<TSystemStateComponent>
        where TSystemStateComponent : struct, ISystemStateComponent
    {
        internal readonly SetupDelegate setupDelegate;
        internal readonly DisposeDelegate disposeDelegate;
        internal readonly World world;

        internal readonly Filter entitiesWithoutStateFilter;
        internal readonly Filter stateOnlyFilterFilter;

        internal readonly Stash<TSystemStateComponent> stateStash;
        internal readonly Stash<Info> infoStash;

        internal int frame;

        public readonly Filter Entities;

        internal SystemStateProcessor(FilterBuilder filter, SetupDelegate setup, DisposeDelegate dispose)
        {
            Entities = filter.Build();

            setupDelegate = setup;
            disposeDelegate = dispose;
            world = filter.world;

            entitiesWithoutStateFilter = filter.Without<TSystemStateComponent>().Build();
            stateOnlyFilterFilter = world.Filter.With<TSystemStateComponent>().Build();

            stateStash = world.GetStash<TSystemStateComponent>();
            infoStash = world.GetStash<Info>();

            frame = 0;

            if (disposeDelegate != null)
            {
#if MORPEH_DEBUG
                if (typeof(IDisposable).IsAssignableFrom(typeof(TSystemStateComponent)))
                {
                    var tName = typeof(TSystemStateComponent).Name;
                    throw new Exception($"{tName} cannot be IDisposable");
                }

                if (stateStash.componentDispose != null)
                {
                    var tName = typeof(TSystemStateComponent).Name;
                    throw new Exception(
                        $"Only one instance of DisposableSystemStateProcessor<{tName}> can be created per world");
                }
#endif

                stateStash.componentDispose = (ref TSystemStateComponent component) => dispose.Invoke(ref component);
            }
        }

        public void Dispose()
        {
            DestroyAllStates();

            if (disposeDelegate != null)
            {
                stateStash.componentDispose = null;
            }
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
                stateStash.Set(entity, setupDelegate.Invoke(entity));
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