using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class NftData
    {
        public string nftData { get; set; }
        public string minter { get; set; }
        public string nftType { get; set; }
        public string tokenAddress { get; set; }
        public string nftId { get; set; }
        public int creatorFeeBips { get; set; }
        public int royaltyPercentage { get; set; }
        public int originalRoyaltyPercentage { get; set; }
        public bool status { get; set; }
        public string nftFactory { get; set; }
        public string nftOwner { get; set; }
        public string nftBaseUri { get; set; }
        public string royaltyAddress { get; set; }
        public string originalMinter { get; set; }
        public long createdAt { get; set; }
    }
}
