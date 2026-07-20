using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class StringNodeMap : IStringNode,
    IEnumerable<KeyValuePair<string, IStringNode>>,
    IEnumerator<KeyValuePair<string, IStringNode>>
{
    Dictionary<string, object> map;

    public StringNodeMap()
    {
        map = new Dictionary<string, object>();
    }

    public IStringNode Add(string key, string str)
    {
        return Add(key, new StringNodeElement(str));
    }

    public IStringNode Add(string key, IStringNode node)
    {
        map.Add(key, node.BoxData());
        return node;
    }

    public int Count { get { return map.Count; } }

    public IStringNode this[string key]
    {
        get { return (IStringNode)map[key]; }
    }

    public bool HasKey(string key)
    {
        return map.ContainsKey(key);
    }

    public object BoxData()
    {
        return map;
    }

    IEnumerator<KeyValuePair<string, object>> dictEnumerator;

    public IEnumerator<KeyValuePair<string, IStringNode>> GetEnumerator()
    {
        dictEnumerator = map.GetEnumerator();
        return this;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        dictEnumerator = map.GetEnumerator();
        return this; 
    }

    public KeyValuePair<string, IStringNode> Current
    {
        get
        {
            var item = dictEnumerator.Current;
            return new KeyValuePair<string, IStringNode>(item.Key,
                item.Value as IStringNode);
        }
    }

    object System.Collections.IEnumerator.Current
    {
        get { return dictEnumerator.Current; }
    }

    public bool MoveNext()
    {
        return dictEnumerator.MoveNext();
    }

    public void Reset()
    {
        dictEnumerator.Reset();
    }

    public void Dispose()
    {
        dictEnumerator.Dispose();
    }
}
