using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Less3.Hierarchy
{
    /// <summary>
    /// Interface used by the editor.
    /// </summary>
    public interface IHierarchyNodeElement
    {
        string NodeName { get; }
        IHierarchyNodeElement ParentElement { get; }
        List<IHierarchyNodeElement> ChildrenElements { get; }
        L3Hierarchy Hierarchy { get; }
    }

    /// <summary>
    /// a "fake" node that you can inject into the hierarchy. Use when you have a "real" node, and you want
    /// to display "generated children" or something like that.
    /// </summary>
    public class InjectedHierarchyNode : IHierarchyNodeElement
    {
        public string name;
        public IHierarchyNodeElement parent;
        public List<IHierarchyNodeElement> children;
        public L3Hierarchy hierarchy;
        private System.Guid _guid = System.Guid.NewGuid();

        public string NodeName => name;
        public IHierarchyNodeElement ParentElement => parent;
        public List<IHierarchyNodeElement> ChildrenElements => children;
        public L3Hierarchy Hierarchy => hierarchy;

        public InjectedHierarchyNode(string name, IHierarchyNodeElement parent, L3Hierarchy hierarchy)
        {
            this.name = name;
            this.parent = parent;
            this.children = new List<IHierarchyNodeElement>();
            this.hierarchy = hierarchy;
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
    }

    public abstract class L3HierarchyNode : ScriptableObject, IHierarchyNodeElement
    {
#pragma warning disable 0414
        [SerializeField, HideInInspector]
        private int serializedVersion = 1;// For future use (maybe)
#pragma warning restore 0414

        // Nodes are arranged based on each nodes internal parent / child references.
        // Modeled after GameObject transforms.
        [SerializeField, HideInInspector]
        public L3HierarchyNode parent;
        // Order of this list is the order of the children, top / first is index 0
        [SerializeField, HideInInspector]
        public List<L3HierarchyNode> children = new List<L3HierarchyNode>();

        [SerializeField, HideInInspector]
        private L3Hierarchy _Hierarchy;
        public L3Hierarchy Hierarchy => _Hierarchy;

        public string NodeName => name;
        public IHierarchyNodeElement ParentElement => parent as IHierarchyNodeElement;
        // The editor can display extra injected children elements.
        public List<IHierarchyNodeElement> ChildrenElements => InjectChildren(children.Cast<IHierarchyNodeElement>().ToList());

        public int Index
        {
            get
            {
                // TODO actually we should return the order of this node in the Hierarchy.
                // like compare against all parentless nodes.
                if (parent == null || parent.children == null)
                    return 0;

                return parent.children.IndexOf(this);
            }
        }

        public L3HierarchyNode GetRoot()
        {
            L3HierarchyNode current = this;
            while (current.parent != null)
            {
                current = current.parent;
            }
            return current;
        }

        public L3HierarchyNode GetChildAtIndex(int index)
        {
            if (index < 0 || index >= children.Count)
                return null;

            return children[index];
        }

        public int GetChildIndex(L3HierarchyNode child)
        {
            if (child == null)
                return -1;

            return children.IndexOf(child);
        }

        /// <summary>
        /// Returns a copy of the children list.
        /// </summary>
        public List<L3HierarchyNode> GetChildren()
        {
            return new List<L3HierarchyNode>(children);
        }

        public List<T> GetChildrenOfType<T>() where T : L3HierarchyNode
        {
            List<T> typedChildren = new List<T>();
            foreach (var child in children)
            {
                if (child is T typedChild)
                {
                    typedChildren.Add(typedChild);
                }
            }
            return typedChildren;
        }

        public void InitNode(L3Hierarchy Hierarchy)
        {
            if (_Hierarchy != null)
            {
                Debug.LogWarning("An L3HierarchyNode can only be initialized once.");
                return;
            }

            this._Hierarchy = Hierarchy;
        }

        public virtual List<IHierarchyNodeElement> InjectChildren(List<IHierarchyNodeElement> children)
        {
            return children;
        }
    }
}
