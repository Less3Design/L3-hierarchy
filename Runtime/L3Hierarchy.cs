using System;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.Hierarchy
{
    public abstract class L3Hierarchy : ScriptableObject
    {
        // hide the variable not used warning
#pragma warning disable 0414
        [SerializeField, HideInInspector]
        private int serializedVersion = 1;// For future use (maybe)
#pragma warning restore 0414

        [HideInInspector]
        public List<L3HierarchyNode> nodes = new List<L3HierarchyNode>();
        public Action OnTreeRefreshRequired_EDITOR;

        private void OnValidate()
        {
            // remove any subassets that are node in nodes.
            // ? If you undo a deletion. Then redo it (in the undo window) the subasset never gets re-removed.
            //   This seems like easist fix without adding complex undo tracking.
#if UNITY_EDITOR
            var allSubAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
            foreach (var subAsset in allSubAssets)
            {
                if (subAsset is L3HierarchyNode node)
                {
                    if (!nodes.Contains(node))
                    {
                        UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                        UnityEditor.EditorUtility.SetDirty(this);
                        UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
                    }
                }
            }
#endif
        }

        protected virtual bool CustomParentActionValidation(L3HierarchyNode node, L3HierarchyNode newParent)
        {
            return true;
        }
        /// <summary>
        /// returns true if the node can be parented to the new parent.
        /// </summary>
        public bool ValidateParentAction(L3HierarchyNode node, L3HierarchyNode newParent)
        {
            if (node == null)
                return false;

            if (node == newParent)
                return false;

            if (newParent != null && node.Hierarchy != newParent.Hierarchy)
                return false;

            // you try to parent an object to one of its children
            if (NodeIsChildOfParent(newParent, node))
                return false;

            return CustomParentActionValidation(node, newParent);
        }

        public bool NodeIsChildOfParent(L3HierarchyNode node, L3HierarchyNode parent)
        {
            if (node == null || parent == null)
                return false;

            L3HierarchyNode current = node.parent;
            while (current != null)
            {
                if (current == parent)
                    return true;

                current = current.parent;
            }

            return false;
        }

        public List<L3HierarchyNode> GetRootNodes()
        {
            List<L3HierarchyNode> rootNodes = new List<L3HierarchyNode>();
            foreach (var node in nodes)
            {
                if (node.parent == null)
                    rootNodes.Add(node);
            }
            return rootNodes;
        }

        public List<T> GetRootNodesOfType<T>() where T : L3HierarchyNode
        {
            List<T> rootNodes = new List<T>();
            foreach (var node in nodes)
            {
                if (node.parent == null && node is T tNode)
                    rootNodes.Add(tNode);
            }
            return rootNodes;
        }

        public void UpdateTree_EDITOR()
        {
            OnTreeRefreshRequired_EDITOR?.Invoke();
        }
    }
}