using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using EntityWorld = Unity.Entities.World;



namespace CellEngine.Rendering
{
    [RequireComponent(typeof(Tilemap))]
    public class CellRenderer : MonoBehaviour
    {
        // [SerializeField, Required] private Camera cameraTarget;

        [SerializeField] private TileBase[] tiles;
        [SerializeField] private string     worldName     = "Default World";
        [SerializeField] private int        pixelsPerUnit = 64;
        [SerializeField] private Vector2Int size;
        // [SerializeField] private float  zoom = 1;

        private Tilemap      _tilemap;
        private RenderTarget _renderTarget;
        
        
        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }


        private void Start()
        {
            _renderTarget = new RenderTarget(_tilemap, tiles, int2.zero);

            EntityWorld ecsWorld = GetWorld();
            Entity entity = ecsWorld.EntityManager.CreateEntity(new ComponentType(typeof(RenderTarget)));
            ecsWorld.EntityManager.AddComponentObject(entity, _renderTarget);

            transform.localScale = Vector3.one * 1 / pixelsPerUnit;
            _tilemap.size        = (Vector3Int)(size * pixelsPerUnit);
            _tilemap.ResizeBounds();
        }


        private EntityWorld GetWorld()
        {
            if (string.IsNullOrEmpty(worldName)) {
                return EntityWorld.DefaultGameObjectInjectionWorld;
            }

            for (int i = EntityWorld.All.Count; i-- > 0;) {
                if (EntityWorld.All[i].Name == worldName) return EntityWorld.All[i];
            }

            return null;
        }
    }
}
