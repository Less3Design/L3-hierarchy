using Less3.Hierarchy;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Less3.Hierarchy
{
    /// <summary>
    /// Holds all the methods like "add node" and "set parent" for manipulating heirarchies and nodes.
    /// Methods are valid to be used in editor and runtime.
    /// </summary>
    public static class L3HierarchyActions
    {
        // Actions are undo-able operations if its editor
        public static bool SetParentAction(this L3HierarchyNode node, L3HierarchyNode newParent)
        {
            if (node == null || newParent == null || node.Hierarchy == null)
                return false;

            // Validate the action
            if (node.Hierarchy.ValidateParentAction(node, newParent) == false)
                return false;

            RecordUndoIfAsset(node, "Set Parent");
            if (node.parent != null)
                RecordUndoIfAsset(node.parent, "Set Parent");
            if (newParent != null)
                RecordUndoIfAsset(newParent, "Set Parent");
            return node.SetParent(newParent);
        }

        public static bool SetParentAction(this L3HierarchyNode node, L3HierarchyNode newParent, int index)
        {
            if (node == null || newParent == null || node.Hierarchy == null)
                return false;

            // Validate the action
            if (node.Hierarchy.ValidateParentAction(node, newParent) == false)
                return false;

            RecordUndoIfAsset(node, "Set Parent");
            if (node.parent != null)
                RecordUndoIfAsset(node.parent, "Set Parent");
            if (newParent != null)
                RecordUndoIfAsset(newParent, "Set Parent");

            node.SetParent(newParent);
            node.SetIndex(index);
            return true;//uhh..
        }

        public static void SetIndexAction(this L3HierarchyNode node, int newIndex)
        {
            if (node.parent != null)
            {
                RecordUndoIfAsset(node.parent, "Set Index");
            }

            node.SetIndex(newIndex);
        }

        public static bool ReleaseParentAction(this L3HierarchyNode node, int siblingIndex)
        {
            if (node.parent != null)
            {
                RecordUndoIfAsset(node, "Release Parent");
                RecordUndoIfAsset(node.parent, "Release Parent");
            }

            node.RemoveParent();
            return true;
        }

        public static T CreateNode<T>(this L3Hierarchy Hierarchy, L3HierarchyNode parent = null) where T : L3HierarchyNode
        {
            if (Hierarchy == null)
                return null;

            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.InitNode(Hierarchy);
            newNode.name = typeof(T).Name;

            RecordUndoIfAsset(Hierarchy, "Create Node");
            Hierarchy.nodes.Add(newNode);
            AddAsSubObjectIfAsset(newNode, Hierarchy);

            if (parent != null)
            {
                RecordUndoIfAsset(parent, "Create Node");
                newNode.SetParent(parent);
            }

            return newNode;
        }

        public static L3HierarchyNode CreateNode(this L3Hierarchy Hierarchy, System.Type nodeType, L3HierarchyNode parent = null)
        {
            if (Hierarchy == null || nodeType == null)
                return null;

            if (!typeof(L3HierarchyNode).IsAssignableFrom(nodeType))
            {
                Debug.LogError("Type " + nodeType.Name + " is not a L3HierarchyNode.");
                return null;
            }

            var newNode = ScriptableObject.CreateInstance(nodeType) as L3HierarchyNode;
            if (newNode == null)
            {
                Debug.LogError("Failed to create instance of type " + nodeType.Name);
                return null;
            }

            newNode.InitNode(Hierarchy);
            newNode.name = nodeType.Name;

            RecordUndoIfAsset(Hierarchy, "Create Node");
            Hierarchy.nodes.Add(newNode);
            AddAsSubObjectIfAsset(newNode, Hierarchy);

            if (parent != null)
            {
                RecordUndoIfAsset(parent, "Create Node");
                newNode.SetParent(parent);
            }

            return newNode;
        }


        /// <summary>
        /// Delete the node, and all its children. Very destructive! Make sure to warn the user before calling this.
        /// </summary>
        public static void DeleteNode(this L3Hierarchy Hierarchy, L3HierarchyNode node)
        {
            RecordUndoIfAsset(Hierarchy, "Delete Node");
            DeleteNodeRecursive(Hierarchy, node);
            SaveObjectIfAsset(Hierarchy);
        }

        private static void DeleteNodeRecursive(this L3Hierarchy Hierarchy, L3HierarchyNode node)
        {
            if (Hierarchy == null || node == null)
                return;

            if (Hierarchy.nodes.Contains(node) == false)
            {
                return;
            }

            if (node.parent != null)
            {
                RecordUndoIfAsset(node.parent, "Delete Node");
            }

            // Remove from parent
            node.RemoveParent();

            // Remove all children
            foreach (var child in node.children.ToArray())
            {
                Hierarchy.DeleteNodeRecursive(child);
            }

#if UNITY_EDITOR
            // Destroy the object
            if (AssetDatabase.Contains(node))
            {
                Undo.DestroyObjectImmediate(node);
            }
            else
            {
                ScriptableObject.DestroyImmediate(node);
            }
#else
            ScriptableObject.Destroy(node);
#endif

            Hierarchy.nodes.Remove(node);
        }

        // * ----------------------------------------------------------------------------------------

        // Components are the bits that make up the actions
        private static bool SetParent(this L3HierarchyNode node, L3HierarchyNode newParent)
        {
            if (!node.Hierarchy.ValidateParentAction(node, newParent))
                return false;

            // Remove from old parent
            if (node.parent != null)
            {
                node.RemoveParent();
            }

            // Set new parent
            node.parent = newParent;
            newParent.children.Add(node);

            SetDirtyIfAsset(node);
            SetDirtyIfAsset(newParent);
            return true;
        }

        private static void RemoveParent(this L3HierarchyNode node)
        {
            if (node == null)
                return;

            if (node.parent == null)
                return;

            var parent = node.parent;
            node.parent.children.Remove(node);
            node.parent = null;
            SetDirtyIfAsset(node);
            SetDirtyIfAsset(parent);
        }

        private static void SetIndex(this L3HierarchyNode node, int newIndex)
        {
            if (node.parent == null)
                return;

            var siblings = node.parent.children;
            var currentIndex = siblings.IndexOf(node);
            if (currentIndex == -1)
                return;

            if (newIndex < 0 || newIndex >= siblings.Count)
                return;

            if (newIndex == currentIndex)
                return;

            siblings.RemoveAt(currentIndex);
            siblings.Insert(newIndex, node);
            SetDirtyIfAsset(node.parent);
        }



        // * ----------------------------------------------------------------------------------------

        // Helpers to make this all work nice in editor or runtime
        private static void RecordUndoIfAsset(Object obj, string actionName)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(obj))
            {
                Undo.RecordObject(obj, actionName);
            }
#endif
        }

        private static void SetDirtyIfAsset(Object obj)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(obj))
            {
                EditorUtility.SetDirty(obj);
            }
#endif
        }

        private static void AddAsSubObjectIfAsset(Object node, Object hierarchy)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(hierarchy))
            {
                AssetDatabase.AddObjectToAsset(node, hierarchy);
                SetDirtyIfAsset(hierarchy);
                AssetDatabase.SaveAssetIfDirty(hierarchy);
            }
#endif
        }

        private static void SaveObjectIfAsset(Object obj)
        {
#if UNITY_EDITOR
            if (AssetDatabase.Contains(obj))
            {
                AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}
