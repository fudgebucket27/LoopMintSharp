using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp.Models
{
    public class RedPacketNftMintResponse
    {
            public string hash { get; set; }
            public string status { get; set; }
            public bool isIdempotent { get; set; }

            public string nftData { get; set; }
            public int accountId { get; set; }
            public int tokenId { get; set; }
            public int storageId { get; set; }
        public string errorMessage { get; set; }
    }
}
