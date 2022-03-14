# LoopMintSharp
Automated minting on Loopring using the API.

This is a Console App on .NET 6. To build and compile this yourself you need something like Visual Studio 2022.

# Setup 
Download one of the compiled release executables in the [Releases](https://github.com/fudgebucket27/LoopMintSharp/releases) section. You will need to edit the included appsettings.json file with your own Loopring details, ie api key, private key and etc. You can export these out from your account via https://loopring.io

Once you have setup the appsettings.json file you can call LoopMintSharp via command line as follows, where the first argument is the IPFS CID of your metadata.json file:

```batch
LoopMintSharp QmWG5QL4MbDux8Dtb1AkijMH73fFnuUDumMxVkQw6YFyBE
```

# Compiling yourself
If compiling yourself. You need to generate an appsettings.json file in the project directory with the "Copy to Output directory" set to "Copy Always". The appsetings.json file should look like the following

```json
{
  "Settings": {
    "LoopringApiKey": "Your Api Key Here", //Your loopring api key
    "LoopringPrivateKey": "Your Private Key Here", //Your loopring private key
    "LoopringAddress": "Your loopring address here", //Your loopring address
    "LoopringAccountId": 40940, //Your loopring account id 
    "NftAmount": 1, //Amount of NFTs to mint, number between 1 - 10 000
    "NftRoyaltyPercentage": 6, //Nft royalty percentage can be between 0% - 50%
    "NftType": 0, //Nft type. 0 = ERC1155, 1 = ERC721
    "ValidUntil": 1700000000, //How long this mint should be valid for. Shouldn't have to change this value
    "MaxFeeTokenId": 0, //The token id for the fee. 0 for ETH, 1 for LRC
    "NftFactory": "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026", //Nft factory of loopring
    "Exchange": "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4" //Loopring Exchange address
  }
}
```
You will also need to supply a command line argument for the ipfs cid or just put it in the code directly.
