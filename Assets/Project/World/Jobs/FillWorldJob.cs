using Unity.Burst;
using Unity.Jobs;



namespace World.Jobs
{
    [BurstCompile]
    public struct FillWorldJob : IJobParallelFor
    {
        public CellWorld world;
        public Cell      template;

        
        public void Execute(int index)
        {
            Chunk chunk = world.chunks[index];
            
            for (int x = Chunk.SIZE; x --> 0;)
            for (int y = Chunk.SIZE; y --> 0;) {
                chunk.cells[x, y] = new Cell(template, x, y);
            }
        }
    }
}
