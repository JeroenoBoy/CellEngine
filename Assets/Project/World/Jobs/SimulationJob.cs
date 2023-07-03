﻿using System;
using System.Security.Cryptography.X509Certificates;
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

            NativeArray<bool> bools = new (Chunk.SIZE, Allocator.Temp);

            for (int y = 0; y < Chunk.SIZE; y++) {
                
                //  First iteration
                
                for (int x = 0; x < Chunk.SIZE; x++) {
                    Cell cell     = chunk[x, y];
                    int2 worldPos = chunkPos + new int2(x,y);

                    bools[x] = cell.behaviour switch
                    {
                        CellBehaviour.Sand => ProcessSand(worldPos),
                        _                  => false
                    };
                }
                
                //  Second iteration

                for (int x = 0; x < Chunk.SIZE; x++) {
                    if (bools[x]) continue;
                    
                    Cell cell     = chunk[x, y];
                    int2 worldPos = chunkPos + new int2(x,y);

                    switch (cell.behaviour) {
                        case CellBehaviour.Sand: ProcessSand2(worldPos, ref random); continue;
                    }
                }
            }
        }


        public bool ProcessSand(int2 worldPos)
        {
            int2 otherPos = worldPos + down;
            if (!worldData.TryGetCell(otherPos, out Cell other) || other.behaviour != CellBehaviour.Air) return false;
            
            worldData.SwapCells(worldPos, otherPos);
            return true;

        }


        public void ProcessSand2(int2 worldPos, ref Random random)
        {
            int2 otherPos = worldPos + down;
            int  state    = random.NextInt(0, 2);
            
            otherPos.x += state * 2 - 1;
            if (worldData.TryGetCell(otherPos, out Cell other) && other.behaviour == CellBehaviour.Air) {
                worldData.SwapCells(worldPos, otherPos);
                return;
            }

            otherPos.x += state * -4 + 2;
            if (worldData.TryGetCell(otherPos, out other) && other.behaviour == CellBehaviour.Air) {
                worldData.SwapCells(worldPos, otherPos);
            }
        }
    }
}