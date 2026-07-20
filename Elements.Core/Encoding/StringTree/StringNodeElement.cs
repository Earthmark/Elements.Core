using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class StringNodeElement : IStringNode
{
    public string String { get { return str; } }

    string str;

    public StringNodeElement(string str)
    {
        this.str = str;
    }

    public object BoxData()
    {
        return str;
    }
}
