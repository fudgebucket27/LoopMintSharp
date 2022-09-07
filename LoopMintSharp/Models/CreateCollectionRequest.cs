using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class CreateCollectionRequest
    {
        public string avatar { get; set; }
        public string banner { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string nftFactory { get; set; }
        public string owner { get; set; }
        public string tileUri { get; set; }
    }
}
