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
        Task<OffchainFee> GetOffChainFee(string apiKey, int accountId, int requestType, string tokenAddress);
        Task<MintResponseData> MintNft(
            string apiKey, 
            string exchange, 
            int minterId, 
            string minterAddress, 
            int toAccountId, 
            string toAddress,
            int nftType, 
            string tokenAddress, 
            string nftId,
            string amount, 
            long validUntil, 
            int creatorFeeBips, 
            int storageId, 
            int maxFeeTokenId, 
            string maxFeeAmount, 
            bool forceToMint, 
            CounterFactualNftInfo counterFactualNftInfo,
            string eddsaSignature);
    }
}
