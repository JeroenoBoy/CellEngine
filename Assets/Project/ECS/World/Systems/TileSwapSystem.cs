using CellEngine.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using ECBSingleton = Unity.Entities.EndSimulationEntityCommandBufferSystem.Singleton;


namespace CellEngine.World
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct TileSwapSystem : ISystem
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
            WorldSingleton   singleton    = WorldSingleton.Get(_worldSingletonQuery);
            ref ECBSingleton ecbSingleton = ref SystemAPI.GetSingletonRW<ECBSingleton>().ValueRW;
            
            if (singleton.tileSwapBuffer.Length == 0) return;

            NativeArray<TileSwapRequest> requests  = singleton.tileSwapBuffer.ToNativeArray(Allocator.Temp);
            WorldSingletonData           worldData = singleton.world;
            Native2dArray<Entity>        entities  = worldData.entities;
            Native2dArray<CellType>      cells     = worldData.cells;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            
            singleton.tileSwapBuffer.Clear();

            for (int i = requests.Length; i --> 0;) {
                TileSwapRequest request = requests[i];

                Profiler.BeginSample("Receiving Phase");
                int2 worldPosA = request.positionA;
                int2 worldPosB = request.positionB;

                Chunk chunkA = Chunk.GetChunkData(worldPosA);
                Chunk chunkB = Chunk.GetChunkData(worldPosB);

                int2 localPosA = Chunk.GetCellChunkPosition(worldPosA);
                int2 localPosB = Chunk.GetCellChunkPosition(worldPosB);

                Entity entityA = entities[worldPosA];
                Entity entityB = entities[worldPosB];

                CellType cellTypeA = cells[worldPosA];
                CellType cellTypeB = cells[worldPosB];
                Profiler.EndSample();

                Profiler.BeginSample("Applying phase");
                Profiler.BeginSample("ECB");
                ecb.SetComponent(entityA, new CellPosition(localPosB));
                ecb.SetSharedComponent(entityA, chunkB);
                
                ecb.SetComponent(entityB, new CellPosition(localPosA));
                ecb.SetSharedComponent(entityB, chunkA);
                Profiler.EndSample();

                Profiler.BeginSample("WorldData");
                entities[worldPosB] = entityA;
                entities[worldPosA] = entityB;

                cells[worldPosB] = cellTypeA;
                cells[worldPosA] = cellTypeB;
                Profiler.EndSample();
                Profiler.EndSample();
            }

            Profiler.BeginSample("Playback");
            requests.Dispose();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            Profiler.EndSample();
        }
    }
}
