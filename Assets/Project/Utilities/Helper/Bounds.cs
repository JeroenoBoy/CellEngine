using System.Runtime.CompilerServices;
using Unity.Mathematics;



namespace CellEngine.Utilities
{
    public static class Bounds
    {
        public static bool IsInside(int2 position, int2 boundsMax, int2 boundsMin)
        {
            return (boundsMin.x <= position.x && boundsMax.x >= position.x)
                && (boundsMin.y <= position.y && boundsMax.y >= position.y);
        }


        public static bool Contains(int2 aMin, int2 aMax, int2 bMin, int2 bMax)
        {
            return CheckHorizontalOverlap(aMin.x, aMax.x, bMin.x, bMax.x)
                && CheckHorizontalOverlap(aMin.y, aMax.y, bMin.y, bMax.y);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckHorizontalOverlap(int aMin, int aMax, int bMin, int bMax)
        {
            if (aMin > bMin) {
                return aMax <= bMax;
            }
            return aMax <= bMax;
        }
    }
}
