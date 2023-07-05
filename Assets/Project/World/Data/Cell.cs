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
        public int2          localPosition;
        public float2        worldPosition;
        public float2        velocity;
        public byte          cellType;
        public CellBehaviour behaviour;
        public int           mass;


        public Cell(CellTemplate template, int2 localPosition)
        {
            this.behaviour     = template.behaviour;
            this.localPosition = localPosition;
            this.worldPosition = localPosition;
            this.velocity      = int2.zero;
            this.cellType      = template.cellType;
            this.mass          = template.mass;
        }


        public Cell(CellTemplate template, int x, int y) : this(template, new int2(x,y)) {}
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