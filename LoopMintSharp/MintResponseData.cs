using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class RawData
    {
        public string hash { get; set; }
        public int nftTokenId { get; set; }
        public string nftData { get; set; }
        public string status { get; set; }
        public bool isIdempotent { get; set; }
    }

    public class MintResponseData
    {
        public string hash { get; set; }
        public RawData raw_data { get; set; }
    }
}
