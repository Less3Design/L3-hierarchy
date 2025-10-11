using System.Collections.Generic;
using UnityEngine;

namespace Less3.Heirachy
{
    public abstract class L3HeirarchyNode : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private int serializedVersion = 1;// For future use (maybe)

        // Nodes are arranged based on each nodes internal parent / child references.
        // Modeled after GameObject transforms.
        [SerializeField, HideInInspector]
        public L3HeirarchyNode parent;
        // Order of this list is the order of the children, top / first is index 0
        [SerializeField, HideInInspector]
        public List<L3HeirarchyNode> children = new List<L3HeirarchyNode>();

        [SerializeField, HideInInspector]
        private L3Heirarchy _heirarchy;
        public L3Heirarchy heirarchy => _heirarchy;

        public int Index
        {
            get
            {
                // TODO actually we should return the order of this node in the heirarchy.
                // like compare against all parentless nodes.
                if (parent == null || parent.children == null)
                    return 0;

                return parent.children.IndexOf(this);
            }
        }

        public void InitNode(L3Heirarchy heirarchy)
        {
            if (_heirarchy != null)
            {
                Debug.LogWarning("An L3HeirarchyNode can only be initialized once.");
                return;
            }

            this._heirarchy = heirarchy;
        }
    }
}
