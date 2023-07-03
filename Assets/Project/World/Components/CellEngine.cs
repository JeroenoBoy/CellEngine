using System;
using System.Linq;
using CellEngine.Utilities;
using JUtils.Attributes;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using World.Components.Rendering;
using World.Jobs;



namespace World.Components
{
    public class CellEngine : MonoBehaviour
    {
        [SerializeField, Required] private CellWorldRenderer _renderer;
        
        [Space]
        [SerializeField] private int2   _chunks;
        [SerializeField] private CellTemplate[] _cellTemplates;

        [Header("Fill")]
        [SerializeField] private bool     _doInitialFill;
        [SerializeField] private byte _cellType;
        [SerializeField] private int2 _position;
        [SerializeField] private int2 _size;

        private WorldData _worldData;


        public void Fill(byte cellType, int2 position, int2 size)
        {
            _worldData.Fill(_cellTemplates.First(x => x.cellType == cellType), position, size);
        }
        

        private void Start()
        {
            _worldData = new WorldData(_chunks, Allocator.Persistent);
            _worldData.Fill(_cellTemplates[0]);
            _renderer.OnWorldSetup(_worldData);

            if (_doInitialFill) Fill();
        }


        private void OnDestroy()
        {
            if (_worldData.isCreated) {
                _worldData.Dispose();
            }
        }


        private void FixedUpdate()
        {
            Profiler.BeginSample("Update loop");
            JobScheduler();
            Profiler.EndSample();
            
            Profiler.BeginSample("Rendering");
            _renderer.OnRender(_worldData);
            Profiler.EndSample();
        }


        [Button()]
        private void Fill()
        {
            Fill(_cellType, _position, _size);
        }


        private void JobScheduler()
        {
            SimulationJob job = new SimulationJob {worldData = _worldData, seed = (uint)(Time.realtimeSinceStartup * Time.deltaTime * 741246)};
            
            int len = _worldData.length;
            Schedule(int2.zero); 
            Schedule(new int2(0, 1));
            Schedule(new int2(1, 0));
            Schedule(new int2(1, 1));

            void Schedule(int2 offset)
            {
                job.offset = offset;
                job.Schedule((int)math.ceil(len / 4f), 1).Complete();
            }
        }
    }
}