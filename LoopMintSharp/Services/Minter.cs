using JsonFlatten;
using Multiformats.Hash;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoseidonSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LoopMintSharp
{
    public class Minter
    {
        static ILoopringMintService loopringMintService = new LoopringMintService();
        public async Task<OffchainFee> GetMintFee(
            string loopringApiKey,
            int accountId,
            string minterAddress,
            string nftFactory,
            bool verboseLogging
            )
        {
            //Getting the token address
            CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
            {
                nftOwner = minterAddress,
                nftFactory = nftFactory,
                nftBaseUri = "" //this aint used in the api as far as i can tell. for future use
            };
            var counterFactualNft = await loopringMintService.ComputeTokenAddress(loopringApiKey, counterFactualNftInfo, verboseLogging);
            var mintFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, counterFactualNft.tokenAddress, verboseLogging);
            return mintFee;
        }

        public async Task<OffchainFee> GetMintFee(
    string loopringApiKey,
    int accountId,
    string minterAddress,
    string nftFactory,
    bool verboseLogging,
    string baseUri
    )
        {
            //Getting the token address
            CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
            {
                nftOwner = minterAddress,
                nftFactory = nftFactory,
                nftBaseUri = baseUri
            };
            var counterFactualNft = await loopringMintService.ComputeTokenAddress(loopringApiKey, counterFactualNftInfo, verboseLogging);
            var mintFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, counterFactualNft.tokenAddress, verboseLogging);
            return mintFee;
        }

        public async Task<CreateCollectionResult> CreateNftCollection(
            string apiKey,
            string avatar,
            string banner,
            string description,
            string name,
            string nftFactory,
            string owner,
            string tileUri,
            string loopringPrivateKey,
            bool verboseLogging)
        {
            CreateCollectionRequest createCollectionRequest = new CreateCollectionRequest()
            {
                avatar = avatar,
                banner = banner,
                description = description,
                name = name,
                nftFactory = nftFactory,
                owner = owner,
                tileUri = tileUri
            };
            Dictionary<string, object> dataToSig = new Dictionary<string, object>();
            dataToSig.Add("avatar", avatar);
            dataToSig.Add("banner", banner);
            dataToSig.Add("description", description);
            dataToSig.Add("name", name);
            dataToSig.Add("nftFactory", nftFactory);
            dataToSig.Add("owner", owner);
            dataToSig.Add("tileUri", tileUri);
            var signatureBase = "POST&";
            var jObject = JObject.Parse(JsonConvert.SerializeObject(dataToSig));
            var jObjectFlattened = jObject.Flatten();
            var parameterString = JsonConvert.SerializeObject(jObjectFlattened);
            signatureBase += Utils.UrlEncodeUpperCase("https://api3.loopring.io/api/v3/nft/collection") + "&";
            signatureBase += Uri.EscapeDataString(parameterString);
            var sha256Number = SHA256Helper.CalculateSHA256HashNumber(signatureBase);
            var sha256Signer = new Eddsa(sha256Number, loopringPrivateKey);
            var sha256Signed = sha256Signer.Sign();
            var collectionResult = await loopringMintService.CreateNftCollection(apiKey, createCollectionRequest, sha256Signed, verboseLogging);
            if (!string.IsNullOrEmpty(collectionResult.contractAddress))
            {
                Console.WriteLine($"Collection with Name:{name}, created with Collection Contract Address: {collectionResult.contractAddress}");
            }
            return collectionResult;
        }

        public async Task<CollectionResult> FindNftCollection(string loopringApiKey, int limit, int offset, string owner, string contractAddress, bool verboseLogging)
        {
            return await loopringMintService.FindNftCollection(loopringApiKey, limit, offset, owner, contractAddress, verboseLogging);
        }

        public async Task<MintResponseData> Mint(string loopringApiKey,
                        string loopringPrivateKey,
                        string? minterAddress,
                        int accountId,
                        int nftType,
                        int nftRoyaltyPercentage,
                        int nftAmount,
                        long validUntil,
                        int maxFeeTokenId,
                        string? nftFactory,
                        string? exchange,
                        string currentCid,
                        bool verboseLogging)
        {
            #region Get storage id, token address and offchain fee
            //Getting the storage id
            var storageId = await loopringMintService.GetNextStorageId(loopringApiKey, accountId, maxFeeTokenId, verboseLogging);
            if (verboseLogging)
            {
                Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");
            }

            //Getting the token address
            CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
            {
                nftOwner = minterAddress,
                nftFactory = nftFactory,
                nftBaseUri = ""
            };
            var counterFactualNft = await loopringMintService.ComputeTokenAddress(loopringApiKey, counterFactualNftInfo, verboseLogging);
            if (verboseLogging)
            {
                Console.WriteLine($"CounterFactualNFT Token Address: {JsonConvert.SerializeObject(counterFactualNft, Formatting.Indented)}");
            }

            //Getting the offchain fee
            var offChainFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, counterFactualNft.tokenAddress, verboseLogging);
            if (verboseLogging)
            {
                Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(offChainFee, Formatting.Indented)}");
            }
            #endregion

            #region Generate Eddsa Signature

            //Generate the nft id here
            Multihash multiHash = Multihash.Parse(currentCid, Multiformats.Base.MultibaseEncoding.Base58Btc);
            string multiHashString = multiHash.ToString();
            var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
            var nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
            if (verboseLogging)
            {
                Console.WriteLine($"Generated NFT ID: {nftId}");
            }

            //Generate the poseidon hash for the nft data
            var nftIdHi = Utils.ParseHexUnsigned(nftId.Substring(0, 34));
            var nftIdLo = Utils.ParseHexUnsigned(nftId.Substring(34, 32));
            BigInteger[] nftDataPoseidonInputs =
            {
                Utils.ParseHexUnsigned(minterAddress),
                (BigInteger) 0,
                Utils.ParseHexUnsigned(counterFactualNft.tokenAddress),
                nftIdLo,
                nftIdHi,
                (BigInteger)nftRoyaltyPercentage
            };
            Poseidon nftDataPoseidon = new Poseidon(7, 6, 52, "poseidon", 5, _securityTarget: 128);
            BigInteger nftDataPoseidonHash = nftDataPoseidon.CalculatePoseidonHash(nftDataPoseidonInputs);

            //Generate the poseidon hash for the remaining data
            BigInteger[] nftPoseidonInputs =
            {
                Utils.ParseHexUnsigned(exchange),
                (BigInteger) accountId,
                (BigInteger) accountId,
                nftDataPoseidonHash,
                (BigInteger) nftAmount,
                (BigInteger) maxFeeTokenId,
                BigInteger.Parse(offChainFee.fees[maxFeeTokenId].fee),
                (BigInteger) validUntil,
                (BigInteger) storageId.offchainId
            };
            Poseidon nftPoseidon = new Poseidon(10, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger nftPoseidonHash = nftPoseidon.CalculatePoseidonHash(nftPoseidonInputs);

            //Generate the poseidon eddsa signature
            Eddsa eddsa = new Eddsa(nftPoseidonHash, loopringPrivateKey);
            string eddsaSignature = eddsa.Sign();
            #endregion

            #region Submit the nft mint
            var nftMintResponse = await loopringMintService.MintNft(
                apiKey: loopringApiKey,
                exchange: exchange,
                minterId: accountId,
                minterAddress: minterAddress,
                toAccountId: accountId,
                toAddress: minterAddress,
                nftType: nftType,
                tokenAddress: counterFactualNft.tokenAddress,
                nftId,
                amount: nftAmount.ToString(),
                validUntil: validUntil,
                royaltyPercentage: nftRoyaltyPercentage,
                storageId.offchainId,
                maxFeeTokenId: maxFeeTokenId,
                maxFeeAmount: offChainFee.fees[maxFeeTokenId].fee,
                forceToMint: false,
                counterFactualNftInfo: counterFactualNftInfo,
                eddsaSignature: eddsaSignature,
                verboseLogging: verboseLogging
                );
            nftMintResponse.metadataCid = currentCid;
            nftMintResponse.nftId = nftId;
            if (nftMintResponse.hash != null)
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(nftMintResponse, Formatting.Indented)}");
                }
                nftMintResponse.status = "Minted successfully";
            }
            else
            {
                nftMintResponse.status = "Mint failed";
            }
            return nftMintResponse;
            #endregion
        }

        public async Task<MintResponseData> MintCollection(string loopringApiKey,
                       string loopringPrivateKey,
                       string? minterAddress,
                       int accountId,
                       int nftType,
                       int nftRoyaltyPercentage,
                       int nftAmount,
                       long validUntil,
                       int maxFeeTokenId,
                       string? nftFactory,
                       string? exchange,
                       string currentCid,
                       bool verboseLogging,
                       string baseUri,
                       string tokenAddress)
        {
            #region Get storage id, token address and offchain fee
            //Getting the storage id
            var storageId = await loopringMintService.GetNextStorageId(loopringApiKey, accountId, maxFeeTokenId, verboseLogging);
            if (verboseLogging)
            {
                Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");
            }

            //Getting the token address
            CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
            {
                nftOwner = minterAddress,
                nftFactory = nftFactory,
                nftBaseUri = baseUri
            };
            //Getting the offchain fee
            var offChainFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, tokenAddress, verboseLogging);
            if (verboseLogging)
            {
                Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(offChainFee, Formatting.Indented)}");
            }
            #endregion

            #region Generate Eddsa Signature

            //Generate the nft id here
            Multihash multiHash = Multihash.Parse(currentCid, Multiformats.Base.MultibaseEncoding.Base58Btc);
            string multiHashString = multiHash.ToString();
            var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
            var nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
            if (verboseLogging)
            {
                Console.WriteLine($"Generated NFT ID: {nftId}");
            }

            //Generate the poseidon hash for the nft data
            var nftIdHi = Utils.ParseHexUnsigned(nftId.Substring(0, 34));
            var nftIdLo = Utils.ParseHexUnsigned(nftId.Substring(34, 32));
            BigInteger[] nftDataPoseidonInputs =
            {
                Utils.ParseHexUnsigned(minterAddress),
                (BigInteger) 0,
                Utils.ParseHexUnsigned(tokenAddress),
                nftIdLo,
                nftIdHi,
                (BigInteger)nftRoyaltyPercentage
            };
            Poseidon nftDataPoseidon = new Poseidon(7, 6, 52, "poseidon", 5, _securityTarget: 128);
            BigInteger nftDataPoseidonHash = nftDataPoseidon.CalculatePoseidonHash(nftDataPoseidonInputs);

            //Generate the poseidon hash for the remaining data
            BigInteger[] nftPoseidonInputs =
            {
                Utils.ParseHexUnsigned(exchange),
                (BigInteger) accountId,
                (BigInteger) accountId,
                nftDataPoseidonHash,
                (BigInteger) nftAmount,
                (BigInteger) maxFeeTokenId,
                BigInteger.Parse(offChainFee.fees[maxFeeTokenId].fee),
                (BigInteger) validUntil,
                (BigInteger) storageId.offchainId
            };
            Poseidon nftPoseidon = new Poseidon(10, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger nftPoseidonHash = nftPoseidon.CalculatePoseidonHash(nftPoseidonInputs);

            //Generate the poseidon eddsa signature
            Eddsa eddsa = new Eddsa(nftPoseidonHash, loopringPrivateKey);
            string eddsaSignature = eddsa.Sign();
            #endregion

            #region Submit the nft mint
            var nftMintResponse = await loopringMintService.MintNft(
                apiKey: loopringApiKey,
                exchange: exchange,
                minterId: accountId,
                minterAddress: minterAddress,
                toAccountId: accountId,
                toAddress: minterAddress,
                nftType: nftType,
                tokenAddress: tokenAddress,
                nftId,
                amount: nftAmount.ToString(),
                validUntil: validUntil,
                royaltyPercentage: nftRoyaltyPercentage,
                storageId.offchainId,
                maxFeeTokenId: maxFeeTokenId,
                maxFeeAmount: offChainFee.fees[maxFeeTokenId].fee,
                forceToMint: false,
                counterFactualNftInfo: counterFactualNftInfo,
                eddsaSignature: eddsaSignature,
                verboseLogging: verboseLogging
                );
            nftMintResponse.metadataCid = currentCid;
            nftMintResponse.nftId = nftId;
            if (nftMintResponse.hash != null)
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(nftMintResponse, Formatting.Indented)}");
                }
                nftMintResponse.status = "Minted successfully";
            }
            else
            {
                nftMintResponse.status = "Mint failed";
            }
            return nftMintResponse;
            #endregion
        }
    }
}
