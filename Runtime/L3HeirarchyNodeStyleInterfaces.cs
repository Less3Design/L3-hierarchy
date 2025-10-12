using UnityEngine;

namespace Less3.Heirarchy
{
    public interface IHeirarchyNodeTitle
    {
        string NodeTitle { get; }
    }
    public interface IHeirarchyNodeSubTitle
    {
        string NodeSubTitle { get; }
    }
    public interface IHeirarchyNodeIcon
    {
        Texture2D NodeIcon { get; }
    }
    public interface IHeirarchyNodeOpacity
    {
        float NodeOpacity { get; }
    }
}
