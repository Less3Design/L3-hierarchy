using System;
using Less3.Heirarchy;
using UnityEngine;

namespace Less3.Heirachy
{
    [CreateAssetMenu(menuName = "Less3/EX Heirarchy/Heirarchy", fileName = "ExNode", order = 1)]
    public class ExHeiarchy : L3Heirarchy
    {
        public string HeirachyData;


        [ContextMenu("Create Example Node")]
        public void CreateExampleNode()
        {
            this.CreateNode<ExNode>();
        }
    }
}
