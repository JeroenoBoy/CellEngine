using System;
using System.Security.Cryptography.X509Certificates;
using CellEngine.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace World.Jobs
{
    [BurstCompile]
    public struct SimulationJob : IJobParallelFor
    {
        public WorldData worldData;
        public int2      offset;
        public uint      seed;
        
        public static readonly int2 down = new int2(0, -1);
        
        
        public void Execute(int index)
        {
            int2 chunkIndex = worldData.GetIndex(index * 2);
            chunkIndex.y *= 2;
            chunkIndex   += offset;
            
            if (!Bools.Any(worldData.worldSize > chunkIndex)) return;
            Chunk chunk    = worldData[chunkIndex];
            int2  chunkPos = chunk.worldPosition;

            Random random = new Random((uint)((index << 10) * 20) + seed);

            NativeList<int2x2> swaps = new (Chunk.SIZE, Allocator.Temp);
            NativeArray<bool>  bools = new (Chunk.SIZE, Allocator.Temp);

            for (int y = 0; y < Chunk.SIZE; y++) {
                
                //  First iteration
                
                for (int x = 0; x < Chunk.SIZE; x++) {
                    Cell cell     = chunk[x, y];
                    int2 worldPos = chunkPos + new int2(x,y);

                    bools[x] = cell.behaviour switch
                    {
                        CellBehaviour.Sand => ProcessGravity(worldPos, cell, ref swaps),
                        CellBehaviour.Water => ProcessGravity(worldPos, cell, ref swaps),
                        _                  => false
                    };
                }
                
                //  Second iteration

                for (int x = 0; x < Chunk.SIZE; x++) {
                    if (bools[x]) continue;
                    
                    Cell cell     = chunk[x, y];
                    int2 worldPos = chunkPos + new int2(x,y);

                    switch (cell.behaviour) {
                        case CellBehaviour.Sand: ProcessSand(worldPos, cell, ref random, ref swaps); break;
                        case CellBehaviour.Water: ProcessWater(worldPos, cell, ref random, ref swaps); break;
                    }
                }
                
                //  Applying swaps

                for (int i = 0; i < swaps.Length; i++) {
                    worldData.SwapCells(swaps[i]);
                }
                swaps.Clear();
            }
        }


        public bool ProcessGravity(int2 worldPos, Cell cell, ref NativeList<int2x2> swaps)
        {
            int2 otherPos = worldPos + down;
            if (!worldData.TryGetCell(otherPos, out Cell other) || cell.mass <= other.mass) return false;

            swaps.Add(new int2x2(worldPos, otherPos));
            return true;
        }


        public bool ProcessSand(int2 worldPos, Cell cell, ref Random random, ref NativeList<int2x2> swaps)
        {
            int2 otherPos = worldPos + down;
            int  state    = random.NextInt(0, 2);
            
            otherPos.x += state * 2 - 1;
            if (worldData.TryGetCell(otherPos, out Cell other) && cell.mass > other.mass) {
                swaps.Add(new int2x2(worldPos, otherPos));
                return true;
            }

            otherPos.x += state * -4 + 2;
            if (worldData.TryGetCell(otherPos, out other) && cell.mass > other.mass) {
                swaps.Add(new int2x2(worldPos, otherPos));
                return true;
            }
            return false;
        }


        public void ProcessWater(int2 worldPos, Cell cell, ref Random random, ref NativeList<int2x2> swaps)
        {
            int2 otherPos = worldPos;
            int  state    = random.NextInt(0, 2);
            
            otherPos.x += state * 2 - 1;
            if (worldData.TryGetCell(otherPos, out Cell other) && cell.mass > other.mass) {
                swaps.Add(new int2x2(worldPos, otherPos));
                return;
            }

            otherPos.x += state * -4 + 2;
            if (worldData.TryGetCell(otherPos, out other) && cell.mass > other.mass) {
                swaps.Add(new int2x2(worldPos, otherPos));
            }
        }
    }
}