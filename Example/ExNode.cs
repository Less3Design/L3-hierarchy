#if LESS3_EXAMPLES
using Less3.Hierarchy;
using Less3.TypeTree;
using UnityEngine;

namespace Less3.Hierarchy
{
    [TypeTreeMenu(typeof(ExHierarchy), "Example/ExNode")]
    public class ExNode : L3HierarchyNode, IHierarchyNodeSubTitle, IHierarchyNodeIcon, IHierarchyNodeTitle, IHierarchyNodeOpacity
    {
        public string title;
        public string NodeTitle => string.IsNullOrEmpty(title) ? name : title;
        public string subTitle;
        public string NodeSubTitle => subTitle;
        public Texture2D icon;
        public Texture2D NodeIcon => icon;
        public bool disable;
        public float NodeOpacity => disable ? 0.5f : 1f;
    }
}
#endif
