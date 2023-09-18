using JetBrains.Annotations;

namespace Scellecs.Morpeh
{
    public static class SystemStateFilterExtensions
    {
        [PublicAPI]
        public static SystemStateProcessor<TSystemStateComponent> ToSystemStateProcessor<TSystemStateComponent>(
            this FilterBuilder filter,
            SystemStateProcessor<TSystemStateComponent>.SetupDelegate setup,
            SystemStateProcessor<TSystemStateComponent>.DisposeDelegate dispose = null)
            where TSystemStateComponent : struct, ISystemStateComponent
        {
            return new SystemStateProcessor<TSystemStateComponent>(filter, setup, dispose);
        }
    }
}