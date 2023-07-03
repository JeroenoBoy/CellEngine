using CellEngine.Utilities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;



namespace World.Jobs
{
    [BurstCompile]
    public struct FillWorldJob : IJobParallelFor
    {
        public WorldData    worldData;
        public CellTemplate template;

        public int2 worldPosition;
        public int2 boundsMax;

        
        public void Execute(int index)
        {
            Chunk chunk     = worldData.chunks[index];
            if (!chunk.Contains(worldPosition, boundsMax)) return;

            int2 chunkWorldPos = chunk.worldPosition;
            
            for (int x = Chunk.SIZE; x --> 0;)
            for (int y = Chunk.SIZE; y --> 0;) {
                int2 worldPos = chunkWorldPos + new int2(x,y);
                if (!Bounds.IsInside(worldPos, worldPosition, boundsMax)) return;
                chunk[x, y] = new Cell(template, x, y);
            }
        }
    }
}
