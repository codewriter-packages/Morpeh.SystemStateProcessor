using JetBrains.Annotations;

namespace Scellecs.Morpeh
{
    public static class SystemStateFilterExtensions
    {
        [PublicAPI]
        public static SystemStateProcessor<TSystemStateComponent> ToSystemStateProcessor<TSystemStateComponent>(
            this Filter filter,
            SystemStateProcessor<TSystemStateComponent>.CreateDelegate onAdd,
            SystemStateProcessor<TSystemStateComponent>.RemoveDelegate onRemove = null)
            where TSystemStateComponent : struct, ISystemStateComponent
        {
            return new(filter, onAdd, onRemove);
        }
    }
}