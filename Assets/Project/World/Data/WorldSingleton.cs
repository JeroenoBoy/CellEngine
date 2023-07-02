using CellEngine.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;



namespace CellEngine.World
{
    public readonly struct WorldSingletonData : IComponentData
    {
        public readonly NativeArray<Chunk>      chunks;
        public readonly Native2dArray<CellType> cells;
        public readonly Native2dArray<Entity>   entities;

        public readonly int width;
        public readonly int height;

        public WorldSingletonData(NativeArray<Chunk> chunks, Native2dArray<CellType> cells, Native2dArray<Entity> entities)
        {
            this.chunks   = chunks;
            this.cells    = cells;
            this.entities = entities;
            
            width  = cells.width;
            height = cells.height;
        }
    }



    public readonly struct TileSwapRequest : IBufferElementData
    {
        public readonly int2 positionA;
        public readonly int2 positionB;


        public TileSwapRequest(int2 positionA, int2 positionB)
        {
            this.positionA = positionA;
            this.positionB   = positionB;
        }
    }



    public readonly struct TileCreateRequest : IBufferElementData
    {
        public readonly int2     position;
        public readonly Entity   prefab;


        public TileCreateRequest(int2 position, Entity prefab)
        {
            this.position = position;
            this.prefab   = prefab;
        }
    }


    public readonly struct WorldSingleton
    {
        
        public static EntityQuery GetQuery(ref SystemState state, bool addDependency = true)
        {
            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<WorldSingletonData, TileSwapRequest, TileCreateRequest>();
            EntityQuery query = builder.Build(ref state);
            builder.Dispose();

            if (addDependency) {
                state.RequireForUpdate<WorldSingletonData>();
            }
            
            return query;
        }


        public static EntityQuery GetQuery(EntityManager manager)
        {
            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<WorldSingletonData, TileSwapRequest, TileCreateRequest>();
            EntityQuery query = builder.Build(manager);
            builder.Dispose();

            return query;
        }
        
        
        public static WorldSingleton Get(EntityQuery worldSingletonQuery)
        {
            return new WorldSingleton(
                world: worldSingletonQuery.GetSingleton<WorldSingletonData>(),
                tileSwapBuffer: worldSingletonQuery.GetSingletonBuffer<TileSwapRequest>(),
                tileCreateRequests: worldSingletonQuery.GetSingletonBuffer<TileCreateRequest>()
            );
        }
        
    
        public readonly WorldSingletonData               world;
        public readonly DynamicBuffer<TileSwapRequest>   tileSwapBuffer;
        public readonly DynamicBuffer<TileCreateRequest> tileCreateRequests;

        public NativeArray<Chunk>      chunks => world.chunks;
        public Native2dArray<CellType> cells  => world.cells;
        
        public int width  => world.width;
        public int height => world.height;

        public int2 boundsMax => new (width, height);
        public int2 boundsMin => int2.zero;
        
        
        public WorldSingleton(WorldSingletonData world, DynamicBuffer<TileSwapRequest> tileSwapBuffer, DynamicBuffer<TileCreateRequest> tileCreateRequests)
        {
            this.world              = world;
            this.tileSwapBuffer     = tileSwapBuffer;
            this.tileCreateRequests = tileCreateRequests;
        }


        public CellType GetCell(int2 position)
        {
            TryGetCell(position, out CellType cellType);
            return cellType;
        }


        public bool TryGetCell(int2 position, out CellType cellType)
        {
            if (!Bounds.IsInside(position, boundsMax, boundsMin)) {
                cellType = CellType.Invalid;
                return false;
            }
            
            cellType = cells[position];
            return true;
        }


        public bool IsInside(int2 position) => Bounds.IsInside(position, boundsMax, boundsMin);


        public Entity GetEntityAt(int2 worldPosition)
        {
            return world.entities[worldPosition];
        }


        public bool TryGetEntity(int2 worldPosition, out Entity entity)
        {
            if (IsInside(worldPosition)) {
                entity = Entity.Null;
                return false;
            }

            entity = world.entities[worldPosition];
            return true;
        }


        public bool TryRequestSwap(int2 from, int2 to)
        {
            if (!IsInside(from) || !IsInside(to)) return false;
            RequestSwap(from, to);
            return true;
        }


        public void RequestSwap(int2 from, int2 to)
        {
            RequestSwap(new TileSwapRequest(from, to));
        }
         

        public void RequestSwap(TileSwapRequest request)
        {
            tileSwapBuffer.Add(request);
        }


        public void SpawnTile(int2 position, Entity prefab)
        {
            tileCreateRequests.Add(new TileCreateRequest(position, prefab));
        }
    }
}
