using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;

namespace Less3.Hierarchy.Editor
{
    [CustomEditor(typeof(L3Hierarchy), true)]
    public class L3HierarchyInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // uhhh... i feel like this isnt right? Im forgetting how to do this properly....
            var so = new SerializedObject(target);
            var property = so.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                var field = new PropertyField(property);
                field.Bind(so);
                root.Add(field);
            }

            var spacer = new VisualElement();
            spacer.style.height = 10;
            root.Add(spacer);

            var button = new Button(() =>
            {
                //open the asset
                AssetDatabase.OpenAsset(target);
            });
            button.text = "Open Hierarchy Editor";
            button.style.height = 32;
            root.Add(button);

            return root;
        }

        // imgui
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Open Hierarchy Editor"))
            {
                AssetDatabase.OpenAsset(target);
            }
        }
    }
}
