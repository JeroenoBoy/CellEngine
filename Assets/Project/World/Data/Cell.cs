using System;
using Unity.Mathematics;
using UnityEngine;



namespace CellEngine.World
{
    public enum CellBehaviour
    {
        Air   = 0,
        Sand  = 1,
        Water = 2
    }
    
    
    public struct Cell
    {
        public const    byte WALL_TYPE = 255;
        public const    byte AIR       = 0;
        public static readonly Cell wall = new Cell() { cellType = WALL_TYPE};

        public int2          localPosition;
        public float2        worldPosition;
        public float2        velocity;
        public byte          cellType;
        public CellBehaviour behaviour;
        public int           mass;


        public Cell(CellTemplate template, int2 localPosition, float2 worldPosition)
        {
            this.behaviour     = template.behaviour;
            this.localPosition = localPosition;
            this.worldPosition = worldPosition;
            this.velocity      = float2.zero;
            this.cellType      = template.cellType;
            this.mass          = template.mass;
        }


        public Cell(CellTemplate template, int x, int y, Chunk chunk) : this(template, new int2(x,y), new int2(x, y) + chunk.worldPosition) {}
    }

    
    
    [Serializable]
    public struct CellTemplate
    {
        public byte          cellType;
        public Color         color;
        public CellBehaviour behaviour;
        public int           mass;
    }
}