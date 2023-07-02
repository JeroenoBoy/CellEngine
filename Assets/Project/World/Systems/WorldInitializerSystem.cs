using CellEngine.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace CellEngine.World
{
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class WorldInitializerSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<WorldInitializer>();
        }


        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new (Allocator.TempJob);
            
            //  Getting world initializer singleton
            
            bool found = GetWorldSingletonEntity(out WorldSingletonData worldSingletonData, out Entity worldSingletonEntity);
            GetInitializer(out WorldInitializer initializer, out Entity initializerEntity);

            int chunkWidth = initializer.chunks.x;
            int chunkHeight     = initializer.chunks.y;

            // Destroying old entities

            if (found) { 
                ecb.DestroyEntity(worldSingletonEntity);
                worldSingletonData.chunks.Dispose();
                worldSingletonData.cells.Dispose();
            }
            
            ecb.DestroyEntity(initializerEntity);
            Entities.WithAll<Chunk>().ForEach((Entity entity) => ecb.DestroyEntity(entity)).WithBurst().Run();
            
            //  Creating new entities
            
            Dependency = new CreateEntitiesJob
            {
                width  = initializer.chunks.x,
                ecb    = ecb.AsParallelWriter(),
                prefab = initializer.airCellEntity
            }.Schedule(chunkWidth * chunkHeight, 4, Dependency);
            Dependency.Complete();
            
            // Applying
            
            ecb.Playback(EntityManager);
            ecb.Dispose();

            //  Setup new world singleton

            NativeArray<Chunk>      chunks    = new (chunkWidth * chunkHeight, Allocator.Persistent);
            Native2dArray<CellType> cellTypes = new (initializer.chunks * Chunk.SIZE, Allocator.Persistent);
            Native2dArray<Entity>   entities  = new (initializer.chunks * Chunk.SIZE, Allocator.Persistent);

            for (int x = chunkWidth; x --> 0;)
            for (int y = chunkHeight; y --> 0;) {
                chunks[x + y * chunkWidth] = new Chunk {chunkPosition = new int2(x, y)};
            }

            Dependency = new GetEntityDataJob {entities = entities, cellTypes = cellTypes}.Schedule(Dependency);
            Dependency.Complete();

            // Applying

            ecb = new EntityCommandBuffer(Allocator.Temp);

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new WorldSingletonData(chunks, cellTypes, entities));
            ecb.AddBuffer<TileSwapRequest>(entity);
            ecb.AddBuffer<TileCreateRequest>(entity);
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }


        private void GetInitializer(out WorldInitializer initializer, out Entity entity)
        {
            WorldInitializer foundInitializer  = default;
            Entity           initializerEntity = default;
            Entities.WithAll<WorldInitializer>().ForEach((in WorldInitializer worldInitializer, in Entity entity) => {
                    foundInitializer       = worldInitializer;
                    initializerEntity = entity;
            }).WithoutBurst().Run();

            initializer = foundInitializer;
            entity      = initializerEntity;
        }


        private bool GetWorldSingletonEntity(out WorldSingletonData worldSingletonData, out Entity entity)
        {
            WorldSingletonData singletonData     = default;
            Entity             initializerEntity = Entity.Null;
            Entities.WithAll<WorldSingletonData>().ForEach((in WorldSingletonData data, in Entity entity) =>
            {
                singletonData     = data;
                initializerEntity = entity;
            }).WithoutBurst().Run();

            worldSingletonData = singletonData;
            entity             = initializerEntity;

            return entity != Entity.Null;
        }
        
        
        
        [BurstCompile]
        private struct CreateEntitiesJob : IJobParallelFor
        {
            public int    width;
            public Entity prefab;

            public EntityCommandBuffer.ParallelWriter ecb;

            
            public void Execute(int index)
            {
                int2  chunkPosition = int2(index % width, (int)floor(index / (float)width));
                Chunk chunk         = new Chunk {chunkPosition = chunkPosition};
                
                for (int x = Chunk.SIZE; x --> 0;)
                for (int y = Chunk.SIZE; y --> 0;) {
                    Entity result = ecb.Instantiate(index, prefab);
                    ecb.SetSharedComponent(index, result, chunk);
                    ecb.SetComponent(index, result, new CellPosition() { value = new int2(x,y) });
                }
            }
        }
        
        
        
        [BurstCompile]
        private partial struct GetEntityDataJob : IJobEntity
        {
            public Native2dArray<Entity> entities;
            public Native2dArray<CellType>    cellTypes;

            private void Execute(in Entity entity, Cell cell)
            {
                int i = entities.GetIndex(cell.worldPosition);
                entities[i]  = entity;
                cellTypes[i] = cell.cellType;
            }
        }
    }
}
