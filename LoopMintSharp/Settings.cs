using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class Settings
    {
        public string LoopringApiKey { get; set; }
        public string LoopringPrivateKey { get; set; }
        public string LoopringAddress { get; set; }
        public int LoopringAccountId { get; set; }
        public int NftAmount { get; set; }
        public int NftRoyaltyPercentage { get; set; }
        public int NftType { get; set; }
        public long ValidUntil { get; set; }
        public int MaxFeeTokenId { get; set; }
        public string NftFactory { get; set; }
        public string Exchange { get; set; }
        public bool VerboseLogging { get;set;}

        public bool SkipMintFeePrompt { get; set; }
    }
}
