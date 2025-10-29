using Less3.Hierarchy;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Less3.Hierarchy
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class L3HierarchyUndoManager
    {
        public static string DUPLICATE_KEY = "Duplicate Node (L3Hierarchy)";

#if UNITY_EDITOR
        static L3HierarchyUndoManager()
        {
            Undo.undoRedoEvent += OnEvent;
        }

        // ? Solving some janky undo/redo stuff here. Im pretty sure i just dont know what im doing.
        //   When we REDO an UNDONE duplication, it seems like unity doesnt support that? We are ressurecting deleted objects..?
        //   Im really not sure...  So the following just clears the REDO stack after undoing a duplication.
        private static void OnEvent(in UndoRedoInfo args)
        {
            if (args.undoName == DUPLICATE_KEY)
            {
                // add to post event
                EditorApplication.delayCall += ClearRedo;
            }
        }

        private static void ClearRedo()
        {
            var temp = ScriptableObject.CreateInstance<ScriptableObject>();
            Undo.RegisterCreatedObjectUndo(temp, "CLEAR REDO STACK (L3Hierarchy)");
            // Destroy it immediately—still clears redo
            Object.DestroyImmediate(temp, true);
        }
#endif
    }

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

        public static bool ReleaseParentAction(this L3HierarchyNode node)
        {
            if (node.parent != null)
            {
                RecordUndoIfAsset(node, "Release Parent");
                RecordUndoIfAsset(node.parent, "Release Parent");
            }

            node.RemoveParent();
            return true;
        }

        public static void ReorderRootAction(this L3Hierarchy hierarchy, L3HierarchyNode nodeToMove, L3HierarchyNode precedingNode)
        {
            if (hierarchy == null || nodeToMove == null)
                return;

            RecordUndoIfAsset(nodeToMove, "Reorder Root Nodes");
            RecordUndoIfAsset(nodeToMove.parent, "Reorder Root Nodes");
            RecordUndoIfAsset(hierarchy, "Reorder Root Nodes");

            nodeToMove.RemoveParent();

            hierarchy.nodes.Remove(nodeToMove);

            int newIndex = 0;
            if (precedingNode != null)
            {
                newIndex = hierarchy.nodes.IndexOf(precedingNode) + 1;
            }

            hierarchy.nodes.Insert(newIndex, nodeToMove);
            SetDirtyIfAsset(hierarchy);
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
        public static void DeleteNodeAction(this L3Hierarchy Hierarchy, L3HierarchyNode node)
        {
            RecordFullUndoIfAsset(Hierarchy, "Delete Node");
            DeleteNodeRecursive(Hierarchy, node);
            SetDirtyIfAsset(Hierarchy);
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
                // Remove from parent
                node.RemoveParent();
                SetDirtyIfAsset(node.parent);
            }

            // Remove all children
            foreach (var child in node.children.ToArray())
            {
                Hierarchy.DeleteNodeRecursive(child);
            }

#if UNITY_EDITOR
            // Destroy the object
            ScriptableObject.DestroyImmediate(node, true);
#else
            ScriptableObject.Destroy(node);
#endif

            Hierarchy.nodes.Remove(node);
        }

        public static L3HierarchyNode DuplicateNodeAction(this L3Hierarchy Hierarchy, L3HierarchyNode node)
        {
            if (Hierarchy == null || node == null)
                return null;

            RecordFullUndoIfAsset(Hierarchy, L3HierarchyUndoManager.DUPLICATE_KEY);

            var newNode = Object.Instantiate(node);

            newNode.name = node.name + " Copy";
            newNode.children = new List<L3HierarchyNode>();
            if (node.parent != null)
            {
                newNode.SetParent(node.parent);
                SetDirtyIfAsset(node.parent);
            }
            Hierarchy.nodes.Add(newNode);
            AddAsSubObjectIfAsset(newNode, Hierarchy);

            // Duplicate children
            foreach (var child in node.children)
            {
                DuplicateNodeRecursive(Hierarchy, child, newNode);
            }

            SetDirtyIfAsset(Hierarchy);
            SaveObjectIfAsset(Hierarchy);
            return newNode;
        }

        private static void DuplicateNodeRecursive(L3Hierarchy Hierarchy, L3HierarchyNode node, L3HierarchyNode newParent)
        {
            if (Hierarchy == null || node == null || newParent == null)
                return;

            var newNode = Object.Instantiate(node);
            newNode.name = node.name + " Copy";
            newNode.children = new List<L3HierarchyNode>();
            Hierarchy.nodes.Add(newNode);
            AddAsSubObjectIfAsset(newNode, Hierarchy);

            // Set parent
            newNode.SetParent(newParent);

            // Duplicate children
            foreach (var child in node.children)
            {
                DuplicateNodeRecursive(Hierarchy, child, newNode);
            }
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
            if (obj == null)
                return;
#if UNITY_EDITOR
            if (AssetDatabase.Contains(obj))
            {
                Undo.RecordObject(obj, actionName);
            }
#endif
        }

        private static void RecordFullUndoIfAsset(L3Hierarchy obj, string actionName)
        {
            if (obj == null)
                return;

#if UNITY_EDITOR
            if (AssetDatabase.Contains(obj))
            {
                var all = obj.nodes.ToList<Object>();
                all.Add(obj);
                Undo.RegisterCompleteObjectUndo(all.ToArray(), actionName);
            }
#endif
        }

        private static void SetDirtyIfAsset(Object obj)
        {
            if (obj == null)
                return;
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
            if (obj == null)
                return;
#if UNITY_EDITOR
            if (AssetDatabase.Contains(obj))
            {
                AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}
