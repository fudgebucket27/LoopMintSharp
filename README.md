# LoopMintSharp
Automated single/batch minting on Loopring using the API.

This is a Console App on .NET 6. To build and compile this yourself you need something like Visual Studio 2022. 

I suggest downloading one of the compiled releases.

**LoopMintSharp is purely for minting NFTs on Loopring once the images and metadata have all been generated. If you want a full end to end solution for Loopring, I recommend [LooPyGen](https://github.com/sk33z3r/LooPyGen)**

# Setup 
Download one of the compiled releases in the [Releases](https://github.com/fudgebucket27/LoopMintSharp/releases) section and unzip it into a location of your choice. You will need to edit the included appsettings.json file with your own Loopring details; ie api key, private key,address(not your ENS) and account id. You can export these out from your account via https://loopring.io . Remember to keep these values private and do not share with anyone!

macOS users: You also need to run the following command in the unzipped folder of LoopMintSharp to turn it into an executable in order to run it. You may also need to add it as a trusted application if it get's blocked from running.

```batch
chmod +x LoopMintSharp
```
If compiling yourself please read the section about it below.

The following instructions in this README are for version 3 and above. For version 3 and above you can also follow the [video tutorial](https://www.youtube.com/watch?v=B6JnNo1WXyY)

For version 2 and below [see this](#video-tutorial-for-version-2-and-below).

## 1. Creating a collection
**All commands must be done in a command line window opened in the same folder as the unzipped LoopMintSharp**

Once you have setup the appsettings.json file you can call LoopMintSharp via command line as follows to create a collection

```batch
LoopMintSharp -createcollection
```

macOS users: 
```batch
./LoopMintSharp -createcollection
```

Follow the prompts for entering in the name, description, avatar, banner and tileUri. Loopring recommends 500x500 for avatar, 1500x500 for banner, 500x700 for tileUri images.

Be sure to save the contract address that is generated for you. You will need this for the next step.

## 2. Minting to a collection
**All commands must be done in a command line window opened in the same folder as the unzipped LoopMintSharp**

Once you have created a collection and have the contract address. Modify the included "cids.txt" file with the metadata json ipfs cids for the NFTs you intend to mint in the collection. The ipfs json cids need to be cid v0 which start with Qm. Once you have done that you can mint to that collection with the following command, replacing *0x1ad897a7957561dc502a19b38e7e5a3b045375bd* with the contract address that was generated for you in the previous step.

```batch
LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd
```

macOS users: 
```batch
./LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd
```

You can also specify a royalty address with v3.0.3 and above:

```batch
LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd
```

macOS users: 
```batch
./LoopMintSharp -mintcollection 0x1ad897a7957561dc502a19b38e7e5a3b045375bd -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd
```

## 3. Minting to the legacy contract
**All commands must be done in a command line window opened in the same folder as the unzipped LoopMintSharp**

Before the introduction of collections, all NFTs on Loopring fell under one contract. This is now considered legacy. If need be you can also mint to this legacy contract. Modify the included "cids.txt" file with the metadata json ipfs cids for the NFTs you intend to mint in the legacy contract. The ipfs json cids need to be cid v0 which start with Qm. Once you have done that you can mint to the legacy contract with the following command.

```batch
LoopMintSharp -legacymintcollection
```

macOS users: 
```batch
./LoopMintSharp -legacymintcollection
```

You can also specify a royalty address with v3.0.3 and above:

```batch
LoopMintSharp -legacymintcollection -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd
```

macOS users: 
```batch
./LoopMintSharp -legacymintcollection -royaltyAddress 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd
```
## 4. Minting red packet nfts (only in v3.0.4 and up)
**All commands must be done in a command line window opened in the same folder as the unzipped LoopMintSharp**

**You need to have filled out the "Layer1PrivateKey" in the appsettings.json file with your own Layer 1 private key, this needs to come from the same wallet associated with your Loopring layer 2 address.**

LoopMintSharp will mint private red packets. Red packets nfts can be batch minted by entering the appropriate data into the included nftData.txt file

Example nftData.txt file:
```batch 
0x09ad0103c3b15dcc6c7465308b4cbcf68ef8dd6c9568b0e501f90100b339e1bc,1,1,30,false
0x17b92c6e88f09029ca125c11fa5d19e2eb028c2ef7ad351f16366f6d47d20f38,2,6,15,true
```

Each line contains a comma seperated list of values. The first value is the nftData. To collect nftData you can use maize to retrieve it: https://github.com/cobmin/Maize, otherwise if you minted with LoopMintSharp the CSV report from a previous mint will contain the nftData.The second value is the amount of red packets, the third value is the amount of nfts per red packet, the fourth value is the amount of days the red packet is valid for and the fifth value is a true/false value on whether the red packet should be randomly split amongst recievers.

The total Amount of Red Packets multiplied by the Amount of Nfts per red packet can not exceed the amount of editions of an NFT you hold in your wallet. 

For example if you hold a balance of 10 editions for an NFT, you can create 2 red packets multipled by 5 nfts per red packet, total = 10. You can not create 3 red packets multipled by 5 nfts per red packet, total = 15.

Once the nftData.txt file is setup run the following command to start minting

```batch
LoopMintSharp -mintredpacketnft
```

macOS users: 
```batch
./LoopMintSharp -mintredpacketnft
```

QR codes for the red packets will be generated in the folder of LoopMintSharp after minting is done.

# Compiling yourself
If compiling yourself. You need to generate an appsettings.json file in the project directory with the "Copy to Output directory" set to "Copy Always". The appsettings.json file should look like the following, remember to keep these values private and do not share with anyone!

```json
{
  "Settings": {
    "LoopringApiKey": "kd", //Your loopring api key
    "LoopringPrivateKey": "0x", //Your loopring private key
    "Layer1PrivateKey": "", //Your layer 1 private key, only needed to mint red packet nfts
    "LoopringAddress": "0x", //Your loopring address
    "LoopringAccountId": 40940, //Your loopring account id 
    "NftAmount": 1, //Amount of NFT Editions per mint, number between 1 - 10 000, For 1 to 1 NFTs leave it as 1
    "NftRoyaltyPercentage": 6, //Nft royalty percentage can be between 0% - 50%
    "NftType": 0, //Nft type. 0 = ERC1155, 1 = ERC721
    "ValidUntil": 1700000000, //How long this mint should be valid for. Shouldn't have to change this value
    "MaxFeeTokenId": 1, //The token id for the fee. 0 for ETH, 1 for LRC
    "NftFactory": "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026", //Nft factory of loopring
    "NftFactoryCollection": "0x97BE94250AEF1Df307749aFAeD27f9bc8aB911db", //Nft factory for collections on loopring 
    "Exchange": "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4", //Loopring Exchange address
    "VerboseLogging": false, //set to either true or false for verbose logging. default is false
    "SkipMintFeePrompt": false //set to either true or false to skip the mint fee prompt when batch minting. default is false
  }
}
```
You will also need to supply command line arguments via your IDE.

# Video Tutorial for Version 2 and below
Here is a video tutorial on how to use LoopMintSharp v2 for single mint use on version 2 and below: https://www.youtube.com/watch?v=yYlaKdXIcuQ Thanks To @BimboSlice5 on Twitter! 

# Video Tutorial for Version 3 and above
Here is a video tutorial on how to use LoopMintSharp v3 and above: https://www.youtube.com/watch?v=B6JnNo1WXyY
