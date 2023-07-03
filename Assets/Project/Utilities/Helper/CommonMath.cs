using System.Runtime.CompilerServices;
using Unity.Mathematics;



namespace CellEngine.Utilities
{
    public static class Bools
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(bool2 bools)
        {
            return bools is {x: true, y: true};
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(bool2 bools)
        {
            return bools.x || bools.y;
        }
    }
}
