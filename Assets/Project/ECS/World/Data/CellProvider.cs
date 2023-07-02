using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Serialization;



namespace CellEngine.World
{
    public struct CellProvider : IBufferElementData
    {
        public Entity   prefab;
        public CellType cellType;
    }
}
