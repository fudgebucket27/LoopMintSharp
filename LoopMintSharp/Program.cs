using LoopMintSharp;
using Newtonsoft.Json;

string apiKey = Environment.GetEnvironmentVariable("LOOPRINGAPIKEY", EnvironmentVariableTarget.Machine);//you can either set an environmental variable or input it here directly.
string loopringPrivateKey = Environment.GetEnvironmentVariable("LOOPRINGPRIVATEKEY", EnvironmentVariableTarget.Machine); //you can either set an environmental variable or input it here directly.
string ethereumPrivateKey = Environment.GetEnvironmentVariable("ETHEREUMPRIVATEKEY", EnvironmentVariableTarget.Machine); //you can either set an environmental variable or input it here directly.

//Console.WriteLine(apiKey);
//Console.WriteLine(loopringPrivateKey);
//Console.WriteLine(ethereumPrivateKey);

ILoopringMintService loopringMintService = new LoopringMintService();
var storageId = await loopringMintService.GetNextStorageId(apiKey, 40940, 0);
Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId, Formatting.Indented)}");

CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
{
    nftOwner = "0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd",
    nftFactory = "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026",
    nftBaseUri = ""
};

var counterFactualNft = await loopringMintService.ComputeTokenAddress(apiKey, counterFactualNftInfo);
Console.WriteLine($"CounterFactualNFT Token Address: {JsonConvert.SerializeObject(counterFactualNft, Formatting.Indented)}");

var offChainFee = await loopringMintService.GetOffChainFee(apiKey, 40940, 9, counterFactualNft.tokenAddress);
Console.WriteLine($"Offchain fee: {JsonConvert.SerializeObject(offChainFee, Formatting.Indented)}");

var nftMintResponse = await loopringMintService.MintNft(
    apiKey,
    "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4",
    40940,
    "0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd",
    40940,
    "0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd",
    0,
    counterFactualNft.tokenAddress,
    "0x1e3d78c9cf6472512434876e36985fcb46f38c98d53e809e4c84e339cf90bce3",
    "1",
    1700000000,
    0,
    storageId.offchainId,
    0,
    offChainFee.fees[0].fee,
    false,
    counterFactualNftInfo
    );
Console.WriteLine($"Nft Mint response: {JsonConvert.SerializeObject(nftMintResponse, Formatting.Indented)}");

Console.WriteLine("Enter any key to exit");
Console.ReadKey();