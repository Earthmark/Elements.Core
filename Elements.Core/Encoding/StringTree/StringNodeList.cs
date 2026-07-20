using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class StringNodeList : IStringNode, IEnumerable<IStringNode>,
    IEnumerator<IStringNode>
{
    List<object> list;

    public StringNodeList()
    {
        list = new List<object>();
    }

    public IStringNode Add(IStringNode node)
    {
        list.Add(node.BoxData());
        return node;
    }

    public int Count { get { return list.Count; } }

    public IStringNode this[int index]
    {
        get
        {
            return (IStringNode)list[index];
        }
    }

    public object BoxData()
    {
        return list;
    }

    IEnumerator<object> listEnumerator;

    public IEnumerator<IStringNode> GetEnumerator()
    {
        listEnumerator = list.GetEnumerator();
        return this;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        listEnumerator = list.GetEnumerator();
        return this;
    }

    public IStringNode Current
    {
        get { return (IStringNode)listEnumerator.Current; }
    }

    object System.Collections.IEnumerator.Current
    {
        get { return listEnumerator.Current; }
    }

    public bool MoveNext()
    {
        return listEnumerator.MoveNext();
    }

    public void Reset()
    {
        listEnumerator.Reset();
    }

    public void Dispose()
    {
        listEnumerator.Dispose();
    }
}
