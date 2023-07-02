using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;



namespace World.Rendering
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapRenderer : CellWorldRenderer
    {
        [SerializeField] private int2 cameraSize;

        private Tilemap _tilemap;
        
        
        public override void OnWorldSetup(CellWorld world)
        {
            transform.localScale = Vector3.one * Chunk.INVERSE_SIZE;
            _tilemap.size        = new Vector3Int(cameraSize.x * Chunk.SIZE, cameraSize.y * Chunk.SIZE);
        }

        
        public override void OnRender(CellWorld world)
        {
            
        }


        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }
    }
}
