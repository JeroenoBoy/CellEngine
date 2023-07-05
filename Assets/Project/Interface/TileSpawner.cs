using System;
using CellEngine.World;
using Unity.Mathematics;
using UnityEngine;



namespace CellEngine.Interface
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private World.CellEngine _cellEngine;
        [SerializeField] private int2             _brushSize;

        [HideInInspector] public CellTemplate activeTemplate;

        private Camera _camera;


        private void Start()
        {
            _camera = Camera.main;
        }


        private void Update()
        {
            if (!Input.GetMouseButton(0)) return;
            float3 enginePos   = _cellEngine.transform.position;
            float2 engineScale = _cellEngine.renderer.scale;

            float2 uvPos       = (Vector2)_camera.ScreenToViewportPoint(Input.mousePosition);
            float2 enginePosUv = (Vector2)_camera.WorldToViewportPoint(enginePos);
            float2 engineMaxUv = (Vector2)_camera.WorldToViewportPoint(enginePos + math.float3(_cellEngine.size * engineScale, 0));

            int2 pos = (int2)((uvPos - enginePosUv) / (engineMaxUv - enginePosUv) * _cellEngine.size);
            if (!Utilities.Bounds.IsInside(pos, int2.zero, _cellEngine.size)) return;
            
            _cellEngine.Fill(activeTemplate, pos - _brushSize / 2, _brushSize);
        }
    }
}
