using Less3.TypeTree;
using UnityEngine;

namespace Less3.Heirachy
{
    [TypeTreeMenu(typeof(ExNode), "Example/test/ExNode")]
    public class ExNode : L3HeirarchyNode
    {
        public string nodeData;
    }
}
