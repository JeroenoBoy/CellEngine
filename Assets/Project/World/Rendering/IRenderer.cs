using UnityEngine;



namespace World.Rendering
{
    public abstract class CellWorldRenderer : MonoBehaviour
    {
        public abstract void OnWorldSetup(CellWorld world);
        public abstract void OnRender(CellWorld world);
    }
}
