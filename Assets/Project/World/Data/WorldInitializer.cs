using Unity.Entities;
using Unity.Mathematics;



namespace CellEngine.World
{
    [System.Serializable]
    public class WorldInitializer : IComponentData
    {
        public int2 chunks;
        public Entity airCellEntity;
    }
}
