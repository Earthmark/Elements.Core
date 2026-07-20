using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class SavedGraph
    {
        public readonly DataTreeDictionary Root;
        public readonly List<DataTreeValue> URLNodes;

        public SavedGraph(DataTreeDictionary root)
        {
            this.Root = root;

            // search for all nodes
            URLNodes = new List<DataTreeValue>();

            URLNodes.AddRange(root.EnumerateTree().Where(n => n is DataTreeValue).Cast<DataTreeValue>()
                .Where(n => n.IsURL));
        }
    }
}
