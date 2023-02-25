using JetBrains.Annotations;

namespace Scellecs.Morpeh
{
    public static class SystemStateFilterExtensions
    {
        [PublicAPI]
        public static SystemStateProcessor<TSystemStateComponent> ToSystemStateProcessor<TSystemStateComponent>(
            this Filter filter,
            SystemStateProcessor<TSystemStateComponent>.SetupDelegate setup,
            SystemStateProcessor<TSystemStateComponent>.DisposeDelegate dispose = null)
            where TSystemStateComponent : struct, ISystemStateComponent
        {
            return dispose != null
                ? new DisposableSystemStateProcessor<TSystemStateComponent>(filter, setup, dispose)
                : new SystemStateProcessor<TSystemStateComponent>(filter, setup);
        }
    }
}