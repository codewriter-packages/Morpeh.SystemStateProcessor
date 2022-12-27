namespace Scellecs.Morpeh
{
    using System;
    using JetBrains.Annotations;
    using UnityEngine;

    public readonly struct SystemStateProcessor<TSystemStateComponent>
        where TSystemStateComponent : struct, ISystemStateComponent
    {
        private readonly CreateDelegate _onAdd;
        private readonly RemoveDelegate _onRemove;

        private readonly Filter _entitiesWithoutStateFilter;
        private readonly Filter _stateOnlyFilterFilter;

        private readonly Stash<TSystemStateComponent> _stateStash;
        private readonly Stash<Info<TSystemStateComponent>> _infoStash;

        public Filter Entities { get; }

        internal SystemStateProcessor(
            Filter filter,
            CreateDelegate onAdd,
            RemoveDelegate onRemove = null)
        {
            Entities = filter;

            _onAdd = onAdd;
            _onRemove = onRemove ?? delegate { };

            _entitiesWithoutStateFilter = Entities.Without<TSystemStateComponent>();
            _stateOnlyFilterFilter = Entities.world.Filter.With<TSystemStateComponent>();

            _stateStash = Entities.world.GetStash<TSystemStateComponent>();
            _infoStash = Entities.world.GetStash<Info<TSystemStateComponent>>();
        }

        [PublicAPI]
        public void Process()
        {
            var currentFrame = Time.frameCount;

            foreach (var entity in Entities)
            {
                _infoStash.Set(entity, new Info<TSystemStateComponent>
                {
                    frame = currentFrame,
                });
            }

            foreach (var entity in _entitiesWithoutStateFilter)
            {
                _stateStash.Set(entity, _onAdd.Invoke(entity));
            }

            foreach (var entity in _stateOnlyFilterFilter)
            {
                var lastFrame = _infoStash.Get(entity, out var exists).frame;

                if (exists && lastFrame == currentFrame)
                {
                    continue;
                }

                ref var state = ref _stateStash.Get(entity);

                _onRemove.Invoke(ref state);

                _stateStash.Remove(entity);
                _infoStash.Remove(entity);
            }

            Entities.world.Commit();
        }

        public delegate TSystemStateComponent CreateDelegate(Entity entity);

        public delegate void RemoveDelegate(ref TSystemStateComponent data);


        [Serializable]
        internal struct Info<TSystemState> : IComponent
            where TSystemState : struct, ISystemStateComponent
        {
            public int frame;
        }
    }
}