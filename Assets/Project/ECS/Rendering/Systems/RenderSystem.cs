using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CellEngine.World;
using CellEngine.Utilities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Bounds = CellEngine.Utilities.Bounds;



namespace CellEngine.Rendering
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial class RenderSystem : SystemBase
    {
        private Native2dArray<int> _jobResults;
        private Native2dArray<int> _previousResults;

        public List<Vector3Int> _keys;
        public List<TileBase>   _bases;

        private EntityQuery                       _chunkQuery;
        private ComponentLookup<CellPosition>     _tileLookup;
        private ComponentTypeHandle<CellPosition> _tilePositionTypeHandle;
        private ComponentTypeHandle<CellType>     _tileIndexTypeHandle;
        private SharedComponentTypeHandle<Chunk>  _chunkTypeHandle;

        protected override void OnCreate()
        {
            _chunkTypeHandle        = GetSharedComponentTypeHandle<Chunk>();
            _tilePositionTypeHandle = GetComponentTypeHandle<CellPosition>();
            _tileIndexTypeHandle    = GetComponentTypeHandle<CellType>();
            _tileLookup             = GetComponentLookup<CellPosition>(true);

            _chunkQuery = GetEntityQuery(new ComponentType(typeof(Chunk)), new ComponentType(typeof(CellPosition)));

            _keys  = new List<Vector3Int>(65536);
            _bases = new List<TileBase>(65536);
            
            RequireForUpdate<RenderTarget>();
            RequireForUpdate<Chunk>();
            RequireForUpdate<CellPosition>();
        }


        protected override void OnDestroy()
        {
            _jobResults.Dispose();
            _previousResults.Dispose();
            _keys  = null;
            _bases = null;
        }


        protected override void OnUpdate()
        {
            _tileLookup.Update(this);
            _chunkTypeHandle.Update(this);
            _tilePositionTypeHandle.Update(this);
            _tileIndexTypeHandle.Update(this);

            RenderTarget renderTarget = default;
            bool         set          = false;
            Entities.WithAll<RenderTarget>().ForEach((RenderTarget v) =>
            {
                if (set) { throw new Exception($"Multiple entities with component of type {nameof(RenderTarget)} exist!"); }

                set          = true;
                renderTarget = v;
            }).WithoutBurst().Run();
            
            Tilemap      tileMap      = renderTarget.target;
            TileBase[]   tileBases    = renderTarget.availableTiles;
            int2         size         = new int2(tileMap.size.x, tileMap.size.y);
            int2         position     = renderTarget.position;

            if (CommonMath.Compare(_jobResults.size != size)) {
                _jobResults.Dispose();
                _previousResults.Dispose();
                
                _jobResults      = new Native2dArray<int>(size, Allocator.Persistent);
                _previousResults = new Native2dArray<int>(size, Allocator.Persistent);
                
                _previousResults.Fill(-1);
            }
            
            //  Rendering

            Dependency = new RenderJob
            {
                results                = _jobResults,
                chunkTypeHandle        = _chunkTypeHandle,
                tilePositionTypeHandle = _tilePositionTypeHandle,
                tileIndexTypeHandle    = _tileIndexTypeHandle,
                cameraPosition         = position,
                cameraSize             = size
            }.Schedule(_chunkQuery, Dependency);
            Dependency.Complete();

            _keys.Clear();
            _bases.Clear();
            for (int i = _jobResults.length; i --> 0;) {
                if (_jobResults[i] == _previousResults[i]) continue;
               
                _previousResults[i] = _jobResults[i];
                int2 index = _jobResults.GetIndex(i);
                _keys.Add(new Vector3Int(index.x, index.y));
                _bases.Add(tileBases[_jobResults[i]]);
            }
            
            //  Applying

            if (_keys.Count == 0) return;
            
            tileMap.SetTiles(_keys.ToArray(), _bases.ToArray());
        }
    }



    [BurstCompile]
    public struct RenderJob : IJobChunk
    {
        public Native2dArray<int> results;

        public SharedComponentTypeHandle<Chunk>  chunkTypeHandle;
        public ComponentTypeHandle<CellPosition> tilePositionTypeHandle;
        public ComponentTypeHandle<CellType>     tileIndexTypeHandle;

        public int2 cameraPosition;
        public int2 cameraSize;


        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Chunk chunkData = chunk.GetSharedComponent(chunkTypeHandle);

            if (!chunkData.Contains(cameraPosition, cameraSize)) return;

            NativeArray<CellPosition> cellPoses = chunk.GetNativeArray(ref tilePositionTypeHandle);
            NativeArray<CellType>     cellTypes = chunk.GetNativeArray(ref tileIndexTypeHandle);

            int2 chunkPos  = chunkData.worldPosition;
            int2 cameraMax = cameraPosition + cameraSize;
            
            for (int i = cellPoses.Length; i --> 0;) {
                int2 worldPos = chunkPos + cellPoses[i].value;
                if (!Bounds.IsInside(worldPos, cameraPosition, cameraMax)) return;

                results[worldPos - cameraPosition] = cellTypes[i].value;
            }
        }
    }
}