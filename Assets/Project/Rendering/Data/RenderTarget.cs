using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Tilemaps;



namespace CellEngine.Rendering
{
    public class RenderTarget : IComponentData, ICloneable
    {
        public readonly Tilemap    target;
        public readonly TileBase[] availableTiles;
        public readonly int2       position;


        public RenderTarget() {}
        public RenderTarget(Tilemap target, TileBase[] availableTiles, int2 position)
        {
            this.target         = target;
            this.availableTiles = availableTiles;
            this.position       = position;
        }
        
        
        public object Clone()
        {
            return new RenderTarget(target, availableTiles, position);
        }
    }
}
