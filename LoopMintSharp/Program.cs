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


Console.WriteLine("Enter any key to exit");
Console.ReadKey();