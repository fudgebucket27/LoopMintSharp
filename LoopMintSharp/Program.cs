using LoopMintSharp;
using Microsoft.Extensions.Configuration;
using CsvHelper;
using System.Globalization;

if(args.Length == 0)
{
    Console.WriteLine("LoopMintSharp needs arguments passed from command line. You can use -createcollection to create a collection, -legacymintcollection to mint on the legacy contract,or -mintcollection to mint to a collection to the latest contract");
    Console.WriteLine("eg: LoopMintSharp -createcollection");
    Console.WriteLine("eg: LoopMintSharp -legacymintcollection");
    Console.WriteLine("When using -mintcollection pass it the collection contract address as well:");
    Console.WriteLine("eg: LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd");
    System.Environment.Exit(0);
}

//Changes these variables to suit
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>();
Minter minter = new Minter();
string loopringApiKey = settings.LoopringApiKey;//you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
string loopringPrivateKey = settings.LoopringPrivateKey; //you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
var layer1PrivateKey = settings.Layer1PrivateKey;
var minterAddress = settings.LoopringAddress; //your loopring address
var accountId = settings.LoopringAccountId; //your loopring account id
var nftType = settings.NftType; //nfttype 0 = ERC1155, shouldn't need to change this unless you want ERC721 which is 1
var nftRoyaltyPercentage = settings.NftRoyaltyPercentage; //i wonder what setting this to something other than 0 would do?
var nftAmount = settings.NftAmount; //leave this to one so you only mint 1
var validUntil = settings.ValidUntil; //the examples seem to use this number
var maxFeeTokenId = settings.MaxFeeTokenId; //0 should be for ETH, 1 is for LRC?
var nftFactory = settings.NftFactory; //current nft factory of loopring, shouldn't need to change unless they deploye a new contract again, sigh...
var nftFactoryCollection = settings.NftFactoryCollection; //nft collection factory for loopring
var exchange = settings.Exchange; //loopring exchange address, shouldn't need to change this,
var verboseLogging = settings.VerboseLogging; //setting for verbose logging
var skipMintFeePrompt = settings.SkipMintFeePrompt; //setting for mint fee prompt when batch minting

if(args[0].Trim() == "-createcollection")
{
    var name = "";
    var description = "";
    var avatar = "";
    var banner = "";
    var tileUri = "";

    while(string.IsNullOrEmpty(name))
    {
        Console.Write("Enter name for collection:");
        name = Console.ReadLine().Trim();
    }

    while (string.IsNullOrEmpty(description))
    {
        Console.Write("Enter description for collection:");
        description = Console.ReadLine().Trim();
    }


    while (!avatar.StartsWith("Qm"))
    {
        Console.Write("Enter avatar ipfs cid for collection:");
        avatar = Console.ReadLine().Trim();
    }


    while (!banner.StartsWith("Qm"))
    {
        Console.Write("Enter banner ipfs cid for collection:");
        banner = Console.ReadLine().Trim();
    }


    while (!tileUri.StartsWith("Qm"))
    {
        Console.Write("Enter tileUri ipfs cid for collection:");
        tileUri = Console.ReadLine().Trim();
    }

    var collectionResult = await minter.CreateNftCollection(
    loopringApiKey,
    "ipfs://" + avatar,
    "ipfs://" + banner,
    description,
    name,
    nftFactoryCollection,
    minterAddress,
    "ipfs://" + tileUri,
    loopringPrivateKey,
    verboseLogging
    );
}
else if (args[0].Trim() == "-legacymintcollection" )
{
    var lineCount = File.ReadLines("cids.txt").Count();
    var count = 0;

    var royaltyAddress = "";

    if (args.Length == 3 && !args[2].Trim().StartsWith("0x"))
    {
        Console.WriteLine("This argument -royaltyAddress needs a VALID royaltyAddress!");
        Console.WriteLine("eg: LoopMintSharp -legacymintcollection -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd");
        System.Environment.Exit(0);
    }

    if (args.Length == 3 && args[2].Trim().StartsWith("0x"))
    {
        royaltyAddress = args[2].Trim();
    }

    if (string.IsNullOrEmpty(royaltyAddress))
    {
        royaltyAddress = minterAddress;
    }

    if (skipMintFeePrompt == false)
    {
        var offChainFee = await minter.GetMintFee(loopringApiKey, accountId, minterAddress, nftFactory, verboseLogging, "");
        var fee = offChainFee.fees[maxFeeTokenId].fee;
        double feeAmount = lineCount * Double.Parse(fee);
        if (maxFeeTokenId == 0)
        {
            Console.WriteLine($"It will cost around {TokenAmountConverter.ToString(feeAmount, 18)} ETH to mint {lineCount} NFTs");
        }
        else if (maxFeeTokenId == 1)
        {
            Console.WriteLine($"It will cost around {TokenAmountConverter.ToString(feeAmount, 18)} LRC to mint {lineCount} NFTs");
        }
        else
        {
            Console.WriteLine("Can only use MaxFeeTokenId of 0 for ETH or MaxFeeTokenId of 1 for LRC. Please set this correctly in your appsettings.json file!");
            System.Environment.Exit(0);
        }

        Console.Write("Continue with minting? Enter y for yes or n for no:");
        string continueMinting = Console.ReadLine().Trim().ToLower();
        while (continueMinting != "y" && continueMinting != "n")
        {
            Console.Write("Continue with minting? Enter y for yes or n for no:");
            continueMinting = Console.ReadLine().Trim().ToLower();
        }

        if (continueMinting == "n")
        {
            Console.WriteLine("Minting cancelled!");
            System.Environment.Exit(0);
        }
        else if (continueMinting == "y")
        {
            Console.WriteLine($"Minting started on legacy contract...");
        }
    }

    List<MintResponseData> mintResponses = new List<MintResponseData>();
    using (StreamReader sr = new StreamReader("cids.txt"))
    {
        string currentCid;
        //currentCid will be null when the StreamReader reaches the end of file
        while ((currentCid = sr.ReadLine()) != null)
        {
            currentCid = currentCid.Trim();
            count++;
            Console.WriteLine($"Attempting mint {count} out of {lineCount} NFTs");
            var mintResponse = await minter.MintLegacyCollection(loopringApiKey, loopringPrivateKey, minterAddress, accountId, nftType, nftRoyaltyPercentage, nftAmount, validUntil, maxFeeTokenId, nftFactory, exchange, currentCid, verboseLogging, royaltyAddress);
            mintResponses.Add(mintResponse);
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            if (!string.IsNullOrEmpty(mintResponse.errorMessage))
            {
                Console.WriteLine($"Mint {count} out of {lineCount} NFTs was UNSUCCESSFUL. ERROR MESSAGE: {mintResponse.errorMessage}");
            }
            else
            {
                Console.WriteLine($"Mint {count} out of {lineCount} NFTs was SUCCESSFUL");
            }
        }
    }

    string csvName = $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.csv";
    using (var writer = new StreamWriter(csvName))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(mintResponses);
        Console.WriteLine($"Generated Mint Report");
        Console.WriteLine($"CSV can be found in the following location: {AppDomain.CurrentDomain.BaseDirectory + csvName}");
    }
}
else if (args[0].Trim() == "-mintcollection")
{
    var royaltyAddress = "";

    if(args.Length != 2 && args.Length !=4)
    {
        Console.WriteLine("This argument -mintcollection needs a collection contract address!");
        Console.WriteLine("eg: LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd");
        Console.WriteLine("You can also specify a royalty address!");
        Console.WriteLine("eg: LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd");
        System.Environment.Exit(0);
    }

    if(!args[1].Trim().StartsWith("0x"))
    {
        Console.WriteLine("This argument -mintcollection needs a VALID collection contract address!");
        Console.WriteLine("eg: LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd");
        System.Environment.Exit(0);
    }

    if(args.Length == 4 && !args[3].Trim().StartsWith("0x"))
    {
        Console.WriteLine("This argument -royaltyAddress needs a VALID royaltyAddress!");
        Console.WriteLine("eg: LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd");
        System.Environment.Exit(0);
    }

    if(args.Length == 4 && args[3].Trim().StartsWith("0x"))
    {
        royaltyAddress = args[3].Trim();
    }

    if(string.IsNullOrEmpty(royaltyAddress))
    {
        royaltyAddress = minterAddress;
    }

    var collectionContractAddress = args[1].Trim();
    var lineCount = File.ReadLines("cids.txt").Count();
    var count = 0;

    var collectionResult = await minter.FindNftCollection(loopringApiKey, 12, 0, minterAddress, collectionContractAddress, verboseLogging);
    if (collectionResult == null)
    {
        Console.WriteLine($"Could not find collection with contract address {collectionContractAddress}");
        System.Environment.Exit(0);
    }

    if (skipMintFeePrompt == false)
    {
        var offChainFee = await minter.GetMintFee(loopringApiKey, accountId, minterAddress, nftFactoryCollection, verboseLogging, collectionResult.collections[0].collection.baseUri);
        var fee = offChainFee.fees[maxFeeTokenId].fee;
        double feeAmount = lineCount * Double.Parse(fee);
        if (maxFeeTokenId == 0)
        {
            Console.WriteLine($"It will cost around {TokenAmountConverter.ToString(feeAmount, 18)} ETH to mint {lineCount} NFTs");
        }
        else if (maxFeeTokenId == 1)
        {
            Console.WriteLine($"It will cost around {TokenAmountConverter.ToString(feeAmount, 18)} LRC to mint {lineCount} NFTs");
        }
        else
        {
            Console.WriteLine("Can only use MaxFeeTokenId of 0 for ETH or MaxFeeTokenId of 1 for LRC. Please set this correctly in your appsettings.json file!");
            System.Environment.Exit(0);
        }

        Console.Write("Continue with minting? Enter y for yes or n for no:");
        string continueMinting = Console.ReadLine().Trim().ToLower();
        while (continueMinting != "y" && continueMinting != "n")
        {
            Console.Write("Continue with minting? Enter y for yes or n for no:");
            continueMinting = Console.ReadLine().Trim().ToLower();
        }

        if (continueMinting == "n")
        {
            Console.WriteLine("Minting cancelled!");
            System.Environment.Exit(0);
        }
        else if (continueMinting == "y")
        {
            Console.WriteLine($"Minting started on {collectionContractAddress}...");
        }
    }

    List<MintResponseData> mintResponses = new List<MintResponseData>();
    using (StreamReader sr = new StreamReader("cids.txt"))
    {
        string currentCid;
        //currentCid will be null when the StreamReader reaches the end of file
        while ((currentCid = sr.ReadLine()) != null)
        {
            currentCid = currentCid.Trim();
            count++;
            Console.WriteLine($"Attempting mint {count} out of {lineCount} NFTs");
            var mintResponse = await minter.MintCollection(loopringApiKey, loopringPrivateKey, minterAddress, accountId, nftType, nftRoyaltyPercentage, nftAmount, validUntil, maxFeeTokenId, nftFactoryCollection, exchange, currentCid, verboseLogging, collectionResult.collections[0].collection.baseUri, collectionContractAddress, royaltyAddress);
            mintResponses.Add(mintResponse);
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            if (!string.IsNullOrEmpty(mintResponse.errorMessage))
            {
                Console.WriteLine($"Mint {count} out of {lineCount} NFTs was UNSUCCESSFUL. ERROR MESSAGE: {mintResponse.errorMessage}");
            }
            else
            {
                Console.WriteLine($"Mint {count} out of {lineCount} NFTs was SUCCESSFUL");
            }
        }
    }

    string csvName = $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.csv";
    using (var writer = new StreamWriter(csvName))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(mintResponses);
        Console.WriteLine($"Generated Mint Report");
        Console.WriteLine($"CSV can be found in the following location: {AppDomain.CurrentDomain.BaseDirectory + csvName}");
    }
}
else if (args[0].Trim() == "-mintredpacketnft")
{
    string nftData = "0x2afcb3a9ab52c109ca6ad8ef6315dc747867589bafcabf2e7e8b7f1983e3c2d9";
    var nftBalance = await minter.GetTokenIdWithCheck(loopringApiKey, accountId, nftData, verboseLogging);
    var amountOfNftsPerPacket = "1";
    var amountOfPackets = "1";
    var validUntilDays = 30;
    //Console.WriteLine($"{nftBalance.data[0].tokenAddress}, {nftBalance.totalNum}");
    var offchainFee = await minter.GetMintFeeWithAmount(loopringApiKey, accountId, "0xbeb2f2367c1e79003dffa34f16a2b933624a6e05", verboseLogging);
    var mintRedPacketNft = await minter.MintRedPacketNft(loopringApiKey, loopringPrivateKey, layer1PrivateKey, minterAddress, accountId, nftBalance, validUntilDays, maxFeeTokenId, exchange, amountOfNftsPerPacket, amountOfPackets, verboseLogging);
}
else
        {
    Console.WriteLine("Invalid arguments. You can use -createcollection to create a collection, -legacymintcollection to mint on the legacy contract,or -mintcollection to mint to a collection to the latest contract");
    Console.WriteLine("eg: LoopMintSharp -createcollection");
    Console.WriteLine("eg: LoopMintSharp -legacymintcollection");
    Console.WriteLine("When using -mintcollection pass it the collection contract address as well:");
    Console.WriteLine("eg: LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd");
}
