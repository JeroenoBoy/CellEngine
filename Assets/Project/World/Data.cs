using System;
using CellEngine.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using World.Jobs;



namespace World
{
    /// <summary>
    /// The struct containing all the data of the world
    /// </summary>
    public readonly struct CellWorld : IDisposable
    {
        public readonly Native2dArray<Chunk> chunks;

        public readonly int2 size => chunks.size;
        public readonly int2 boundsMax;
        public readonly int2 boundsMin;

        public bool isCreated => chunks.isCreated;

        public CellWorld(int2 chunks, Allocator allocator)
        {
            this.chunks = new Native2dArray<Chunk>(chunks, allocator);
            boundsMax   = chunks * Chunk.SIZE;
            boundsMin   = int2.zero;
            
            for (int x = chunks.x; x --> 0;)
            for (int y = chunks.y; y --> 0;) {
                this.chunks[x, y] = new Chunk(new int2(x, y), allocator);
            }
        }


        public void Fill(Cell template)
        {
            new FillWorldJob()
            {
                world = this,
                template = template
            }.Schedule(chunks.length, 1).Complete();
        }


        public void SwapCells(int2 posA, int2 posB)
        {
        }


        public Chunk GetChunkAt(int2 worldPosition)
        {
            return chunks[(int2)math.floor((float2)worldPosition * Chunk.INVERSE_SIZE)];
        }


        public bool IsInBounds(int2 position)
        {
            return Bounds.IsInside(position, boundsMax, boundsMin);
        }


        public void Dispose()
        {
            for (int i = chunks.length; i --> 0;)
                chunks[i].Dispose();
            chunks.Dispose();
        }
    }



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
        
        public int2 max => position * SIZE + SIZE2;
        public int2 min => position * SIZE;
        
        
        public Chunk(int2 position, Allocator allocator)
        {
            this.position = position;
            this.cells    = new Native2dArray<Cell>(SIZE2, allocator);
        }


        public void Dispose()
        {
            cells.Dispose();
        }
    }
    

    
    /// <summary>
    /// The representation of a cell on the grid
    /// </summary>
    [System.Serializable]
    public struct Cell
    {
       public int2 position;
       public int  type;


        public Cell(int2 position, int type)
        {
            this.position = position;
            this.type     = type;
        }


        public Cell(Cell template, int2 position)
        {
            this.type     = template.type;
            this.position = position;
        }


        public Cell(Cell template, int x, int y)
        {
            type     = template.type;
            position = new int2(x, y);
        }
    }
}
