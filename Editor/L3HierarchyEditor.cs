using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Less3.Hierarchy;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using Less3.TypeTree.Editor;

namespace Less3.Hierarchy.Editor
{
    public class L3HierarchyEditor : UnityEditor.EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset;// Assigned manually in script inspector.
        [SerializeField] private VisualTreeAsset m_element_VisualTreeAsset;// Assigned manually in script inspector.
        [SerializeField, HideInInspector] private L3Hierarchy target;
        [SerializeField] private Texture2D windowIcon;

        private TreeView treeView;
        private VisualElement inspectorContainer;

        private Dictionary<int, L3HierarchyNodeElement> nodeElements = new Dictionary<int, L3HierarchyNodeElement>();

        [OnOpenAsset(1)]
        public static bool DoubleClickAsset(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is L3Hierarchy asset)
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
            EditorApplication.update += Update;//
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.update -= Update;//
        }

        private void Update()
        {
            if (treeView != null && target != null)
            {
                // update selected indexes
                foreach (int i in treeView.selectedIndices)
                {
                    if (nodeElements.ContainsKey(i))
                    {
                        nodeElements[i].UpdateContent();
                    }
                }
            }
        }

        public static void OpenForAsset(L3Hierarchy asset)
        {
            var window = GetWindow<L3HierarchyEditor>();
            window.InitGUI(asset);
        }

        public void InitGUI(L3Hierarchy Hierarchy)
        {
            this.titleContent = new GUIContent(Hierarchy.name, windowIcon);
            target = Hierarchy;
            var root = m_VisualTreeAsset.CloneTree();
            rootVisualElement.Clear();
            rootVisualElement.Add(root);
            root.Q<Label>("TypeName").text = Hierarchy.GetType().Name;

            inspectorContainer = root.Q("InspectorContainer");

            treeView = root.Q<TreeView>("TreeView");
            treeView.makeItem = () => m_element_VisualTreeAsset.CloneTree();
            treeView.bindItem = (element, i) =>
            {
                var item = treeView.GetItemDataForIndex<L3HierarchyNode>(i);

                if (!nodeElements.ContainsKey(item.GetInstanceID()))
                {
                    nodeElements[i] = new L3HierarchyNodeElement(element, item);
                }
                else
                {
                    Debug.LogError("Already contains element for node index " + i);
                }

                // add a right click context menu to the element
                element.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
                {
                    evt.menu.AppendAction("Add node as Child", (a) =>
                    {
                        L3TypeTreeWindow.OpenForType(item.Hierarchy.GetType(), (type) =>
                        {
                            var newNode = item.Hierarchy.CreateNode(type, item);
                            RefreshTreeView();
                        });
                    });
                    evt.menu.AppendAction("Delete Node", (a) =>
                    {
                        // warning dialogue
                        if (EditorUtility.DisplayDialog("Delete Node?", "This will delete the node and all its children. Are you sure?", "Delete", "Cancel"))
                        {
                            item.Hierarchy.DeleteNode(item);
                            RefreshTreeView();
                        }
                    });
                    evt.StopPropagation();
                }));
            };
            treeView.unbindItem = (element, i) =>
            {
                if (nodeElements.ContainsKey(i))
                {
                    nodeElements.Remove(i);
                }
            };

            treeView.itemIndexChanged += (idFrom, idTo) =>
            {
                // global index
                int destinationIndex = treeView.viewController.GetIndexForId(idFrom);
                int parentIndex = treeView.viewController.GetIndexForId(idTo);
                // index relative to new parent
                int siblingIndex = destinationIndex - parentIndex - 1;

                var nodeMoved = treeView.GetItemDataForId<L3HierarchyNode>(idFrom);
                var newParent = treeView.GetItemDataForId<L3HierarchyNode>(idTo);

                List<L3HierarchyNode> nodesToMove = new List<L3HierarchyNode>();
                foreach (int index in treeView.selectedIndices)
                {
                    var child = treeView.GetItemDataForIndex<L3HierarchyNode>(index);
                    nodesToMove.Add(child);
                }

                int i = 0;
                if (newParent == null)
                {
                    foreach (var n in nodesToMove)
                    {
                        if (n.Hierarchy.ValidateParentAction(n, newParent))
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
                        if (n.Hierarchy.ValidateParentAction(n, newParent))
                        {
                            n.SetParentAction(newParent, siblingIndex + i);
                            i++;
                        }
                    }
                }
                nodeMoved.Hierarchy.UpdateTree_EDITOR();
                RefreshTreeView();// todo remove.
            };

            treeView.selectionChanged += (items) =>
            {
                List<L3HierarchyNode> nodes = new List<L3HierarchyNode>();
                foreach (var obj in items)
                {
                    if (obj is L3HierarchyNode node)
                    {
                        nodes.Add(node);
                    }
                }
                UpdateSelction(nodes);
            };

            // * background right click.
            treeView.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Add Node", (a) =>
                {
                    L3TypeTreeWindow.OpenForType(target.GetType(), (type) =>
                    {
                        var newNode = target.CreateNode(type, null);
                        RefreshTreeView();
                    });
                });
            }));

            root.Q<ToolbarButton>("AddNodeButton").clicked += () =>
            {
                L3TypeTreeWindow.OpenForType(target.GetType(), (type) =>
                {
                    var newNode = target.CreateNode(type, null);
                    RefreshTreeView();
                });
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

        private void UpdateSelction(List<L3HierarchyNode> node)
        {
            inspectorContainer.Clear();

            //check if they are all the same type
            for (int i = 1; i < node.Count; i++)
            {
                if (node[i].GetType() != node[0].GetType())
                {
                    // they are not the same type
                    inspectorContainer.Add(new Label("Multiple Different Types Selected"));
                    return;
                }
            }

            // create a serialized object of the array
            var serializedObject = new SerializedObject(node.ToArray());
            inspectorContainer.Add(new InspectorElement(serializedObject));
        }

        private List<TreeViewItemData<L3HierarchyNode>> BuildTreeView()
        {
            List<TreeViewItemData<L3HierarchyNode>> tree = new List<TreeViewItemData<L3HierarchyNode>>();

            foreach (var node in ((L3Hierarchy)target).nodes)
            {
                if (node.parent == null)
                {
                    tree.Add(BuildTreeViewItem(node));
                }
            }
            return tree;
        }

        private TreeViewItemData<L3HierarchyNode> BuildTreeViewItem(L3HierarchyNode node)
        {
            var children = new List<TreeViewItemData<L3HierarchyNode>>();
            foreach (var child in node.children)
            {
                children.Add(BuildTreeViewItem(child));
            }
            return new TreeViewItemData<L3HierarchyNode>(node.GetInstanceID(), node, children);
        }
    }
}