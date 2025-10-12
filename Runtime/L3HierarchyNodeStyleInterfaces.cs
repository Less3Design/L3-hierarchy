using UnityEngine;

namespace Less3.Hierarchy
{
    public interface IHierarchyNodeTitle
    {
        string NodeTitle { get; }
    }
    public interface IHierarchyNodeSubTitle
    {
        string NodeSubTitle { get; }
    }
    public interface IHierarchyNodeIcon
    {
        Texture2D NodeIcon { get; }
    }
    public interface IHierarchyNodeOpacity
    {
        float NodeOpacity { get; }
    }
}
