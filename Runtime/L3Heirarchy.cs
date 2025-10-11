using System.Collections.Generic;
using UnityEngine;

namespace Less3.Heirachy
{
    public abstract class L3Heirarchy : ScriptableObject
    {
        public List<L3HeirarchyNode> nodes = new List<L3HeirarchyNode>();
        public Action OnTreeRefreshRequired_EDITOR;

        protected virtual bool CustomParentActionValidation(L3HeirarchyNode node, L3HeirarchyNode newParent)
        {
            return true;
        }
        public bool ValidateParentAction(L3HeirarchyNode node, L3HeirarchyNode newParent)
        {
            if (node == null || newParent == null)
                return false;

            if (node == newParent)
                return false;

            if (node.heirarchy != newParent.heirarchy)
                return false;

            return CustomParentActionValidation(node, newParent);
        }

        public void UpdateTree_EDITOR()
        {
            OnTreeRefreshRequired_EDITOR?.Invoke();
        }
    }
}