using LoopMintSharp;
using Newtonsoft.Json;
using PoseidonSharp;
using System.Numerics;
using System.Text;
using Multiformats.Hash;

#region Initial Setup
//Changes these variables to suit
string apiKey = Environment.GetEnvironmentVariable("LOOPRINGAPIKEY", EnvironmentVariableTarget.Machine);//you can either set an environmental variable or input it here directly. You can export this account using loopring.io
string loopringPrivateKey = Environment.GetEnvironmentVariable("LOOPRINGPRIVATEKEY", EnvironmentVariableTarget.Machine); //you can either set an environmental variable or input it here directly. You can export this account using loopring.io
var ipfsCid = "QmNhSqvvzQDy4GW8MUVH8hcDJPzHh22WSrW6Eu6DTUCmja"; //the ipfs cid of your metadata.json
var exchange = "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4"; //shouldn't need to change this
var minterAddress = "0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd"; //your address
var accountId = 40940; //your account id
var nftType = 0; //nfttype 0 = ERC1155, shouldn't need to change this unless you want ERC721 which is 1
var creatorFeeBips = 0; //i wonder what setting to something other than 0 would do?
var amount = 1; //leave this to one so you only mint 1
var validUntil = 1700000000; //the examples seem to use this number
#endregion

#region Get storeage id, token address and offchain fee
ILoopringMintService loopringMintService = new LoopringMintService();

//Getting the storage id
var storageId = await loopringMintService.GetNextStorageId(apiKey, accountId, 0);
Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");

//Getting the token address
CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
{
    nftOwner = minterAddress,
    nftFactory = "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026",
    nftBaseUri = ""
};
var counterFactualNft = await loopringMintService.ComputeTokenAddress(apiKey, counterFactualNftInfo);
Console.WriteLine($"CounterFactualNFT Token Address: {JsonConvert.SerializeObject(counterFactualNft, Formatting.Indented)}");

//Getting the offchain fee
var offChainFee = await loopringMintService.GetOffChainFee(apiKey, 40940, 9, counterFactualNft.tokenAddress);
Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(offChainFee, Formatting.Indented)}");
#endregion

#region Generate Eddsa Signature

//Generate the nft id here
Multihash multiHash = Multihash.Parse(ipfsCid);
string multiHashString = multiHash.ToString();
var ipfsCidBigInteger = Utils.ParseHexUnsigned(multiHashString);
var nftId = "0x" + ipfsCidBigInteger.ToString("x").Substring(4);
Console.WriteLine($"Generated NFT ID: {nftId}");

//Generate the poseidon hash for the nft data
var nftIdHi = Utils.ParseHexUnsigned(nftId.Substring(0,34));
var nftIdLo = Utils.ParseHexUnsigned(nftId.Substring(34, 32));
BigInteger[] nftDataPoseidonInputs = 
{
    Utils.ParseHexUnsigned(minterAddress),
    (BigInteger) 0,
    Utils.ParseHexUnsigned(counterFactualNft.tokenAddress),
    nftIdLo,
    nftIdHi,
    (BigInteger)creatorFeeBips
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
    (BigInteger) amount,
    (BigInteger) 0,
    BigInteger.Parse(offChainFee.fees[0].fee),
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
    apiKey: apiKey,
    exchange: exchange,
    minterId: accountId,
    minterAddress: minterAddress,
    toAccountId: accountId,
    toAddress: minterAddress,
    nftType: 0,
    tokenAddress: counterFactualNft.tokenAddress,
    nftId,
    amount: amount.ToString(),
    validUntil: validUntil,
    creatorFeeBips: creatorFeeBips,
    storageId.offchainId,
    maxFeeTokenId: 0,
    maxFeeAmount: offChainFee.fees[0].fee,
    forceToMint: false,
    counterFactualNftInfo: counterFactualNftInfo,
    eddsaSignature: eddsaSignature
    );
if(nftMintResponse.hash != null)
{
    Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(nftMintResponse, Formatting.Indented)}");
}
#endregion
Console.WriteLine("Enter any key to exit");
Console.ReadKey();