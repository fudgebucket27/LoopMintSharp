using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public interface ILoopringMintService
    {
        Task<StorageId> GetNextStorageId(string apiKey, int accountId, int sellTokenId);
        Task<CounterFactualNft> ComputeTokenAddress(string apiKey, CounterFactualNftInfo counterFactualNftInfo);
    }
}
