using System;
using System.Runtime.CompilerServices;
using CellEngine.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using World.Jobs;
using Bounds = CellEngine.Utilities.Bounds;



namespace World
{
    /// <summary> x
    /// The struct containing all the data of the world
    /// </summary>
    public struct WorldData : IDisposable
    {
        public Unsafe2dArray<Chunk> chunks;

        public readonly int2 chunkSize => chunks.size;
        public readonly int  length    => chunks.length;
        public readonly int2 worldSize => chunks.size * Chunk.SIZE;
        public readonly int2 boundsMax;
        public readonly int2 boundsMin;

        public bool isCreated => chunks.isCreated;

        public WorldData(int2 chunks, Allocator allocator)
        {
            this.chunks = new Unsafe2dArray<Chunk>(chunks, allocator);
            
            boundsMax   = chunks * Chunk.SIZE;
            boundsMin   = int2.zero;
            
            for (int x = chunks.x; x --> 0;)
            for (int y = chunks.y; y --> 0;) {
                this.chunks[x, y] = new Chunk(new int2(x, y), allocator);
            }
        }


        public void Fill(CellTemplate template, int2 worldPosition, int2 size)
        {
            new FillWorldJob()
            {
                worldData = this,
                template = template,
                worldPosition = worldPosition,
                boundsMax = worldPosition + size
            }.Schedule(chunks.length, 16).Complete();
        }
        

        public void Fill(CellTemplate template)
        {
            Fill(template, int2.zero, worldSize);
        }

        
        public bool TryGetCell(int2 worldPos, out Cell cell)
        {
            if (!IsInBounds(worldPos)) {
                cell = default;
                return false;
            }

            Chunk chunk = GetChunkAt(worldPos);
            cell = chunk[worldPos - chunk.worldPosition];
            return true;
        }


        public void SwapCells(int2x2 cells) => SwapCells(cells.c0, cells.c1);
        public void SwapCells(int2 worldPosA, int2 worldPosB)
        {
            Chunk chunkA = GetChunkAt(worldPosA);
            Chunk chunkB = GetChunkAt(worldPosB);

            int2 indexA = worldPosA - chunkA.worldPosition;
            int2 indexB = worldPosB - chunkB.worldPosition;
            
            Cell cellA = chunkA[indexA];
            Cell cellB = chunkB[indexB];

            chunkB[indexB] = cellA;
            chunkA[indexA] = cellB;
        }


        public Chunk GetChunkAt(int2 worldPosition)
        {
            return chunks[(int2)math.floor((float2)worldPosition * Chunk.INVERSE_SIZE)];
        }


        public bool IsInBounds(int2 position)
        {
            return Bounds.IsInside(position, boundsMin, boundsMax - new int2(1, 1));
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int2 index) => index.x + index.y * chunks.width;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y) => x + y * chunks.width;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 GetIndex(int index)
        {
            int y = (int)math.floor(index / (float)chunks.width);
            return new int2(index - y * chunks.width, y);
        }
        
        
        public Chunk this[int x, int y]
        {
            get => chunks[GetIndex(x, y)];
            set => chunks[GetIndex(x, y)] = value;
        }


        public Chunk this[int2 index]
        {
            get => chunks[GetIndex(index)];
            set => chunks[GetIndex(index)] = value;
        }


        public Chunk this[int index]
        {
            get => chunks[index];
            set => chunks[index] = value;
        }

        
        public void Dispose()
        {
            for (int i = chunks.length; i-- > 0;) {
                chunks[i].Dispose();
            }
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


    public enum CellBehaviour
    {
        Air = 0,
        Sand = 1
    }
    
    
    public struct Cell
    {
       public int2 position;
       public byte cellType;
       public CellBehaviour behaviour;


        public Cell(CellTemplate template, int2 position)
        {
            this.behaviour = template.behaviour;
            this.position  = position;
            this.cellType  = template.cellType;
        }


        public Cell(CellTemplate template, int x, int y) : this(template, new int2(x,y)) {}
    }

    
    
    [Serializable]
    public struct CellTemplate
    {
        public byte          cellType;
        public CellBehaviour behaviour;
    }
}
