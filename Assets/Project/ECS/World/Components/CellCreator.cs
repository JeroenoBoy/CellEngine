using System;
using System.Collections;
using CellEngine.Utilities;
using JUtils.Attributes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using EntityWorld = Unity.Entities.World;



namespace CellEngine.World
{
    public class CellCreator : MonoBehaviour
    {
        [Header("Late Start Settings")]
        [SerializeField] private bool       _enabled;
        [SerializeField] private int        _cellType;
        [SerializeField] private Vector2Int _position;
        [SerializeField] private Vector2Int _size;


        private IEnumerator Start()
        {
            if (!_enabled) yield break;
            yield return null;
            yield return null;
            yield return null;
            
            CreateTile(_cellType, _position, _size);
        }


        [Button]
        private void CreateTile(int cellType, Vector2Int position, Vector2Int size)
        {
            EntityWorld world = EntityWorld.DefaultGameObjectInjectionWorld;

            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Persistent);

            EntityQuery cellQuery  = builder.WithAll<CellProvider>().Build(world.EntityManager);
            EntityQuery worldQuery = WorldSingleton.GetQuery(world.EntityManager);
            
            WorldSingleton singleton    = WorldSingleton.Get(worldQuery);
            DynamicBuffer<CellProvider>   cellProviders = cellQuery.GetSingletonBuffer<CellProvider>();

            try {
                CellProvider cellProvider = cellProviders.Find(x => x.cellType == cellType);
                int2 pos = new int2(position.x, position.y);
                
                for (int x = math.max(size.x, 1); x --> 0;)
                for (int y = math.max(size.y, 1); y --> 0;) {
                    int2 spawnPos = pos + new int2(x, y);
                    singleton.SpawnTile(spawnPos, cellProvider.prefab);
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
            
            cellQuery.Dispose();
            worldQuery.Dispose();
            builder.Dispose();
        }
    }
}
