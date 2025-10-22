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

        public L3HierarchyNode GetChildAtIndex(int index)
        {
            if (index < 0 || index >= children.Count)
                return null;

            return children[index];
        }

        public int GetChildIndex(L3HierarchyNode child)
        {
            if (child == null)
                return -1;

            return children.IndexOf(child);
        }

        /// <summary>
        /// Returns a copy of the children list.
        /// </summary>
        public List<L3HierarchyNode> GetChildren()
        {
            return new List<L3HierarchyNode>(children);
        }

        public List<T> GetChildrenOfType<T>() where T : L3HierarchyNode
        {
            List<T> typedChildren = new List<T>();
            foreach (var child in children)
            {
                if (child is T typedChild)
                {
                    typedChildren.Add(typedChild);
                }
            }
            return typedChildren;
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
