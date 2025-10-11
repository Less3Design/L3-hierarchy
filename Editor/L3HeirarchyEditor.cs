using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Less3.Heirachy;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

namespace Less3.Heirarchy.Editor
{
    public class L3HeirarchyInspector : UnityEditor.EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset;// Assigned manually in script inspector.
        [SerializeField] private VisualTreeAsset m_element_VisualTreeAsset;// Assigned manually in script inspector.
        [SerializeField] private L3Heirarchy target;

        private TreeView treeView;
        private VisualElement inspectorContainer;

        [OnOpenAsset(1)]
        public static bool DoubleClickAsset(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is L3Heirarchy asset)
            {
                OpenForAsset(asset);
                return true; // we handled the open
            }
            return false; // we did not handle the open
        }

        //on unity undo was called
        private void OnUndoRedoPerformed()
        {
            if (target != null)
            {
                RefreshTreeView();
            }
        }

        private void OnEnable()
        {
            if (target != null)
            {
                InitGUI(target);
            }
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        public static void OpenForAsset(L3Heirarchy asset)
        {
            var window = GetWindow<L3HeirarchyInspector>();
            window.InitGUI(asset);
        }

        public void InitGUI(L3Heirarchy heirarchy)
        {
            this.titleContent = new GUIContent(heirarchy.name);
            target = heirarchy;
            var root = m_VisualTreeAsset.CloneTree();
            rootVisualElement.Clear();
            rootVisualElement.Add(root);

            inspectorContainer = root.Q("InspectorContainer");

            treeView = root.Q<TreeView>("TreeView");
            treeView.makeItem = () => m_element_VisualTreeAsset.CloneTree();
            treeView.bindItem = (element, i) =>
            {
                var item = treeView.GetItemDataForIndex<L3HeirarchyNode>(i);

                element.Q<Label>("Title").text = item.name;
                element.Q<Label>("SubTitle").text = item.name;
                //element.Q<Label>("SubTitle").style.display = DisplayStyle.None;

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
                // global index
                int destinationIndex = treeView.viewController.GetIndexForId(idFrom);
                int parentIndex = treeView.viewController.GetIndexForId(idTo);
                // index relative to new parent
                int siblingIndex = destinationIndex - parentIndex - 1;

                var nodeMoved = treeView.GetItemDataForId<L3HeirarchyNode>(idFrom);
                var newParent = treeView.GetItemDataForId<L3HeirarchyNode>(idTo);

                List<L3HeirarchyNode> nodesToMove = new List<L3HeirarchyNode>();
                foreach (int index in treeView.selectedIndices)
                {
                    var child = treeView.GetItemDataForIndex<L3HeirarchyNode>(index);
                    nodesToMove.Add(child);
                }

                int i = 0;
                if (newParent == null)
                {
                    foreach (var n in nodesToMove)
                    {
                        if (n.heirarchy.ValidateParentAction(n, newParent))
                        {
                            n.ReleaseParentAction(siblingIndex + i);
                            i++;
                        }
                    }
                }
                else
                {
                    foreach (var n in nodesToMove)
                    {
                        if (n.heirarchy.ValidateParentAction(n, newParent))
                        {
                            n.SetParentAction(newParent, siblingIndex + i);
                            i++;
                        }
                    }
                }
                nodeMoved.heirarchy.UpdateTree_EDITOR();
                RefreshTreeView();// todo remove.
            };

            treeView.selectionChanged += (items) =>
            {
                List<L3HeirarchyNode> nodes = new List<L3HeirarchyNode>();
                foreach (var obj in items)
                {
                    if (obj is L3HeirarchyNode node)
                    {
                        nodes.Add(node);
                    }
                }
                UpdateSelction(nodes);
            };

            treeView.SetRootItems(BuildTreeView());
            treeView.autoExpand = true;
            treeView.Rebuild();
            treeView.ExpandAll();
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

        private void UpdateSelction(List<L3HeirarchyNode> node)
        {
            inspectorContainer.Clear();
            // create a serialized object of the array
            var serializedObject = new SerializedObject(node.ToArray());
            inspectorContainer.Add(new InspectorElement(serializedObject));
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