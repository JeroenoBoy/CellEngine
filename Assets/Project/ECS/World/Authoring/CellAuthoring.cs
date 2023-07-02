using JUtils.Attributes;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;



namespace CellEngine.World
{
    public class CellAuthoring : MonoBehaviour
    {
        [SerializeField] private int type;
        [SerializeField, SerializeReference, TypeSelector] private ICellTagComponent[] _components;

        public CellType cellType => new CellType(type);
        
        
        private class CellBaker : Baker<CellAuthoring>
        {
            public override void Bake(CellAuthoring authoring)
            {
                Entity entity = GetEntityWithoutDependency();
                AddSharedComponent(entity, new Chunk()  { chunkPosition = new int2(-1, -1) });
                AddComponent(entity, new CellPosition() { value = new int2(-1, -1) });
                AddComponent(entity, authoring.cellType);
                
                foreach (ICellTagComponent component in authoring._components) {
                    AddComponent(entity, new ComponentType(component.GetType()));
                }
            }
        }
    }
}