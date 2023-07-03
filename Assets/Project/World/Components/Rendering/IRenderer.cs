using UnityEngine;



namespace World.Components.Rendering
{
    public abstract class CellWorldRenderer : MonoBehaviour
    {
        public abstract void OnWorldSetup(WorldData worldData);
        public abstract void OnRender(WorldData worldData);
    }
}
