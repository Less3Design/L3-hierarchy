using Less3.Heirachy;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.Heirarchy.Editor
{
    /// <summary>
    /// Node as it exists as a visual element
    /// </summary>
    public class L3HeirarchyNodeElement
    {
        public L3HeirarchyNode node;

        public VisualElement root;
        public VisualElement icon;
        public Label title;
        public Label subTitle;

        public L3HeirarchyNodeElement(VisualElement root, L3HeirarchyNode node)
        {
            this.root = root;
            this.node = node;

            icon = root.Q<VisualElement>("Icon");
            title = root.Q<Label>("Title");
            subTitle = root.Q<Label>("SubTitle");
            UpdateContent();
        }

        public void UpdateContent()
        {
            // * Title
            if (node is IHeirarchyNodeTitle titleNode)
            {
                title.text = titleNode.NodeTitle;
            }
            else
            {
                title.text = node.name;
            }

            // * Subtitle
            if (node is IHeirarchyNodeSubTitle subTitleNode)
            {
                if (string.IsNullOrEmpty(subTitleNode.NodeSubTitle))
                {
                    subTitle.style.display = DisplayStyle.None;
                }
                else
                {
                    subTitle.style.display = DisplayStyle.Flex;
                    subTitle.text = subTitleNode.NodeSubTitle;
                }
            }
            else
            {
                subTitle.style.display = DisplayStyle.None;
            }

            // * Icon
            if (node is IHeirarchyNodeIcon iconNode && iconNode.NodeIcon != null)
            {
                icon.style.backgroundImage = new StyleBackground(iconNode.NodeIcon);
                icon.style.display = DisplayStyle.Flex;
            }
            else
            {
                icon.style.backgroundImage = new StyleBackground();
                icon.style.display = DisplayStyle.None;
            }

            if (node is IHeirarchyNodeOpacity opacityNode)
            {
                root.style.opacity = opacityNode.NodeOpacity;
            }
            else
            {
                root.style.opacity = 1f;
            }
        }
    }
}
