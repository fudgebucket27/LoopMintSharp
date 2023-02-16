using LoopMintSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public interface ILoopringMintService
    {
        Task<StorageId> GetNextStorageId(string apiKey, int accountId, int sellTokenId, bool verboseLogging);
        Task<CounterFactualNft> ComputeTokenAddress(string apiKey, CounterFactualNftInfo counterFactualNftInfo, bool verboseLogging);
        Task<OffchainFee> GetOffChainFee(string apiKey, int accountId, int requestType, string tokenAddress, bool verboseLogging);
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
            int royaltyPercentage,
            int storageId,
            int maxFeeTokenId,
            string maxFeeAmount,
            bool forceToMint,
            CounterFactualNftInfo counterFactualNftInfo,
            string eddsaSignature,
            bool verboseLogging,
            string royaltyAddress);

        Task<string> MintRedPacketNft
        (
            string apiKey,
            string apiSig,
            RedPacketNft redPacketNft,
            bool verboseLogging
        );

        Task<NftBalance> GetTokenIdWithCheck(string apiKey, int accountId, string nftData);

        Task<CreateCollectionResult> CreateNftCollection(
            string apiKey,
            CreateCollectionRequest createCollectionRequest,
            string apiSig,
            bool verboseLogging);

        Task<CollectionResult> FindNftCollection(
             string apiKey,
             int limit,
             int offset,
             string owner,
             string tokenAddress,
             bool verboseLogging);

    }
}
