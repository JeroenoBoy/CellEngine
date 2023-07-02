using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;



namespace CellEngine.World
{
    public class WorldInitializerAuthoring : MonoBehaviour
    {
        [SerializeField] private int2       _chunks;
        [SerializeField] private GameObject _airPrefab;
        

        private class WorldInitializerBaker : Baker<WorldInitializerAuthoring>
        {
            public override void Bake(WorldInitializerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponentObject(entity, new WorldInitializer()
                {
                    chunks = authoring._chunks,
                    airCellEntity = GetEntity(authoring._airPrefab, TransformUsageFlags.None)
                });
            }
        }
    }
}
