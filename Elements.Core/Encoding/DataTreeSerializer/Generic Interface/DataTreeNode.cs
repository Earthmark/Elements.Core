using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public abstract class DataTreeNode
    {
        public abstract IEnumerable<DataTreeNode> EnumerateTree();
    }
}
