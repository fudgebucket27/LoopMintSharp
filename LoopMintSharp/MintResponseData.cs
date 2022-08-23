using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class MintResponseData
    {
            public string hash { get; set; }
            public int nftTokenId { get; set; }

            public string nftId { get; set; }
            public string nftData { get; set; }
            public string status { get; set; }
            public bool isIdempotent { get; set; }
            public int accountId { get; set; }
            public int storageId { get; set; }
            public string metadataCid { get; set; }
            public string errorMessage { get; set; }
    }
}
