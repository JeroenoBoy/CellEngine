using CellEngine.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;



namespace CellEngine.World
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(TileSwapSystem))]
    public partial struct TileRequestSystem : ISystem
    {
        private EntityQuery _worldSingletonQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _worldSingletonQuery = WorldSingleton.GetQuery(ref state);
        }


        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            WorldSingleton      worldSingleton = WorldSingleton.Get(_worldSingletonQuery);
            EntityCommandBuffer ecb            = new EntityCommandBuffer(Allocator.Temp);

            DynamicBuffer<TileCreateRequest> requests = worldSingleton.tileCreateRequests;

            Native2dArray<Entity>   entities  = worldSingleton.world.entities;
            Native2dArray<CellType> cellTypes = worldSingleton.world.cells;

            for (int i =  requests.Length; i --> 0;) {
                TileCreateRequest request = requests[i];
                int               index   = entities.GetIndex(request.position);

                ecb.DestroyEntity(entities[index]);

                Entity entity = state.EntityManager.Instantiate(request.prefab);
                ecb.SetSharedComponent(entity, Chunk.GetChunkData(request.position));
                ecb.SetComponent(entity, new CellPosition(Chunk.GetCellChunkPosition(request.position)));

                entities[index]  = entity;
                cellTypes[index] = state.EntityManager.GetComponentData<CellType>(entity);
            }

            requests.Clear();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
