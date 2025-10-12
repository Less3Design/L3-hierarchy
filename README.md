<img width="1787" height="412" alt="Banner" src="https://github.com/user-attachments/assets/a0e998c5-1e48-4713-bb51-7d5cb153efe9" />

A framework to create Scriptable Objects that are a tree of many sub-nodes.

## Setup
This repo is dependant on [L3-typeTree](https://github.com/Less3Design/L3-typeTree), and must be added as packages together to function.

Add both packages to the package manager in Unity:
```
https://github.com/Less3Design/L3-typeTree.git
https://github.com/Less3Design/L3-hierarchy.git
```

## Usage
A hierarchy is made up of at least 2 object types, a `L3Hierarchy` and `L3HierarchyNode`s.

### Heirarchy

The `L3Hierarchy` object is what holds all of our nodes. It does not maintain any hierarchy structure itself.
```c#
using Less3.Hierarchy;

[CreateAssetMenu(menuName = "Less3/Examples/Hierarchy", fileName = "NewExampleHierarchy", order = 1)]
public class ExampleHierarchy : L3Hierarchy
{
    // settings that affect the entire heirarchy can go here
}
```
### Nodes
`L3HierarchyNodes` make up the content of your tree. They function similair to Unity's `Transform` component, where they maintain a `parent` and list of `children`.

The `[TypeTreeMenu]` attribute is used to place this node into the "add new node" menu. The type parameter should match the heirarchy type you plan to use this node inside.
```c#
using Less3.Hierarchy;
using Less3.TypeTree;

[TypeTreeMenu(typeof(ExampleHierarchy), "Nodes/MyExample")]
public class ExampleNode : L3HierarchyNode, IHierarchyNodeTitle
{
    public string customTitle;
    public string NodeTitle => string.IsNullOrEmpty(customTitle) ? name : customTitle;
}
```

### Tips
- Different node types can parent to each other.
- Nodes can be styled using interfaces found in `L3HierarchyNodeStyleInterfaces.cs`
- A node type can be used in multiple hierarchy types. You just need to define mutiple `[TypeTreeMenu]` attributes on the node.
