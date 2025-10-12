using Less3.Heirarchy;
using Less3.TypeTree;
using UnityEngine;

namespace Less3.Heirachy
{
    [TypeTreeMenu(typeof(ExHeiarchy), "Example/ExNode")]
    public class ExNode : L3HeirarchyNode, IHeirarchyNodeSubTitle, IHeirarchyNodeIcon, IHeirarchyNodeTitle, IHeirarchyNodeOpacity
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
