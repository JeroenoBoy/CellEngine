using System;
using System.Security.Cryptography.X509Certificates;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;



namespace World.Jobs
{
    public struct SimulationJob : IJobParallelFor
    {
        public WorldData worldData;
        public int       offset;
        public uint      seed;
        
        public static readonly int2 down = new int2(0, -1);
        
        
        public void Execute(int index)
        {
            int chunkIndex = index * 4 + offset;
            if (chunkIndex >= worldData.length) return;
            Chunk chunk    = worldData[chunkIndex];
            int2  chunkPos = chunk.worldPosition;

            Random random = new Random((uint)((index << 10) * 20) + seed);

            NativeList<int2x2> swaps = new (Chunk.AREA, Allocator.TempJob);

            for (int y = 0; y < Chunk.SIZE; y++) {
                for (int x = 0; x < Chunk.SIZE; x++) {
                    Cell cell     = chunk[x, y];
                    int2 worldPos = chunkPos + new int2(x,y);

                    switch (cell.behaviour) {
                        case CellBehaviour.Air: continue;
                        case CellBehaviour.Sand: ProcessSand(worldPos, ref random, ref swaps); continue;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                for (int i = 0; i < swaps.Length; i++) {
                    worldData.SwapCells(swaps[i]);
                }
                
                swaps.Clear();
            }
        }


        public void ProcessSand(int2 worldPos, ref Random random, ref NativeList<int2x2> swaps)
        {
            int2 otherPos = worldPos + down;
            
            if (worldData.TryGetCell(otherPos, out Cell other) && other.behaviour == CellBehaviour.Air) {
                swaps.Add(new int2x2(worldPos, otherPos));
                return;
            }

            int state = random.NextInt(0, 2);
            
            otherPos.x += state * 2 - 1;
            if (worldData.TryGetCell(otherPos, out other) && other.behaviour == CellBehaviour.Air) {
                swaps.Add(new int2x2(worldPos, otherPos));
                return;
            }

            otherPos.x += state * -4 + 2;
            if (worldData.TryGetCell(otherPos, out other) && other.behaviour == CellBehaviour.Air) {
                swaps.Add(new int2x2(worldPos, otherPos));
            }
        }
    }
}