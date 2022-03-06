using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class LoopringMintService : ILoopringMintService, IDisposable
    {
        const string _baseUrl = "https://api3.loopring.io";

        readonly RestClient _client;

        public LoopringMintService()
        {
            _client = new RestClient(_baseUrl);
        }

        public async Task<StorageId> GetNextStorageId(string apiKey, int accountId, int sellTokenId)
        {
            var request = new RestRequest("api/v3/storageId");
            request.AddHeader("x-api-key", apiKey);
            request.AddParameter("accountId", accountId);
            request.AddParameter("sellTokenId", sellTokenId);
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<StorageId>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                Console.WriteLine($"Error getting storage id {httpException.Message}");
                return null;
            }
        }

        public async Task<CounterFactualNft> ComputeTokenAddress(string apiKey, CounterFactualNftInfo counterFactualNftInfo)
        {
            var request = new RestRequest("api/v3/nft/info/computeTokenAddress");
            request.AddHeader("x-api-key", apiKey);
            request.AddParameter("nftFactory", counterFactualNftInfo.nftFactory);
            request.AddParameter("nftOwner", counterFactualNftInfo.nftOwner);
            request.AddParameter("nftBaseUri", counterFactualNftInfo.nftBaseUri);
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<CounterFactualNft>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                Console.WriteLine($"Error getting storage id: {httpException.Message}");
                return null;
            }
        }

        public async Task<OffchainFee> GetOffChainFee(string apiKey, int accountId, int requestType, string tokenAddress)
        {
            var request = new RestRequest("api/v3/user/nft/offchainFee");
            request.AddHeader("x-api-key", apiKey);
            request.AddParameter("accountId", accountId);
            request.AddParameter("requestType", requestType);
            request.AddParameter("tokenAddress", tokenAddress);
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<OffchainFee>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                Console.WriteLine($"Error getting off chain fee: {httpException.Message}");
                return null;
            }
        }

        public async Task<MintResponseData> MintNft(
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
            string eddsaSignature)
        {
            var request = new RestRequest("api/v3/nft/mint");
            request.AddHeader("x-api-key", apiKey);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("exchange", exchange);
            request.AddParameter("minterId", minterId);
            request.AddParameter("minterAddress", minterAddress);
            request.AddParameter("toAccountId", toAccountId);
            request.AddParameter("toAddress", toAddress);
            request.AddParameter("nftType", nftType);
            request.AddParameter("tokenAddress", tokenAddress);
            request.AddParameter("nftId", nftId);
            request.AddParameter("amount", amount);
            request.AddParameter("validUntil", validUntil);
            request.AddParameter("creatorFeeBips", creatorFeeBips);
            request.AddParameter("storageId", storageId);
            request.AddParameter("maxFee.tokenId", maxFeeTokenId);
            request.AddParameter("maxFee.amount", maxFeeAmount);
            request.AddParameter("forceToMint", "false");
            request.AddParameter("counterFactualNftInfo.nftFactory", counterFactualNftInfo.nftFactory);
            request.AddParameter("counterFactualNftInfo.nftOwner", counterFactualNftInfo.nftOwner);
            request.AddParameter("counterFactualNftInfo.nftBaseUri", counterFactualNftInfo.nftBaseUri);
            request.AddParameter("eddsaSignature", eddsaSignature);

            try
            {
                var response = await _client.ExecutePostAsync(request);
                Console.WriteLine("Mint response: " + response.Content);
                var data = JsonConvert.DeserializeObject<MintResponseData>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                Console.WriteLine($"Error minting nft!: {httpException.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
