string apiKey = Environment.GetEnvironmentVariable("LOOPRINGAPIKEY", EnvironmentVariableTarget.Machine);//you can either set an environmental variable or input it here directly.
string loopringPrivateKey = Environment.GetEnvironmentVariable("LOOPRINGPRIVATEKEY", EnvironmentVariableTarget.Machine); //you can either set an environmental variable or input it here directly.
string ethereumPrivateKey = Environment.GetEnvironmentVariable("ETHEREUMPRIVATEKEY", EnvironmentVariableTarget.Machine); //you can either set an environmental variable or input it here directly.

Console.WriteLine(apiKey);
Console.WriteLine(loopringPrivateKey);
Console.WriteLine(ethereumPrivateKey);


Console.WriteLine("Enter any key to exit");
Console.ReadKey();