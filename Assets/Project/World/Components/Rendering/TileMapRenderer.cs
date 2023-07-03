﻿using System.Collections.Generic;
using CellEngine.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;
using World.Jobs;



namespace World.Components.Rendering
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapCellRenderer : CellWorldRenderer
    {
        [SerializeField] private int2       _cameraSize;
        [SerializeField] private TileBase[] _tilesBases;

        private Tilemap            _tilemap;
        private Native2dArray<int> _tiles;
        private Native2dArray<int> _previousTiles;

        private List<Vector3Int> _keys;
        private List<TileBase>   _values;


        public override void OnWorldSetup(WorldData worldData)
        {
            Dispose();
            
            transform.localScale = Vector3.one * Chunk.INVERSE_SIZE;
            _tilemap.size        = new Vector3Int(_cameraSize.x * Chunk.SIZE, _cameraSize.y * Chunk.SIZE);

            _tiles         = new Native2dArray<int>(worldData.worldSize, Allocator.Persistent);
            _previousTiles = new Native2dArray<int>(worldData.worldSize, Allocator.Persistent);
            _keys          = new List<Vector3Int>(_tiles.length);
            _values        = new List<TileBase>(_tiles.length);
            
            _previousTiles.Fill(-1);
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
                int tileIndex = _tiles[i];
                if (tileIndex == _previousTiles[i]) continue;

                int2 index = _tiles.GetIndex(i);
                _previousTiles[i] = tileIndex;
                _keys.Add(new Vector3Int(index.x, index.y));
                _values.Add(_tilesBases[tileIndex]);
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
