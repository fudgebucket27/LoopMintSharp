using LoopMintSharp;
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
var ipfsCid = "C:\\Temp\\cids_test.txt"; //command line argument, can be the ipfs cid of your metadata.json or a .txt file containg your all of your ipfs cids on each line
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

Minter minter = new Minter();
#region Single Mint
if (!ipfsCid.Contains(".txt") && ipfsCid.StartsWith("Qm")) //Single Mint
{
    var mintResponse = await minter.Mint(loopringApiKey, loopringPrivateKey, minterAddress, accountId, nftType, nftRoyaltyPercentage, nftAmount, validUntil, maxFeeTokenId, nftFactory, exchange, ipfsCid);
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
            var mintResponse = await minter.Mint(loopringApiKey, loopringPrivateKey, minterAddress, accountId, nftType, nftRoyaltyPercentage, nftAmount, validUntil, maxFeeTokenId, nftFactory, exchange, currentCid);
            mintResponses.Add(mintResponse);
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




