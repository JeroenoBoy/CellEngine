using System;
using Unity.Jobs;
using Unity.Mathematics;
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
            Chunk chunk = worldData[index * 4 + offset];
            int2  chunkPos = chunk.worldPosition;

            Random random = new Random((uint)((index << 10) * 20) + seed);
            
            for (int x = 0; x < Chunk.SIZE; x++)
            for (int y = 0; y < Chunk.SIZE; y++) {
                Cell cell     = chunk[x, y];
                int2 worldPos = chunkPos + new int2(x,y);

                switch (cell.behaviour) {
                    case CellBehaviour.Air: continue;
                    case CellBehaviour.Sand: ProcessSand(worldPos, ref random); continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        public void ProcessSand(int2 worldPos, ref Random random)
        {
            int2 otherPos = worldPos + down;
            
            if (worldData.TryGetCell(otherPos, out Cell other) && other.behaviour == CellBehaviour.Air) {
                worldData.SwapCells(worldPos, otherPos);
                return;
            }

            int state = random.NextInt(0, 2);
            otherPos.x += state * 2 - 1;
            
            if (worldData.TryGetCell(otherPos, out other) && other.behaviour == CellBehaviour.Air) {
                worldData.SwapCells(worldPos, otherPos);
                return;
            }

            otherPos.x -= state * -4 - 2;

            if (worldData.TryGetCell(otherPos, out other) && other.behaviour == CellBehaviour.Air) {
                worldData.SwapCells(worldPos, otherPos);
            }
        }
    }
}
