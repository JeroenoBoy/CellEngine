using System;
using Unity.Entities;



namespace CellEngine.Utilities
{
    public static class ManagedDynamicBufferExtensions
    {
        public static T Find<T>(this DynamicBuffer<T> buffer, Predicate<T> predicate) where T : unmanaged, IBufferElementData
        {
            for (int i = 0; i < buffer.Length; i++) {
                if (predicate(buffer[i])) {
                    return buffer[i];
                }
            }

            throw new ArgumentNullException();
        }
    }
}
