using System;
using System.Runtime.CompilerServices;
using CellEngine.Utilities;
using Unity.Collections;
using Unity.Mathematics;



namespace CellEngine.World
{
    /// <summary>
    /// The struct containing all the data in this struct
    /// </summary>
    public struct Chunk : IDisposable
    {
        public const           int   SIZE         = 32;
        public const           int   AREA         = SIZE * SIZE;
        public const           float INVERSE_SIZE = 1f / SIZE;
        public static readonly int2  SIZE2        = new int2(SIZE, SIZE);

        public Native2dArray<Cell> cells;

        public int2 position;
        public int2 worldPosition => position * SIZE;
        
        public int2 max => position * SIZE + SIZE2 - 1;
        public int2 min => position * SIZE;
        
        
        public Chunk(int2 position, Allocator allocator)
        {
            this.position = position;
            this.cells    = new Native2dArray<Cell>(SIZE2, allocator);
        }


        public bool IsInside(int2 worldPosition)
        {
            int2 myPos = this.worldPosition;
            return Bounds.IsInside(worldPosition, myPos, myPos + SIZE);
        }


        public bool Contains(int2 boundsMin, int2 boundsMax)
        {
            int2 myPos = this.worldPosition;
            return Bounds.Contains(myPos, myPos + SIZE, boundsMin, boundsMax);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int2 index) => index.x + index.y * SIZE;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y) => x + y * SIZE;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 GetIndex(int index)
        {
            int y = (int)math.floor(index / (float)SIZE);
            return new int2(index - y * SIZE, y);
        }
        
        
        public Cell this[int x, int y]
        {
            get => cells[GetIndex(x, y)];
            set => cells[GetIndex(x, y)] = value;
        }


        public Cell this[int2 index]
        {
            get => cells[GetIndex(index)];
            set => cells[GetIndex(index)] = value;
        }


        public Cell this[int index]
        {
            get => cells[index];
            set => cells[index] = value;
        }


        public void Dispose()
        {
            cells.Dispose();
        }
    }
}
