using UnityEditor;
using UnityEngine.UIElements;



namespace World.Editor
{
    [CustomPropertyDrawer(typeof(Cell))]
    public class CellEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }
    }
}
