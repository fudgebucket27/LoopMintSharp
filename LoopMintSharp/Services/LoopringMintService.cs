using JsonFlatten;
using LoopMintSharp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<StorageId> GetNextStorageId(string apiKey, int accountId, int sellTokenId, bool verboseLogging)
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
                if (verboseLogging)
                {
                    Console.WriteLine($"Error getting storage id: {httpException.Message}");
                }
                return null;
            }
        }

        public async Task<CounterFactualNft> ComputeTokenAddress(string apiKey, CounterFactualNftInfo counterFactualNftInfo, bool verboseLogging)
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
                if (verboseLogging)
                {
                    Console.WriteLine($"Error getting computing token address: {httpException.Message}");
                }
                return null;
            }
        }

        public async Task<OffchainFee> GetOffChainFee(string apiKey, int accountId, int requestType, string tokenAddress, bool verboseLogging)
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

        public async Task<OffchainFee> GetOffChainFeeWithAmount(string apiKey, int accountId, int amount, int requestType, string tokenAddress, bool verboseLogging)
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
            int royaltyPercentage, 
            int storageId, 
            int maxFeeTokenId, 
            string maxFeeAmount, 
            bool forceToMint, 
            CounterFactualNftInfo counterFactualNftInfo,
            string eddsaSignature, 
            bool verboseLogging,
            string royaltyAddress)
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
            request.AddParameter("royaltyPercentage", royaltyPercentage);
            request.AddParameter("storageId", storageId);
            request.AddParameter("maxFee.tokenId", maxFeeTokenId);
            request.AddParameter("maxFee.amount", maxFeeAmount);
            request.AddParameter("forceToMint", "false");
            request.AddParameter("royaltyAddress", royaltyAddress);
            request.AddParameter("counterFactualNftInfo.nftFactory", counterFactualNftInfo.nftFactory);
            request.AddParameter("counterFactualNftInfo.nftOwner", counterFactualNftInfo.nftOwner);
            request.AddParameter("counterFactualNftInfo.nftBaseUri", counterFactualNftInfo.nftBaseUri);
            request.AddParameter("eddsaSignature", eddsaSignature);

            try
            {
                var response = await _client.ExecutePostAsync(request);
                var data = JsonConvert.DeserializeObject<MintResponseData>(response.Content!);
                if(!response.IsSuccessful && verboseLogging)
                {
                    data.errorMessage = response.Content;
                    Console.WriteLine($"Error minting nft: {response.Content}");
                }
                else if(!response.IsSuccessful)
                {
                    data.errorMessage = response.Content;
                }
                return data;
            }
            catch (HttpRequestException httpException)
            {
                var data = new MintResponseData();
                if (verboseLogging)
                {
                    Console.WriteLine($"Error minting nft!: {httpException.Message}");
                }
                data.errorMessage = httpException.Message;
                return null;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<CreateCollectionResult> CreateNftCollection(
            string apiKey, 
            CreateCollectionRequest createCollectionRequest,
            string apiSig,
            bool verboseLogging)
        {
            var request = new RestRequest("api/v3/nft/collection", Method.Post);
            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("x-api-sig", apiSig);
            request.AddHeader("Accept", "application/json");
            var jObject = JObject.Parse(JsonConvert.SerializeObject(createCollectionRequest));
            var jObjectFlattened = jObject.Flatten();
            var jObjectFlattenedString = JsonConvert.SerializeObject(jObjectFlattened);
            request.AddParameter("application/json", jObjectFlattenedString, ParameterType.RequestBody);

            try
            {
                var response = await _client.ExecuteAsync(request);
                var data = JsonConvert.DeserializeObject<CreateCollectionResult>(response.Content!);
                if (!response.IsSuccessful && verboseLogging)
                {
                    Console.WriteLine($"Error creating nft collection: {response.Content}");
                }
                else if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Error creating nft collection: {response.Content}");
                }
                return data;
            }
            catch (HttpRequestException httpException)
            {
                var data = new MintResponseData();
                if (verboseLogging)
                {
                    Console.WriteLine($"Error creating nft collection!: {httpException.Message}");
                }
                data.errorMessage = httpException.Message;
                return null;
            }
        }

        public async Task<CollectionResult> FindNftCollection(string apiKey, int limit, int offset, string owner, string tokenAddress, bool verboseLogging)
        {
            var request = new RestRequest("api/v3/nft/collection");
            request.AddHeader("x-api-key", apiKey);
            request.AddParameter("limit", limit.ToString());
            request.AddParameter("offset", offset.ToString());
            request.AddParameter("owner", owner);
            request.AddParameter("tokenAddress", tokenAddress);
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<CollectionResult>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Error finding collection: {httpException.Message}");
                }
                return null;
            }
        }

        public async Task<string> MintRedPacketNft(string apiKey, string apiSig, RedPacketNft redPacketNft, bool verboseLogging)
        {
            var request = new RestRequest("/api/v3/luckyToken/sendLuckyToken", Method.Post);
            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("x-api-sig", apiSig);
            request.AddHeader("Accept", "application/json");
            var jObject = JObject.Parse(JsonConvert.SerializeObject(redPacketNft));
            var jObjectFlattened = jObject.Flatten();
            var jObjectFlattenedString = JsonConvert.SerializeObject(jObjectFlattened);
            request.AddParameter("application/json", jObjectFlattenedString, ParameterType.RequestBody);
            try
            {
                var response = await _client.ExecuteAsync(request);
                var data = response.Content!;
                if (response.IsSuccessful)
                {
                    Console.WriteLine($"Red packet nft mint response: {data}");
                }
                else if (!response.IsSuccessful && verboseLogging)
                {
                    Console.WriteLine($"Error creating minting red packet nft: {response.Content}");
                }
                else if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Error creating minting red packet nft: {response.Content}");
                }
                return data;
            }
            catch (HttpRequestException httpException)
            {
                var data = "";
                if (verboseLogging)
                {
                    Console.WriteLine($"Error creating minting red packet nft!: {httpException.Message}");
                }
                return null;
            }
        }

        public async Task<NftBalance> GetTokenIdWithCheck(string apiKey, int accountId, string nftData, bool verboseLogging)
        {
            var data = new NftBalance();
            var request = new RestRequest("/api/v3/user/nft/balances");
            request.AddHeader("x-api-key", apiKey);
            request.AddParameter("accountId", accountId);
            request.AddParameter("nftDatas", nftData);
            try
            {
                var response = await _client.GetAsync(request);
                data = JsonConvert.DeserializeObject<NftBalance>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Error getting Token Id: {httpException.Message}");
                }
                return null;
            }
        }
    }
}
