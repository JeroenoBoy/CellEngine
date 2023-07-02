using System;
using Unity.Entities;
using Unity.Mathematics;



namespace CellEngine.World
{
    public struct CellPosition : IComponentData
    {
        public int2 value;


        public CellPosition(int2 chunkPosition)
        {
            value = chunkPosition;
        }
    }



    public struct CellType : IComponentData, IEquatable<CellType>
    {
        public static readonly CellType Invalid = new CellType { value = -1};

        public int value;


        public CellType(int type)
        {
            value = type;
        }


        public bool Equals(CellType other)
        {
            return value == other.value;
        }


        public override bool Equals(object obj)
        {
            return obj is CellType other && Equals(other);
        }


        public override int GetHashCode()
        {
            return value;
        }


        public static bool operator ==(CellType a, CellType b) => a.Equals(b);
        public static bool operator !=(CellType a, CellType b) => !a.Equals(b);
        public static bool operator ==(CellType a, int b) => a.value.Equals(b);
        public static bool operator !=(CellType a, int b) => !a.value.Equals(b);
    }



    public readonly partial struct Cell : IAspect
    {
        private readonly Chunk               _chunk;
        private readonly RefRO<CellPosition> _cellPosition;
        private readonly RefRO<CellType>     _cellType;

        public int2     localPosition => _cellPosition.ValueRO.value;
        public int2     worldPosition => _chunk.chunkPosition * Chunk.SIZE + localPosition;
        public CellType cellType      => _cellType.ValueRO;
    }
}
