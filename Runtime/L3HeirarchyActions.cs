using Less3.Heirachy;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Less3.Heirarchy
{
    /// <summary>
    /// Holds all the methods like "add node" and "set parent" for manipulating heirarchies and nodes.
    /// Methods are valid to be used in editor and runtime.
    /// </summary>
    public static class L3HeirarchyActions
    {
        // Actions are undo-able operations if its editor
        public static bool SetParentAction(this L3HeirarchyNode node, L3HeirarchyNode newParent)
        {
            if (node == null || newParent == null || node.heirarchy == null)
                return false;

            // Validate the action
            if (node.heirarchy.ValidateParentAction(node, newParent) == false)
                return false;

            RecordUndoIfAsset(node, "Set Parent");
            if (node.parent != null)
                RecordUndoIfAsset(node.parent, "Set Parent");
            if (newParent != null)
                RecordUndoIfAsset(newParent, "Set Parent");
            return node.SetParent(newParent);
        }

        public static bool SetParentAction(this L3HeirarchyNode node, L3HeirarchyNode newParent, int index)
        {
            if (node == null || newParent == null || node.heirarchy == null)
                return false;

            // Validate the action
            if (node.heirarchy.ValidateParentAction(node, newParent) == false)
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

        public static void SetIndexAction(this L3HeirarchyNode node, int newIndex)
        {
            if (node.parent != null)
            {
                RecordUndoIfAsset(node.parent, "Set Index");
            }

            node.SetIndex(newIndex);
        }

        public static bool ReleaseParentAction(this L3HeirarchyNode node, int siblingIndex)
        {
            if (node.parent != null)
            {
                RecordUndoIfAsset(node, "Release Parent");
                RecordUndoIfAsset(node.parent, "Release Parent");
            }

            node.RemoveParent();
            return true;
        }

        public static T CreateNode<T>(this L3Heirarchy heirarchy, L3HeirarchyNode parent = null) where T : L3HeirarchyNode
        {
            if (heirarchy == null)
                return null;

            var newNode = ScriptableObject.CreateInstance<T>();
            newNode.InitNode(heirarchy);
            newNode.name = System.Guid.NewGuid().ToString();

            RecordUndoIfAsset(heirarchy, "Create Node");
            heirarchy.nodes.Add(newNode);
            AddAsSubObjectIfAsset(newNode, heirarchy);

            if (parent != null)
            {
                RecordUndoIfAsset(parent, "Create Node");
                newNode.SetParent(parent);
            }

            return newNode;
        }

        public static L3HeirarchyNode CreateNode(this L3Heirarchy heirarchy, System.Type nodeType, L3HeirarchyNode parent = null)
        {
            if (heirarchy == null || nodeType == null)
                return null;

            if (!typeof(L3HeirarchyNode).IsAssignableFrom(nodeType))
            {
                Debug.LogError("Type " + nodeType.Name + " is not a L3HeirarchyNode.");
                return null;
            }

            var newNode = ScriptableObject.CreateInstance(nodeType) as L3HeirarchyNode;
            if (newNode == null)
            {
                Debug.LogError("Failed to create instance of type " + nodeType.Name);
                return null;
            }

            newNode.InitNode(heirarchy);
            newNode.name = System.Guid.NewGuid().ToString();// !

            RecordUndoIfAsset(heirarchy, "Create Node");
            heirarchy.nodes.Add(newNode);
            AddAsSubObjectIfAsset(newNode, heirarchy);

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
        public static void DeleteNode(this L3Heirarchy heirarchy, L3HeirarchyNode node)
        {
            RecordUndoIfAsset(heirarchy, "Delete Node");
            DeleteNodeRecursive(heirarchy, node);
            SaveObjectIfAsset(heirarchy);
        }

        private static void DeleteNodeRecursive(this L3Heirarchy heirarchy, L3HeirarchyNode node)
        {
            if (heirarchy == null || node == null)
                return;

            if (heirarchy.nodes.Contains(node) == false)
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
                heirarchy.DeleteNodeRecursive(child);
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

            heirarchy.nodes.Remove(node);
        }

        // * ----------------------------------------------------------------------------------------

        // Components are the bits that make up the actions
        private static bool SetParent(this L3HeirarchyNode node, L3HeirarchyNode newParent)
        {
            if (!node.heirarchy.ValidateParentAction(node, newParent))
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

        private static void RemoveParent(this L3HeirarchyNode node)
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

        private static void SetIndex(this L3HeirarchyNode node, int newIndex)
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
