using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Less3.Heirachy;
using System.Collections.Generic;

namespace Less3.Heirarchy.Editor
{
    [CustomEditor(typeof(L3Heirarchy), true)]
    public class L3HeirarchyEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset;// Assigned manually in script inspector.

        private TreeView treeView;

        public override VisualElement CreateInspectorGUI()
        {
            var root = m_VisualTreeAsset.CloneTree();
            treeView = root.Q<TreeView>("TreeView");
            treeView.makeItem = () => new Label();
            treeView.bindItem = (element, i) =>
            {
                var item = treeView.GetItemDataForIndex<L3HeirarchyNode>(i);
                (element as Label).text = item.name;
                // add a right click context menu to the element
                element.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
                {
                    evt.menu.AppendAction("Create Child Node", (a) =>
                    {
                        item.heirarchy.CreateNode<ExNode>(item);
                        RefreshTreeView();
                    });
                    evt.menu.AppendAction("Delete Node", (a) =>
                    {
                        // warning dialogue
                        if (EditorUtility.DisplayDialog("Delete Node?", "This will delete the node and all its children. Are you sure?", "Delete", "Cancel"))
                        {
                            item.heirarchy.DeleteNode(item);
                            RefreshTreeView();
                        }
                    });
                }));
            };

            treeView.itemIndexChanged += (idFrom, idTo) =>
            {
                Debug.Log($"Item (id){idFrom} moved to (id){idTo}");

                // global index
                int destinationIndex = treeView.viewController.GetIndexForId(idFrom);
                int parentIndex = treeView.viewController.GetIndexForId(idTo);
                // index relative to new parent
                int siblingIndex = destinationIndex - parentIndex - 1;

                Debug.Log($"Index: {destinationIndex} moved to {parentIndex}. Sibling index: {siblingIndex}");

                var nodeMoved = treeView.GetItemDataForId<L3HeirarchyNode>(idFrom);
                var newParent = treeView.GetItemDataForId<L3HeirarchyNode>(idTo);

                Debug.Log("New parent is null? " + (newParent == null));
                List<L3HeirarchyNode> nodesToMove = new List<L3HeirarchyNode>();
                foreach (int index in treeView.selectedIndices)
                {
                    var child = treeView.GetItemDataForIndex<L3HeirarchyNode>(index);
                    nodesToMove.Add(child);
                }

                // TODO: Handle multi-select move

                if (nodeMoved.parent == newParent)
                {
                    // Re-order
                    nodeMoved.SetIndexAction(siblingIndex);
                }
                else if (newParent == null)
                {
                    // Make parentless
                    nodeMoved.ReleaseParentAction();
                    nodeMoved.SetIndexAction(siblingIndex);
                }
                else
                {
                    // Re-parent
                    nodeMoved.SetParentAction(newParent, siblingIndex);
                }
                nodeMoved.heirarchy.

                RefreshTreeView();
            };

            treeView.SetRootItems(BuildTreeView());
            treeView.autoExpand = true;
            treeView.Rebuild();
            treeView.ExpandAll();


            return root;
        }

        private void RefreshTreeView()
        {
            if (treeView != null)
            {
                treeView.SetRootItems(BuildTreeView());
                treeView.Rebuild();
                treeView.ExpandAll();
            }
        }

        private List<TreeViewItemData<L3HeirarchyNode>> BuildTreeView()
        {
            List<TreeViewItemData<L3HeirarchyNode>> tree = new List<TreeViewItemData<L3HeirarchyNode>>();

            foreach (var node in ((L3Heirarchy)target).nodes)
            {
                if (node.parent == null)
                {
                    tree.Add(BuildTreeViewItem(node));
                }
            }
            return tree;
        }

        private TreeViewItemData<L3HeirarchyNode> BuildTreeViewItem(L3HeirarchyNode node)
        {
            var children = new List<TreeViewItemData<L3HeirarchyNode>>();
            foreach (var child in node.children)
            {
                children.Add(BuildTreeViewItem(child));
            }
            return new TreeViewItemData<L3HeirarchyNode>(node.GetInstanceID(), node, children);
        }
    }
}