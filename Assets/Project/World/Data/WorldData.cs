using System;
using System.Runtime.CompilerServices;
using CellEngine.Utilities;
using CellEngine.World.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Bounds = CellEngine.Utilities.Bounds;



namespace CellEngine.World
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

            boundsMax = chunks * Chunk.SIZE - 1;
            boundsMin = int2.zero;
            
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
                boundsMax = worldPosition + size - 1
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


        public bool CellCast(int2 from, int2 to, out CellCastHit cellCastHit, int skip = 1)
        {
            WorldData self = this;
            Chunk     currentChunk;
            int2      chunkPos;
            int2      maxPos;
            
            GetChunk(from);

            int2 dir = to - from;
            int2 abs = math.abs(dir);
            
            int  i;
            cellCastHit = CellCastHit.WallHit(from);

            if (abs.x >= abs.y) {
                float yStep = math.abs(dir.y / (float)dir.x) * math.sign(dir.y);
                int   sign  = (int)math.sign(dir.x);
                for (i = skip; i < abs.x; i++) {
                    int2 worldPos = new int2(from.x + i * sign, (int)math.floor(from.y + (i + .5f) * yStep));

                    if (!IsInBounds(worldPos)) break;
                    if (!Bounds.IsInside(worldPos, chunkPos, maxPos)) GetChunk(worldPos);

                    Cell cell = currentChunk[worldPos - chunkPos];

                    if (cell.cellType == Cell.AIR) {
                        cellCastHit = CellCastHit.WallHit(worldPos);
                    }
                    else {
                        cellCastHit = new CellCastHit(cell, worldPos);
                        return true;
                    }
                }
            }
            else {
                float xStep = math.abs(dir.x / (float)dir.y) * math.sign(dir.x);
                int   sign  = (int)math.sign(dir.y);
                for (i = skip; i < abs.y; i++) {
                    int2 worldPos = new int2((int)math.floor(from.x + (i + .5f) * xStep), from.y + i * sign);

                    if (!IsInBounds(worldPos)) break;
                    if (!Bounds.IsInside(worldPos, chunkPos, maxPos)) GetChunk(worldPos);
                    
                    Cell cell = currentChunk[worldPos - chunkPos];
                    
                    if (cell.cellType == Cell.AIR) {
                        cellCastHit = CellCastHit.WallHit(worldPos);
                    }
                    else {
                        cellCastHit = new CellCastHit(cell, worldPos);
                        return true;
                    }
                }
            }

            return i < math.max(abs.x, abs.y);

            void GetChunk(int2 worldPos)
            {
                currentChunk = self.GetChunkAt(worldPos);
                chunkPos     = currentChunk.worldPosition;
                maxPos       = currentChunk.max;
            }
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

            cellA.worldPosition = worldPosB + cellA.worldPosition % 1;
            cellB.worldPosition = worldPosA + cellB.worldPosition % 1;

            chunkB[indexB] = cellA;
            chunkA[indexA] = cellB;
        }


        public Chunk GetChunkAt(int2 worldPosition)
        {
            return chunks[(int2)math.floor((float2)worldPosition * Chunk.INVERSE_SIZE)];
        }


        public bool IsInBounds(int2 position)
        {
            return Bounds.IsInside(position, boundsMin, boundsMax);
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



    public readonly struct CellCastHit
    {
        public static CellCastHit WallHit(int2 worldPosition)
        {
            Cell cell = Cell.wall;
            cell.worldPosition = worldPosition;
            return new CellCastHit(cell, worldPosition);
        }
        
        public readonly Cell cell;
        public readonly int2 worldPosition;

        public bool hitBounds => cell.cellType == Cell.WALL_TYPE;

        
        public CellCastHit(Cell cell, int2 worldPosition)
        {
            this.cell          = cell;
            this.worldPosition = worldPosition;
        }
    }
}
