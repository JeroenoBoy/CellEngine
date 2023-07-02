using JUtils.Attributes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using World.Rendering;



namespace World.Components
{
    public class CellEngine : MonoBehaviour
    {
        [SerializeField, Required] public CellWorldRenderer _renderer;
        
        [Space]
        [SerializeField] private int2   _chunks;
        [SerializeField] private Cell[] _cellTemplates;

        private CellWorld _cellWorld;


        private void Start()
        {
            _cellWorld = new CellWorld(_chunks, Allocator.Temp);
            _cellWorld.Fill(_cellTemplates[0]);
            _renderer.OnWorldSetup(_cellWorld);
        }


        private void Update()
        {
        }
    }
}
