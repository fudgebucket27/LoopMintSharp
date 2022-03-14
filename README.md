# LoopMintSharp
Trying to automate minting on loopring using the API. I'm using my own PoseidonSharp for the hashing.

This is a Console App on .NET 6. To build and compile this yourself you need something like Visual Studio 2022.

# Setup 
Download one of the compiled release executables in the [Releases](https://github.com/fudgebucket27/LoopMintSharp/releases) section. You will need to edit the included appsettings.json file with your own Loopring details, ie api key, private key and etc. You can export these out from your account via https://loopring.io

Once you have setup the appsettings.json file you can call LoopMintSharp via command line as follows, where the first argument is the IPFS CID of your metadata.json file:

```batch
LoopMintSharp QmWG5QL4MbDux8Dtb1AkijMH73fFnuUDumMxVkQw6YFyBE
```
