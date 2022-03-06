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
Console.WriteLine($"Storage id: {JsonConvert.SerializeObject(storageId)}");

CounterFactualNftInfo counterFactualNftInfo = new CounterFactualNftInfo
{
    nftOwner = "0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd",
    nftFactory = "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026",
    nftBaseUri = ""
};

Helpers.ComputerNftTokenAddress(counterFactualNftInfo);


Console.WriteLine("Enter any key to exit");
Console.ReadKey();