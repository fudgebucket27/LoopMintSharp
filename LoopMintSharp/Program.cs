using LoopMintSharp;
using Newtonsoft.Json;
using PoseidonSharp;
using System.Numerics;
using System.Text;
using Multiformats.Hash;
using Microsoft.Extensions.Configuration;
using CsvHelper;
using System.Globalization;

#region Initial Setup
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

//Changes these variables to suit
var ipfsCid = args[0]; //command line argument, can be the ipfs cid of your metadata.json or a .txt file containg your all of your ipfs cids on each line
string loopringApiKey = settings.LoopringApiKey;//you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
string loopringPrivateKey = settings.LoopringPrivateKey; //you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
var minterAddress = settings.LoopringAddress; //your loopring address
var accountId = settings.LoopringAccountId; //your loopring account id
var nftType = settings.NftType; //nfttype 0 = ERC1155, shouldn't need to change this unless you want ERC721 which is 1
var nftRoyaltyPercentage = settings.NftRoyaltyPercentage; //i wonder what setting this to something other than 0 would do?
var nftAmount = settings.NftAmount; //leave this to one so you only mint 1
var validUntil = settings.ValidUntil; //the examples seem to use this number
var maxFeeTokenId = settings.MaxFeeTokenId; //0 should be for ETH, 1 is for LRC?
var nftFactory = settings.NftFactory; //current nft factory of loopring, shouldn't need to change unless they deploye a new contract again, sigh...
var exchange = settings.Exchange; //loopring exchange address, shouldn't need to change this,
#endregion


#region Single Mint
if (!ipfsCid.Contains(".txt") && ipfsCid.StartsWith("Qm")) //Single Mint
{
    #region Get storage id, token address and offchain fee
    ILoopringMintService loopringMintService = new LoopringMintService();
    //Getting the storage id
    var storageId = await loopringMintService.GetNextStorageId(loopringApiKey, accountId, maxFeeTokenId);
    Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");

    //Getting the token address
    CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
    {
        nftOwner = minterAddress,
        nftFactory = nftFactory,
        nftBaseUri = "" //this aint used in the api as far as i can tell. for future use
    };
    var counterFactualNft = await loopringMintService.ComputeTokenAddress(loopringApiKey, counterFactualNftInfo);
    Console.WriteLine($"CounterFactualNFT Token Address: {JsonConvert.SerializeObject(counterFactualNft, Formatting.Indented)}");

    //Getting the offchain fee
    var offChainFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, counterFactualNft.tokenAddress);
    Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(offChainFee, Formatting.Indented)}");
    #endregion

    #region Generate Eddsa Signature

    //Generate the nft id here
    Multihash multiHash = Multihash.Parse(ipfsCid, Multiformats.Base.MultibaseEncoding.Base58Btc);
    string multiHashString = multiHash.ToString();
    var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
    var nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
    Console.WriteLine($"Generated NFT ID: {nftId}");

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
        eddsaSignature: eddsaSignature
        );
    if (nftMintResponse.hash != null)
    {
        Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(nftMintResponse, Formatting.Indented)}");
    }
    #endregion
}
#endregion
#region Batch Mint
else //Batch mint from CSV
{
    List<MintResponse> mintResponses = new List<MintResponse>();
    using (StreamReader sr = new StreamReader(ipfsCid))
    {
        string currentCid;
        // currentLine will be null when the StreamReader reaches the end of file
        while ((currentCid = sr.ReadLine()) != null)
        {
            #region Get storage id, token address and offchain fee
            //Getting the storage id
            ILoopringMintService loopringMintService = new LoopringMintService();
            var storageId = await loopringMintService.GetNextStorageId(loopringApiKey, accountId, maxFeeTokenId);
            Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");

            //Getting the token address
            CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
            {
                nftOwner = minterAddress,
                nftFactory = nftFactory,
                nftBaseUri = "" //this aint used in the api as far as i can tell. for future use
            };
            var counterFactualNft = await loopringMintService.ComputeTokenAddress(loopringApiKey, counterFactualNftInfo);
            Console.WriteLine($"CounterFactualNFT Token Address: {JsonConvert.SerializeObject(counterFactualNft, Formatting.Indented)}");

            //Getting the offchain fee
            var offChainFee = await loopringMintService.GetOffChainFee(loopringApiKey, accountId, 9, counterFactualNft.tokenAddress);
            Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(offChainFee, Formatting.Indented)}");
            #endregion

            #region Generate Eddsa Signature

            //Generate the nft id here
            Multihash multiHash = Multihash.Parse(currentCid, Multiformats.Base.MultibaseEncoding.Base58Btc);
            string multiHashString = multiHash.ToString();
            var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
            var nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
            Console.WriteLine($"Generated NFT ID: {nftId}");

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
                eddsaSignature: eddsaSignature
                );
            MintResponse mintResponse = new MintResponse();
            mintResponse.metadataCid = currentCid;
            if (nftMintResponse.hash != null)
            {
                Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(nftMintResponse, Formatting.Indented)}");
                mintResponse.status = "Minted successfully";
            }
            else
            {
                mintResponse.status = "Mint failed";
            }
            mintResponses.Add(mintResponse);
            #endregion
        }
    }

    #region Create csv report
    string csvName = $"{DateTime.Now.ToString("yyyy-mm-dd hh-mm-ss")}.csv";
    using (var writer = new StreamWriter(csvName))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(mintResponses);
        Console.WriteLine($"Generated Mint Report");
        Console.WriteLine($"CSV can be found in the following location: {AppDomain.CurrentDomain.BaseDirectory + csvName}");
    }
    #endregion
}
#endregion