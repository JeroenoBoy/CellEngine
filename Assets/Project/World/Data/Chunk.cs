using System.Runtime.CompilerServices;
using CellEngine.Utilities;
using Unity.Entities;
using Unity.Mathematics;



namespace CellEngine.World
{
    public struct Chunk : ISharedComponentData
    {
        //  Must always be a multiple of 2!
        public const int SIZE      = 32;
        public const int AREA      = SIZE * SIZE;
        public const int HALF_SIZE = SIZE / 2;

        public static int2 size2     => new int2(SIZE, SIZE);
        public static int2 halfSize2 => new (HALF_SIZE, HALF_SIZE);

        
        public int2 chunkPosition;

        public int2 worldPosition => chunkPosition * SIZE;
        public int2 max           => worldPosition + size2;
        public int2 min           => worldPosition;
        
        
        public bool Contains(int2 position, int2 size)
        {
            int2 myMin = chunkPosition * SIZE, myMax = myMin + new int2(SIZE, SIZE);
            int2 otherMax = position + size, otherMin = position;

            return Bounds.Contains(myMin, myMax, otherMin, otherMax);
        }


        public static Chunk GetChunkData(int2 worldPosition)
        {
            return new Chunk {chunkPosition = GetChunkPosition(worldPosition)};
        }


        public static int2 GetChunkPosition(int2 worldPosition)
        {
            return (int2)math.floor((float2)worldPosition / (float)SIZE);
        }


        public static int2 GetCellChunkPosition(int2 worldPosition)
        {
            return worldPosition % SIZE;
        }
    }
}