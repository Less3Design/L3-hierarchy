using System;
using System.Collections.Generic;
using UnityEngine;

namespace Less3.Heirachy
{
    public abstract class L3Heirarchy : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private int serializedVersion = 1;// For future use (maybe)

        public List<L3HeirarchyNode> nodes = new List<L3HeirarchyNode>();
        public Action OnTreeRefreshRequired_EDITOR;

        protected virtual bool CustomParentActionValidation(L3HeirarchyNode node, L3HeirarchyNode newParent)
        {
            return true;
        }
        /// <summary>
        /// returns true if the node can be parented to the new parent.
        /// </summary>
        public bool ValidateParentAction(L3HeirarchyNode node, L3HeirarchyNode newParent)
        {
            if (node == null)
                return false;

            if (node == newParent)
                return false;

            if (newParent != null && node.heirarchy != newParent.heirarchy)
                return false;

            // you try to parent an object to one of its children
            if (NodeIsChildOfParent(newParent, node))
                return false;

            return CustomParentActionValidation(node, newParent);
        }

        public bool NodeIsChildOfParent(L3HeirarchyNode node, L3HeirarchyNode parent)
        {
            if (node == null || parent == null)
                return false;

            L3HeirarchyNode current = node.parent;
            while (current != null)
            {
                if (current == parent)
                    return true;

                current = current.parent;
            }

            return false;
        }

        public void UpdateTree_EDITOR()
        {
            OnTreeRefreshRequired_EDITOR?.Invoke();
        }
    }
}