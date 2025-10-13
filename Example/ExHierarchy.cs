#if LESS3_EXAMPLES
using System;
using Less3.Hierarchy;
using UnityEngine;

namespace Less3.Hierarchy
{
    [CreateAssetMenu(menuName = "Less3/EX Hierarchy/Hierarchy", fileName = "ExNode", order = 1)]
    public class ExHierarchy : L3Hierarchy
    {
        public string HierarchyData;


        [ContextMenu("Create Example Node")]
        public void CreateExampleNode()
        {
            this.CreateNode<ExNode>();
        }
    }
}
#endif
