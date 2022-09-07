using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{

    public class Collection
    {
        public int id { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public string contractAddress { get; set; }
        public string collectionAddress { get; set; }
        public bool isPublic { get; set; }
        public string baseUri { get; set; }
        public string nftFactory { get; set; }
        public string collectionTitle { get; set; }
        public string description { get; set; }
        public string avatar { get; set; }
        public string banner { get; set; }
        public string thumbnail { get; set; }
        public string cid { get; set; }
        public string tileUri { get; set; }
        public string deployStatus { get; set; }
        public bool isCounterFactualNFT { get; set; }
        public bool isMintable { get; set; }
        public string nftType { get; set; }
        public long createdAt { get; set; }
        public long updatedAt { get; set; }

    }
    public class Collections
    {
        public Collection collection { get; set; }

    }
    public class CollectionResult
    {
        public IList<Collections> collections { get; set; }
        public int totalNum { get; set; }

    }
}
