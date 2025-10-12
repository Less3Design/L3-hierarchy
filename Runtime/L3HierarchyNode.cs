using System.Collections.Generic;
using UnityEngine;

namespace Less3.Hierarchy
{
    public abstract class L3HierarchyNode : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private int serializedVersion = 1;// For future use (maybe)

        // Nodes are arranged based on each nodes internal parent / child references.
        // Modeled after GameObject transforms.
        [SerializeField, HideInInspector]
        public L3HierarchyNode parent;
        // Order of this list is the order of the children, top / first is index 0
        [SerializeField, HideInInspector]
        public List<L3HierarchyNode> children = new List<L3HierarchyNode>();

        [SerializeField, HideInInspector]
        private L3Hierarchy _Hierarchy;
        public L3Hierarchy Hierarchy => _Hierarchy;

        public int Index
        {
            get
            {
                // TODO actually we should return the order of this node in the Hierarchy.
                // like compare against all parentless nodes.
                if (parent == null || parent.children == null)
                    return 0;

                return parent.children.IndexOf(this);
            }
        }

        public L3HierarchyNode GetRoot()
        {
            L3HierarchyNode current = this;
            while (current.parent != null)
            {
                current = current.parent;
            }
            return current;
        }

        public void InitNode(L3Hierarchy Hierarchy)
        {
            if (_Hierarchy != null)
            {
                Debug.LogWarning("An L3HierarchyNode can only be initialized once.");
                return;
            }

            this._Hierarchy = Hierarchy;
        }
    }
}
