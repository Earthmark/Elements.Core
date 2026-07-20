using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    // TODO!!! Should get rid of this eventually and instead use implicitly encodable structs - simply flag them with an attribute
    // The system will then automatically generate code for encoding them and saving them (they will need to use only other encodable structs
    // and primitives
    public interface IEncodable
    {
        void Encode(BinaryWriter writer);
        DataTreeNode Save();

        void Decode(BinaryReader reader);
        void Load(DataTreeNode node);
    }
}
