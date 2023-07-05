using System.Collections.Generic;
using CellEngine.Utilities;
using CellEngine.World.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;



namespace CellEngine.World.Rendering
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapCellRenderer : CellWorldRenderer
    {
        [SerializeField] private int2       _cameraSize;
        [SerializeField] private TileBase[] _tilesBases;

        private Tilemap            _tilemap;
        private Native2dArray<byte> _tiles;
        private Native2dArray<byte> _previousTiles;

        private List<Vector3Int> _keys;
        private List<TileBase>   _values;


        public override void OnWorldSetup(WorldData worldData)
        {
            Dispose();
            
            transform.localScale = Vector3.one * Chunk.INVERSE_SIZE;
            _tilemap.size        = new Vector3Int(_cameraSize.x * Chunk.SIZE, _cameraSize.y * Chunk.SIZE);

            _tiles         = new Native2dArray<byte>(worldData.worldSize, Allocator.Persistent);
            _previousTiles = new Native2dArray<byte>(worldData.worldSize, Allocator.Persistent);
            _keys          = new List<Vector3Int>(_tiles.length);
            _values        = new List<TileBase>(_tiles.length);
            
            _previousTiles.Fill(255);
        }

        
        public override void OnRender(WorldData worldData)
        {
            new RenderFilterJob
            {
                worldData = worldData,
                results = _tiles,
                cameraPosition = int2.zero,
                cameraMax = _cameraSize * Chunk.SIZE
            }.Schedule(worldData.chunks.length, 1).Complete();

            _keys.Clear();
            _values.Clear();
            
            Profiler.BeginSample("Tile filtering");
            for (int i = 0; i < _tiles.length; i++) {
                byte tileType = _tiles[i];
                if (tileType == _previousTiles[i]) continue;

                int2 index = _tiles.GetIndex(i);
                _previousTiles[i] = tileType;
                _keys.Add(new Vector3Int(index.x, index.y));
                _values.Add(_tilesBases[tileType]);
            }
            Profiler.EndSample();

            if (_keys.Count == 0) return;
            Profiler.BeginSample("Updating");
            _tilemap.SetTiles(_keys.ToArray(), _values.ToArray());
            Profiler.EndSample();
        }


        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }


        private void OnDestroy()
        {
            Dispose();
        }


        private void Dispose()
        {
            if (!_tiles.isCreated) return;
            _previousTiles.Dispose();
            _tiles.Dispose();
            _keys   = null;
            _values = null;
        }
    }
}
