using UnityEngine;



namespace CellEngine.World.Rendering
{
    public abstract class CellWorldRenderer : MonoBehaviour
    {
        public abstract void OnWorldSetup(WorldData worldData);
        public abstract void OnRender(WorldData worldData);
    }
}
