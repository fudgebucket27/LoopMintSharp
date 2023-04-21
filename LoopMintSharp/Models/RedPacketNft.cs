using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp.Models
{
    public class LuckyToken
    {
        public string exchange { get; set; }
        public string payerAddr { get; set; }
        public int payerId { get; set; }
        public string payeeAddr { get; set; }
        public int storageId { get; set; }
        public int token { get; set; }
        public string amount { get; set; }
        public int feeToken { get; set; }
        public string maxFeeAmount { get; set; }
        public long validUntil { get; set; }
        public int payeeId { get; set; }
        public string memo { get; set; }
        public string eddsaSig { get; set; }
    }

    public class RedPacketNft
    {
        public string ecdsaSignature { get; set; }
        public string? giftNumbers { get; set; }
        public LuckyToken luckyToken { get; set; }
        public string memo { get; set; }
        public string nftData { get; set; }
        public string numbers { get; set; }
        public bool signerFlag { get; set; }
        public int templateId { get; set; }
        public Type type { get; set; }
        public long validSince { get; set; }
        public long validUntil { get; set; }


    }

    public class Type
    {
        public int partition { get; set; }
        public int mode { get; set; }
        public int scope { get; set; }
    }
}
