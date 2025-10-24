using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Less3.Hierarchy;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using Less3.TypeTree.Editor;
using System.Linq;

namespace Less3.Hierarchy.Editor
{
    public class L3HierarchyEditor : UnityEditor.EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset;// Assigned manually in script inspector.
        [SerializeField] private VisualTreeAsset m_element_VisualTreeAsset;// Assigned manually in script inspector.
        [SerializeField, HideInInspector] private L3Hierarchy target;
        [SerializeField] private Texture2D windowIcon;

        private TreeView treeView;
        private VisualElement rootContainer;
        private VisualElement inspectorContainer;
        private VisualElement inspectorRoot;

        private Label selectedTypeName;
        private Label selectedObjName;

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

                if (rootContainer != null)
                {
                    if (rootContainer.resolvedStyle.width > 800)
                    {
                        rootContainer.style.flexDirection = FlexDirection.Row;
                    }
                    else
                    {
                        rootContainer.style.flexDirection = FlexDirection.Column;
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
            rootContainer = root.Q<VisualElement>("RootContainer");
            rootVisualElement.Clear();
            rootVisualElement.Add(root);
            root.Q<Label>("TypeName").text = Hierarchy.GetType().Name;
            root.Q<Label>("ObjName").text = Hierarchy.name;

            inspectorContainer = root.Q("InspectorContainer");
            inspectorRoot = root.Q("InspectorRoot");
            selectedTypeName = root.Q<Label>("SelectedType");
            selectedObjName = root.Q<Label>("SelectedName");

            // heirarchy type open
            root.Q<Label>("TypeName").AddManipulator(new Clickable(() =>
            {
                //open the object type script in script editor
                //get the m_script
                var so = new SerializedObject(target);
                var m_script = so.FindProperty("m_Script").objectReferenceValue;
                AssetDatabase.OpenAsset(m_script);
            }));

            // selected obj type open
            selectedTypeName.AddManipulator(new Clickable(() =>
            {
                //open the object type script in script editor
                //get the m_script
                List<L3HierarchyNode> selectedNodes = new List<L3HierarchyNode>();
                foreach (int index in treeView.selectedIndices)
                {
                    var node = treeView.GetItemDataForIndex<L3HierarchyNode>(index);
                    selectedNodes.Add(node);
                }

                if (selectedNodes.Count > 0)
                {
                    //if all types are the same
                    if (selectedNodes.All(n => n.GetType() == selectedNodes[0].GetType()))
                    {
                        var so = new SerializedObject(selectedNodes[0]);
                        var m_script = so.FindProperty("m_Script").objectReferenceValue;
                        AssetDatabase.OpenAsset(m_script);
                    }
                }
            }));



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
                        L3TypeTreeWindow.OpenForType(item.Hierarchy.GetType(), element.worldTransform.GetPosition(), (type) =>
                        {
                            var newNode = item.Hierarchy.CreateNode(type, item);
                            RefreshTreeView();
                            ForceSelectNode(newNode);
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
            /*
            treeView.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Add Node", (a) =>
                {
                    L3TypeTreeWindow.OpenForType(target.GetType(), evt.mousePosition, (type) =>
                    {
                        var newNode = target.CreateNode(type, null);
                        RefreshTreeView();
                    });
                });
            }));
            */

            var addButton = root.Q<ToolbarButton>("AddNodeButton");
            addButton.clicked += () =>
            {
                L3TypeTreeWindow.OpenForType(target.GetType(), addButton.worldTransform.GetPosition(), (type) =>
                {
                    var newNode = target.CreateNode(type, null);
                    RefreshTreeView();
                    ForceSelectNode(newNode);
                });
            };

            var addChildButton = root.Q<ToolbarButton>("AddChildNodeButton"); ;
            addChildButton.clicked += () =>
            {
                List<L3HierarchyNode> selectedNodes = new List<L3HierarchyNode>();
                foreach (int index in treeView.selectedIndices)
                {
                    var node = treeView.GetItemDataForIndex<L3HierarchyNode>(index);
                    selectedNodes.Add(node);
                }

                if (selectedNodes.Count == 1)
                {
                    L3TypeTreeWindow.OpenForType(target.GetType(), addChildButton.worldTransform.GetPosition(), (type) =>
                    {
                        var newNode = target.CreateNode(type, selectedNodes[0]);
                        RefreshTreeView();
                        ForceSelectNode(newNode);
                    });
                }
                else
                {
                    EditorUtility.DisplayDialog("Select One Parent Node", "Please select a single parent node to add a child to.", "OK");
                }
            };

            treeView.SetRootItems(BuildTreeView());
            treeView.autoExpand = true;
            treeView.Rebuild();
            treeView.ExpandAll();
            UpdateSelction(new List<L3HierarchyNode>());
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

            if (node.Count == 0)
            {
                inspectorRoot.style.display = DisplayStyle.None;
                return;
            }
            else
            {
                inspectorRoot.style.display = DisplayStyle.Flex;
            }

            //check if they are all the same type
            for (int i = 1; i < node.Count; i++)
            {
                if (node[i].GetType() != node[0].GetType())
                {
                    // they are not the same type
                    selectedTypeName.text = "---";
                    selectedObjName.text = "";
                    var label = new Label("Multiple Different Types Selected");
                    label.style.paddingBottom = 16;
                    label.style.paddingTop = 16;
                    label.style.paddingLeft = 16;
                    label.style.paddingRight = 16;

                    inspectorContainer.Add(label);
                    return;
                }
            }

            // create a serialized object of the array
            var serializedObject = new SerializedObject(node.ToArray());
            inspectorContainer.Add(new InspectorElement(serializedObject));

            selectedTypeName.text = node[0].GetType().Name;
            if (node.Count == 1)
            {
                if (node[0] is IHierarchyNodeTitle customTitle)
                {
                    selectedObjName.text = customTitle.NodeTitle;
                }
                else
                {
                    selectedObjName.text = node[0].name;
                }
            }
            else
            {
                selectedObjName.text = $"{node.Count} Nodes Selected";
            }
        }

        private void ForceSelectNode(L3HierarchyNode node)
        {
            List<int> indices = new List<int>();
            foreach (var kvp in nodeElements)
            {
                if (kvp.Value.node == node)
                {
                    indices.Add(kvp.Key);
                    break;
                }
            }
            treeView.SetSelection(indices);
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