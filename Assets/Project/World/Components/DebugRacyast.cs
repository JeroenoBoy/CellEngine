using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;



namespace CellEngine.World
{
    public class DebugRaycast : MonoBehaviour
    {
        [SerializeField] private int2       _from, _to;
        [SerializeField] private CellEngine _cellEngine;


        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            _cellEngine.worldData.CellCast(_from, _to, out CellCastHit hit);

            Vector3 size   = new Vector3(_cellEngine.renderer.scale.x, _cellEngine.renderer.scale.y, 0);
            Vector3 center = size / 2f;
            Gizmos.DrawCube(Convert(hit.worldPosition) + center, size);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(Convert(_from), Convert(_to));

            Vector3 Convert(int2 pos)
            {
                return new Vector3(pos.x * _cellEngine.renderer.scale.x, pos.y * _cellEngine.renderer.scale.y, 0);
            }
        }
    }
}
