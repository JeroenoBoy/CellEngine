using CellEngine.World;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;



namespace CellEngine.Simulation
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct SandSystem : ISystem
    {
        private EntityQuery _worldSingletonQuery;
        private Random      _random;

        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _worldSingletonQuery = WorldSingleton.GetQuery(ref state);
            _random              = new Random();
            _random.InitState();
            
            state.RequireForUpdate<Sand>();
        }


        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
        
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            WorldSingleton worldSingleton = WorldSingleton.Get(_worldSingletonQuery);

            state.Dependency = new SandJob
            {
                worldSingleton = worldSingleton,
                random = _random
            }.Schedule(state.Dependency);
        }



        [BurstCompile]
        [WithAll(typeof(Sand))]
        private partial struct SandJob : IJobEntity
        {
            public WorldSingleton worldSingleton;
            public Random         random;

            private void Execute(Cell cell)
            {
                int2 myPos = cell.worldPosition;
                int2 down  = cell.worldPosition - new int2(0, 1);
                
                if (!worldSingleton.TryGetCell(down, out CellType cellType))return;
                
                if (cellType.value == 0) {
                    worldSingleton.RequestSwap(myPos, down);
                    return;
                }

                int state = random.NextInt(0, 1);
                for (int i = 2; i --> 0;) {
                    int2 pos = down + new int2((i + state) % 2 * 2 - 1, 0);
                    if (!worldSingleton.TryGetCell(pos, out cellType) || cellType.value != 0) continue;
                    worldSingleton.RequestSwap(myPos, pos);
                    return;
                }
            }
        }
    }
}
