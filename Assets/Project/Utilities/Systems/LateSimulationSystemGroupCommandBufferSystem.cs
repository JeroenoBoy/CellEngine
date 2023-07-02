using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;



namespace CellEngine.Utilities
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderFirst = true)]
    public partial class LateSimulationEcbSystem : EcbSingletonSystem
    { 
        
    }
}
