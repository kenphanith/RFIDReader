using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFIDTagReader
{
    public class RFIDEventArgs: EventArgs
    {
        public string TagData { get; private set; }

        public RFIDEventArgs(string tagData)
        {
            this.TagData = tagData;
        }
    }
}
