using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LoopMintSharp
{
    public class Datum
    {
        public int id { get; set; }
        public int accountId { get; set; }
        public int tokenId { get; set; }
        public string nftData { get; set; }
        public string tokenAddress { get; set; }
        public string nftId { get; set; }
        public string nftType { get; set; }
        public string total { get; set; }
        public string locked { get; set; }
        public Pending pending { get; set; }
        public string deploymentStatus { get; set; }
        public bool isCounterFactualNFT { get; set; }

        public Metadata metadata { get; set; }

    }

    public class Metadata
    {
        [JsonProperty(PropertyName = "base")]
        public Base basename { get; set; }
    }

    public class Base
    {
        public string name { get; set; }
    }

    public class Pending
    {
        public string withdraw { get; set; }
        public string deposit { get; set; }
    }

    public class NftBalance
    {
        public int totalNum { get; set; }
        public List<Datum> data { get; set; }
    }
}
