using JsonFlatten;
using Multiformats.Hash;
using Nethereum.Signer.EIP712;
using Nethereum.Signer;
using Nethereum.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoseidonSharp;
using System.Numerics;
using LoopDropSharp;
using Type = LoopDropSharp.Type;
using LoopMintSharp.Models;

namespace LoopMintSharp
{
    public class Minter
    {
        static ILoopringMintService loopringMintService = new LoopringMintService();

        static HttpClient httpClient = new HttpClient();
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

        public async Task<OffchainFee> GetMintFeeWithAmount(
            string loopringApiKey,
            int accountId,
            string tokenAddress,
            bool verboseLogging
            )
        {
            //Getting the token address
            var mintFee = await loopringMintService.GetOffChainFeeWithAmount(loopringApiKey, accountId, 0, 11, tokenAddress, verboseLogging);
            if (verboseLogging)
            {
                Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(mintFee, Formatting.Indented)}");
            }
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

        public async Task<MintResponseData> MintLegacyCollection(string loopringApiKey,
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
                 string royaltyAddress)
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
            string nftId = "";
            if (currentCid.StartsWith('b'))
            {
                try
                {
                    var cidv1 = currentCid;
                    var apiEndpoint = "http://localhost:5001/api/v0";
                    var remoteFileUrl = $"https://loopring.mypinata.cloud/ipfs/{currentCid}";

                    // Step 1: Download the remote file
                    var response = await httpClient.GetAsync(remoteFileUrl);
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();

                    // Step 2: Convert the raw bytes into a CIDv0 dag-pb file
                    var postFileUrl = $"{apiEndpoint}/dag/put?format=dag-pb&input-enc=raw&hash=sha2-256";
                    var fileContent = new ByteArrayContent(fileBytes);
                    response = await httpClient.PostAsync(postFileUrl, fileContent);
                    var cidv0 = await response.Content.ReadAsStringAsync();

                    // Step 3: Upload the CIDv0 file to IPFS
                    var addUrl = $"{apiEndpoint}/add?pin=true";
                    var fileName = Path.GetFileName(remoteFileUrl);
                    var formData = new MultipartFormDataContent();
                    formData.Add(new ByteArrayContent(fileBytes), "file", fileName);
                    formData.Add(new StringContent(cidv0), "cid-version");
                    response = await httpClient.PostAsync(addUrl, formData);

                    // Step 4: Print out the response
                    var responseString = await response.Content.ReadAsStringAsync();
                    responseString = "[" + responseString.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("}{", "},{") + "]";

                    var responseObject = JsonConvert.DeserializeObject<List<IpfsData>>(responseString);
                    //Console.WriteLine($"[Debug] Converted cidv1: {responseObject[0].Name} to cidv0: {responseObject[0].Hash})");
                    Multihash multiHash = Multihash.Parse(responseObject[0].Hash, Multiformats.Base.MultibaseEncoding.Base58Btc);
                    string multiHashString = multiHash.ToString();
                    var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
                    nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
                }
                catch(Exception ex)
                {
                    if (verboseLogging)
                    {
                        Console.WriteLine($"{ex.Message}");
                    }
                    return new MintResponseData()
                    {
                        metadataCid = currentCid,
                        errorMessage = $"{ex.Message}",
                        status = "Mint failed"
                    };
                }
            }
            else if(currentCid.StartsWith("Qm"))
            {
                Multihash multiHash = Multihash.Parse(currentCid, Multiformats.Base.MultibaseEncoding.Base58Btc);
                string multiHashString = multiHash.ToString();
                var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
                nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
            }
            else
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Not a valid CID, must begin with b or Qm");
                }
                return new MintResponseData()
                {
                    metadataCid = currentCid,
                    errorMessage = "Not a valid CID, must begin with b or Qm",
                    status = "Mint failed"
                };
            }

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
                verboseLogging: verboseLogging,
                royaltyAddress: royaltyAddress
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
                       string tokenAddress,
                       string royaltyAddress)
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
            string nftId = "";
            if (currentCid.StartsWith('b'))
            {
                try
                {
                    var cidv1 = currentCid;
                    var apiEndpoint = "http://localhost:5001/api/v0";
                    var remoteFileUrl = $"https://loopring.mypinata.cloud/ipfs/{currentCid}";

                    // Step 1: Download the remote file
                    var response = await httpClient.GetAsync(remoteFileUrl);
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();

                    // Step 2: Convert the raw bytes into a CIDv0 dag-pb file
                    var postFileUrl = $"{apiEndpoint}/dag/put?format=dag-pb&input-enc=raw&hash=sha2-256";
                    var fileContent = new ByteArrayContent(fileBytes);
                    response = await httpClient.PostAsync(postFileUrl, fileContent);
                    var cidv0 = await response.Content.ReadAsStringAsync();

                    // Step 3: Upload the CIDv0 file to IPFS
                    var addUrl = $"{apiEndpoint}/add?pin=true";
                    var fileName = Path.GetFileName(remoteFileUrl);
                    var formData = new MultipartFormDataContent();
                    formData.Add(new ByteArrayContent(fileBytes), "file", fileName);
                    formData.Add(new StringContent(cidv0), "cid-version");
                    response = await httpClient.PostAsync(addUrl, formData);

                    // Step 4: Print out the response
                    var responseString = await response.Content.ReadAsStringAsync();
                    responseString = "[" + responseString.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("}{", "},{") + "]";

                    var responseObject = JsonConvert.DeserializeObject<List<IpfsData>>(responseString);
                    //Console.WriteLine($"[Debug] Converted cidv1: {responseObject[0].Name} to cidv0: {responseObject[0].Hash})");
                    Multihash multiHash = Multihash.Parse(responseObject[0].Hash, Multiformats.Base.MultibaseEncoding.Base58Btc);
                    string multiHashString = multiHash.ToString();
                    var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
                    nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
                }
                catch (Exception ex)
                {
                    if(verboseLogging)
                    {
                        Console.WriteLine($"Can't connect to ipfs. Ipfs desktop must be installed and running on http, port 5001!");
                    }
                    return new MintResponseData()
                    {
                        metadataCid = currentCid,
                        errorMessage = "Can't connect to ipfs. Ipfs desktop must be installed and running on http, port 5001!",
                        status = "Mint failed"
                    };
                }
            }
            else if (currentCid.StartsWith("Qm"))
            {
                Multihash multiHash = Multihash.Parse(currentCid, Multiformats.Base.MultibaseEncoding.Base58Btc);
                string multiHashString = multiHash.ToString();
                var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
                nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
            }
            else
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Not a valid CID, must begin with b or Qm");
                }
                return new MintResponseData()
                {
                    metadataCid = currentCid,
                    errorMessage = "Not a valid CID, must begin with b or Qm",
                    status = "Mint failed"
                };
            }
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
                verboseLogging: verboseLogging,
                royaltyAddress: royaltyAddress
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

        public async Task<NftBalance> GetTokenIdWithCheck(string loopringApiKey, int loopringAccountId, string nftData, bool verboseLogging)
        {
            return await loopringMintService.GetTokenIdWithCheck(loopringApiKey, loopringAccountId, nftData, verboseLogging);
        }

        public async Task<RedPacketNftMintResponse> MintRedPacketNft(
                    string loopringApiKey,
                 string loopringPrivateKey,
                 string layer1PrivateKey,
                 string? minterAddress,
                 int accountId,
                 NftBalance nftBalance,
                 double validUntilDays,
                 int maxFeeTokenId,
                 string? exchange,
                 string amountOfNftsPerPacket,
                 string amountOfPackets,
                 bool isRandomSplit,
                 string memo,
                 bool verboseLogging)
        {
            var dateTimeNow = DateTimeOffset.UtcNow;
            long validSince = dateTimeNow.ToUnixTimeSeconds();
            var validUntil30Days = dateTimeNow.AddDays(30).ToUnixTimeSeconds();
            var validUntil = dateTimeNow.AddDays(validUntilDays).ToUnixTimeSeconds();
            var offchainFee = await loopringMintService.GetOffChainFeeWithAmount(loopringApiKey, accountId, 0, 11, nftBalance.data[0].tokenAddress, verboseLogging);
            var storageId = await loopringMintService.GetNextStorageId(loopringApiKey, accountId, nftBalance.data[0].tokenId, verboseLogging);

            //Calculate eddsa signautre
            BigInteger[] poseidonInputs =
    {
                                    Utils.ParseHexUnsigned(exchange),
                                    (BigInteger) accountId,
                                    (BigInteger) 0,
                                    (BigInteger) nftBalance.data[0].tokenId,
                                    BigInteger.Parse(amountOfNftsPerPacket) * BigInteger.Parse(amountOfPackets),
                                    (BigInteger) maxFeeTokenId,
                                    BigInteger.Parse(offchainFee.fees[maxFeeTokenId].fee),
                                    Utils.ParseHexUnsigned("0x9cde4366824d9410fb2e2f885601933a926f40d7"),
                                    (BigInteger) 0,
                                    (BigInteger) 0,
                                    (BigInteger) validUntil30Days,
                                    (BigInteger) storageId.offchainId
                    };
            Poseidon poseidon = new Poseidon(13, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(poseidonInputs);
            Eddsa eddsa = new Eddsa(poseidonHash, loopringPrivateKey);
            string eddsaSignature = eddsa.Sign();


            //Calculate ecdsa
            string primaryTypeName = "Transfer";
            TypedData eip712TypedData = new TypedData();
            eip712TypedData.Domain = new Domain()
            {
                Name = "Loopring Protocol",
                Version = "3.6.0",
                ChainId = 1,
                VerifyingContract = "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4",
            };
            eip712TypedData.PrimaryType = primaryTypeName;
            eip712TypedData.Types = new Dictionary<string, MemberDescription[]>()
            {
                ["EIP712Domain"] = new[]
                    {
                                            new MemberDescription {Name = "name", Type = "string"},
                                            new MemberDescription {Name = "version", Type = "string"},
                                            new MemberDescription {Name = "chainId", Type = "uint256"},
                                            new MemberDescription {Name = "verifyingContract", Type = "address"},
                                        },
                [primaryTypeName] = new[]
                    {
                                            new MemberDescription {Name = "from", Type = "address"},            // payerAddr
                                            new MemberDescription {Name = "to", Type = "address"},              // toAddr
                                            new MemberDescription {Name = "tokenID", Type = "uint16"},          // token.tokenId 
                                            new MemberDescription {Name = "amount", Type = "uint96"},           // token.volume 
                                            new MemberDescription {Name = "feeTokenID", Type = "uint16"},       // maxFee.tokenId
                                            new MemberDescription {Name = "maxFee", Type = "uint96"},           // maxFee.volume
                                            new MemberDescription {Name = "validUntil", Type = "uint32"},       // validUntill
                                            new MemberDescription {Name = "storageID", Type = "uint32"}         // storageId
                                        },

            };
            eip712TypedData.Message = new[]
            {
                                    new MemberValue {TypeName = "address", Value = minterAddress},
                                    new MemberValue {TypeName = "address", Value = "0x9cde4366824d9410fb2e2f885601933a926f40d7"},
                                    new MemberValue {TypeName = "uint16", Value = nftBalance.data[0].tokenId},
                                    new MemberValue {TypeName = "uint96", Value = BigInteger.Parse(amountOfNftsPerPacket) * BigInteger.Parse(amountOfPackets)},
                                    new MemberValue {TypeName = "uint16", Value = maxFeeTokenId},
                                    new MemberValue {TypeName = "uint96", Value = BigInteger.Parse(offchainFee.fees[maxFeeTokenId].fee)},
                                    new MemberValue {TypeName = "uint32", Value = validUntil30Days},
                                    new MemberValue {TypeName = "uint32", Value = storageId.offchainId},
                                };

            TransferTypedData typedData = new TransferTypedData()
            {
                domain = new TransferTypedData.Domain()
                {
                    name = "Loopring Protocol",
                    version = "3.6.0",
                    chainId = 1,
                    verifyingContract = "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4",
                },
                message = new TransferTypedData.Message()
                {
                    from = minterAddress,
                    to = "0x9cde4366824d9410fb2e2f885601933a926f40d7",
                    tokenID = nftBalance.data[0].tokenId,
                    amount = (BigInteger.Parse(amountOfNftsPerPacket) * BigInteger.Parse(amountOfPackets)).ToString(),
                    feeTokenID = maxFeeTokenId,
                    maxFee = offchainFee.fees[maxFeeTokenId].fee,
                    validUntil = (int)validUntil30Days,
                    storageID = storageId.offchainId
                },
                primaryType = primaryTypeName,
                types = new TransferTypedData.Types()
                {
                    EIP712Domain = new List<Type>()
                                        {
                                            new Type(){ name = "name", type = "string"},
                                            new Type(){ name="version", type = "string"},
                                            new Type(){ name="chainId", type = "uint256"},
                                            new Type(){ name="verifyingContract", type = "address"},
                                        },
                    Transfer = new List<Type>()
                                        {
                                            new Type(){ name = "from", type = "address"},
                                            new Type(){ name = "to", type = "address"},
                                            new Type(){ name = "tokenID", type = "uint16"},
                                            new Type(){ name = "amount", type = "uint96"},
                                            new Type(){ name = "feeTokenID", type = "uint16"},
                                            new Type(){ name = "maxFee", type = "uint96"},
                                            new Type(){ name = "validUntil", type = "uint32"},
                                            new Type(){ name = "storageID", type = "uint32"},
                                        }
                }
            };

            Eip712TypedDataSigner signer = new Eip712TypedDataSigner();
            var ethECKey = new Nethereum.Signer.EthECKey(layer1PrivateKey.Replace("0x", ""));
            var encodedTypedData = signer.EncodeTypedData(eip712TypedData);
            var ECDRSASignature = ethECKey.SignAndCalculateV(Sha3Keccack.Current.CalculateHash(encodedTypedData));
            var serializedECDRSASignature = EthECDSASignature.CreateStringSignature(ECDRSASignature);
            var ecdsaSignature = serializedECDRSASignature + "0" + (int)2;
            RedPacketNft redPacketNft = new RedPacketNft();
            redPacketNft.ecdsaSignature = ecdsaSignature;
            LuckyToken luckyToken = new LuckyToken();
            luckyToken.exchange = exchange;
            luckyToken.payerAddr = minterAddress;
            luckyToken.payerId = accountId;
            luckyToken.payeeAddr = "0x9cde4366824d9410fb2e2f885601933a926f40d7";
            luckyToken.storageId = storageId.offchainId;
            luckyToken.token = nftBalance.data[0].tokenId;
            luckyToken.amount = (int.Parse(amountOfNftsPerPacket) * int.Parse(amountOfPackets)).ToString();
            luckyToken.feeToken = maxFeeTokenId;
            luckyToken.maxFeeAmount = offchainFee.fees[maxFeeTokenId].fee;
            luckyToken.validUntil = validUntil30Days;
            luckyToken.payeeId = 0;
            luckyToken.memo = $"LuckTokenSendBy{accountId}";
            luckyToken.eddsaSig = eddsaSignature;
            redPacketNft.luckyToken = luckyToken;
            redPacketNft.memo = memo;
            redPacketNft.nftData = nftBalance.data[0].nftData;
            redPacketNft.numbers = amountOfPackets;
            redPacketNft.signerFlag = false;
            redPacketNft.templateId = 0;
            redPacketNft.type = new Models.Type()
            {
                partition = isRandomSplit == true ? 0 : 1,
                mode = 1,
                scope = 1
            };
            redPacketNft.validSince = validSince;
            redPacketNft.validUntil = validUntil;
            var mintResponse = await loopringMintService.MintRedPacketNft(loopringApiKey, ecdsaSignature, redPacketNft, verboseLogging);
            if (mintResponse.hash != null)
            {
                if (verboseLogging)
                {
                    Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(mintResponse, Formatting.Indented)}");
                }
                mintResponse.status = "Minted successfully";
            }
            else
            {
                mintResponse.status = "Mint failed";
            }
            return mintResponse;
        }
    }
}
