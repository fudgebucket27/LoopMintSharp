using Multiformats.Hash;
using Newtonsoft.Json;
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
            ILoopringMintService loopringMintService = new LoopringMintService();
            var storageId = await loopringMintService.GetNextStorageId(loopringApiKey, accountId, maxFeeTokenId, verboseLogging);
            if(verboseLogging)
            {
                Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");
            }

            //Getting the token address
            CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
            {
                nftOwner = minterAddress,
                nftFactory = nftFactory,
                nftBaseUri = "" //this aint used in the api as far as i can tell. for future use
            };
            var counterFactualNft = await loopringMintService.ComputeTokenAddress(loopringApiKey, counterFactualNftInfo, verboseLogging);
            if(verboseLogging)
            {
                Console.WriteLine($"CounterFactualNFT Token Address: {JsonConvert.SerializeObject(counterFactualNft, Formatting.Indented)}");
            }

            //Getting the offchain fee
            var offChainFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, counterFactualNft.tokenAddress, verboseLogging);
            if(verboseLogging)
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
            if(verboseLogging)
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
                if(verboseLogging)
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
