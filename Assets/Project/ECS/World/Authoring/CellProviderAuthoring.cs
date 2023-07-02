using Unity.Collections;
using Unity.Entities;
using UnityEngine;



namespace CellEngine.World
{
    public class CellProviderAuthoring : MonoBehaviour
    {
        [SerializeField] private CellAuthoring[] _cells;
        
        
        public class CellProviderBaker : Baker<CellProviderAuthoring>
        {
            public override void Bake(CellProviderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                DynamicBuffer<CellProvider> providers = AddBuffer<CellProvider>(entity);

                foreach (CellAuthoring cellAuthoring in authoring._cells) {
                    Entity prefab = GetEntity(cellAuthoring.gameObject, TransformUsageFlags.None);
                    providers.Add(new CellProvider {prefab = prefab, cellType = cellAuthoring.cellType});
                }
            }
        }
    }
}
