using CellEngine.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;



namespace World.Jobs
{
    [BurstCompile]
    public struct RenderFilterJob : IJobParallelFor
    {
        public WorldData          worldData;
        [NativeDisableParallelForRestriction]
        public Native2dArray<int> results;

        public int2 cameraPosition;
        public int2 cameraMax;


        public void Execute(int index)
        {
            Chunk chunk = worldData[index];

            if (!chunk.Contains(cameraPosition, cameraMax)) return;

            int2 chunkPos = chunk.worldPosition;

            for (int x = Chunk.SIZE; x --> 0;)
            for (int y = Chunk.SIZE; y --> 0;) {
                int2 pos = chunkPos + new int2(x, y);
                if (!Bounds.IsInside(pos, cameraPosition, cameraMax)) continue; 
                results[pos - cameraPosition] = (int)chunk[x, y].behaviour;
            }
        }
    }
}   