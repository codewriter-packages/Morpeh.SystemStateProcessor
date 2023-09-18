using System;
using UnityEngine;

namespace Scellecs.Morpeh
{
    internal class DisposableSystemStateProcessor<TSystemStateComponent>
        : SystemStateProcessor<TSystemStateComponent>
        where TSystemStateComponent : struct, ISystemStateComponent
    {
        private static readonly Stash<TSystemStateComponent>.ComponentDispose InvalidDisposer
            = LogInvalidSystemStateDispose;

        internal DisposableSystemStateProcessor(FilterBuilder filter, SetupDelegate setup, DisposeDelegate dispose)
            : base(filter, setup)
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(TSystemStateComponent)))
            {
                var tName = typeof(TSystemStateComponent).Name;
                throw new Exception($"{tName} cannot be IDisposable");
            }

            if (stateStash.componentDispose != null && stateStash.componentDispose != InvalidDisposer)
            {
                var tName = typeof(TSystemStateComponent).Name;
                throw new Exception(
                    $"Only one instance of DisposableSystemStateProcessor<{tName}> can be created per world");
            }

            stateStash.componentDispose = (ref TSystemStateComponent component) => dispose.Invoke(ref component);
        }

        public override void Dispose()
        {
            base.Dispose();

            stateStash.componentDispose = InvalidDisposer;
        }

        private static void LogInvalidSystemStateDispose(ref TSystemStateComponent component)
        {
            Debug.LogError($"SystemStateComponent {typeof(TSystemStateComponent).Name} was destroyed " +
                           $"after SystemStateProcessor disposing");
        }
    }
}