using Less3.Hierarchy;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.Hierarchy.Editor
{
    /// <summary>
    /// Node as it exists as a visual element
    /// </summary>
    public class L3HierarchyNodeElement
    {
        public L3HierarchyNode node;

        public VisualElement root;
        public VisualElement icon;
        public Label title;
        public Label subTitle;

        public L3HierarchyNodeElement(VisualElement root, L3HierarchyNode node)
        {
            this.root = root;
            this.node = node;

            icon = root.Q<VisualElement>("Icon");
            title = root.Q<Label>("Title");
            subTitle = root.Q<Label>("SubTitle");
            root.parent.parent.AddToClassList("ElementBackground");
            UpdateContent();
        }

        public void UpdateContent()
        {
            // * Title
            if (node is IHierarchyNodeTitle titleNode)
            {
                title.text = titleNode.NodeTitle;
            }
            else
            {
                title.text = node.name;
            }

            // * Subtitle
            if (node is IHierarchyNodeSubTitle subTitleNode)
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
            if (node is IHierarchyNodeIcon iconNode && iconNode.NodeIcon != null)
            {
                icon.style.backgroundImage = new StyleBackground(iconNode.NodeIcon);
                icon.style.display = DisplayStyle.Flex;
            }
            else
            {
                icon.style.backgroundImage = new StyleBackground();
                icon.style.display = DisplayStyle.None;
            }

            if (node is IHierarchyNodeOpacity opacityNode)
            {
                root.style.opacity = opacityNode.NodeOpacity;
            }
            else
            {
                root.style.opacity = 1f;
            }
            
            /*
            if (node is IHierarchyAlternateBackground alternateBackgroundNode)
            {
                if (alternateBackgroundNode.UseAlternateBackground)
                {
                    root.parent.parent.AddToClassList("ElementAlternateBackground");
                }
                else
                {
                    root.parent.parent.RemoveFromClassList("ElementAlternateBackground");
                }
            }
            else
            {
                root.parent.parent.RemoveFromClassList("ElementAlternateBackground");
            }
            */
        }
    }
}
