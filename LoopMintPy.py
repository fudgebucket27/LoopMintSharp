#!/usr/bin/env python3
import sys
from os import path, environ

sys.path.insert(0, path.abspath(path.join(path.dirname(__file__), "hello_loopring")))

import argparse
import asyncio
from aiohttp import ClientSession
import json
from pprint import pprint
import base58

from LoopringMintService import LoopringMintService, NFTDataEddsaSignHelper, NFTEddsaSignHelper
from CounterFactualNft import CounterFactualNftInfo

cfg = {}

def setup():
    # Changes these variables to suit
    cfg['loopringApiKey']       = environ.get("LOOPRINGAPIKEY")                     # TODO: Get from env variable. you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
    cfg['loopringPrivateKey']   = environ.get("LOOPRINGPRIVATEKEY")                 # TODO: Get from env variable. you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
    cfg['ipfsCid']              = "QmdmRoWVU4PV9ZCi1khprtX2YdAzV9UEFN5igZZGxPVAa4"  # the ipfs cid of your metadata.json
    cfg['minterAddress']        = "0x1c65331556Cff08bb06c56fBb68FB0D1D2194F8A"      # your loopring address
    cfg['accountId']            = 34247		                                        # your loopring account id
    cfg['nftType']              = 0		                                            # nfttype 0 = ERC1155, shouldn't need to change this unless you want ERC721 which is 1
    cfg['creatorFeeBips']       = 10		                                        # i wonder what setting this to something other than 0 would do?
    cfg['amount']               = 1		                                            # leave this to one so you only mint 1
    cfg['validUntil']           = 1700000000		                                # the examples seem to use this number
    cfg['maxFeeTokenId']        = 1		                                            # 0 should be for ETH, 1 is for LRC?
    cfg['nftFactory']           = "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026"	    # current nft factory of loopring, shouldn't need to change unless they deploye a new contract again, sigh...
    cfg['exchange']             = "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4"	    # loopring exchange address, shouldn't need to change this
    print("config dump:")
    pprint(cfg)

    assert cfg['loopringPrivateKey'] is not None and cfg['loopringPrivateKey'][:2] == "0x"
    assert cfg['loopringApiKey'] is not None

def parse_args():
    pass
    
async def main():
    # Initial Setup
    setup()
    args = parse_args()

    # Get storage id, token address and offchain fee
    async with LoopringMintService() as lms:
        # Getting the storage id
        storage_id = await lms.getNextStorageId(apiKey=cfg['loopringApiKey'], accountId=cfg['accountId'], sellTokenId=cfg['maxFeeTokenId'])
        print(f"Storage id: {json.dumps(storage_id, sort_keys=True, indent=4)}")

        # Getting the token address
        counterfactual_ntf_info = CounterFactualNftInfo(nftOwner=cfg['minterAddress'], nftFactory=cfg['nftFactory'], nftBaseUri="")
        counterfactual_nft = await lms.computeTokenAddress(apiKey=cfg['loopringApiKey'], counterFactualNftInfo=counterfactual_ntf_info)
        print(f"CounterFactualNFT Token Address: {json.dumps(counterfactual_nft, sort_keys=True, indent=4)}")

        # Getting the offchain fee
        off_chain_fee = await lms.getOffChainFee(apiKey=cfg['loopringApiKey'], accountId=cfg['accountId'], requestType=9, tokenAddress=counterfactual_nft['tokenAddress'])
        print(f"Offchain fee:  {json.dumps(off_chain_fee, sort_keys=True, indent=4)}")
    
    # Generate Eddsa Signature
    # Generate the nft id here
    nft_id = "0x" + base58.b58decode(cfg['ipfsCid']).hex()[4:]    # Base58 to hex and drop first 2 bytes
    print(f"Generated NFT ID: {nft_id}")

    # Generate the poseidon hash for the nft data
    # https://github.com/Loopring/loopring_sdk/blob/692d372165b5ea0d760e33e177d9003cc0dfb0f7/src/api/sign/sign_tools.ts#L704
    ntf_id_hi = int(nft_id[2:34], 16)   # Skip "0x" prefix
    nft_id_lo = int(nft_id[34:66], 16)
    inputs = [
        int(cfg['minterAddress'], 16),
        cfg['nftType'],
        int(counterfactual_nft['tokenAddress'], 16),
        nft_id_lo,
        ntf_id_hi,
        cfg['creatorFeeBips']
    ]
    hasher = NFTDataEddsaSignHelper()
    nft_data_poseidon_hash = hasher.hash(inputs)
    # pprint(inputs)
    # pprint(hex(nft_data_poseidon_hash))
    print(f"Hashed NFT data: {hex(nft_data_poseidon_hash)}")

    # Generate the poseidon hash for the remaining data
    # https://github.com/Loopring/loopring_sdk/blob/692d372165b5ea0d760e33e177d9003cc0dfb0f7/src/api/sign/sign_tools.ts#L899
    inputs = [
        int(cfg['exchange'], 16),
        cfg['accountId'],   # minterId
        cfg['accountId'],   # toAccountId
        nft_data_poseidon_hash,
        cfg['amount'],
        cfg['maxFeeTokenId'],
        int(off_chain_fee['fees'][cfg['maxFeeTokenId']]['fee']),
        cfg['validUntil'],
        storage_id['offchainId']
    ]
    hasher = NFTEddsaSignHelper(private_key=cfg['loopringPrivateKey'])
    nft_poseidon_hash = hasher.hash(inputs)
    # pprint(inputs)
    # pprint(hex(nft_poseidon_hash))
    print(f"Hashed NFT payload: {hex(nft_poseidon_hash)}")

    # hasher = NFTEddsaSignHelper()
    eddsa_signature = hasher.sign(inputs)
    print(f"Signed NFT payload hash: {eddsa_signature}")

    # Submit the nft mint
    async with LoopringMintService() as lms:
        nft_mint_response = await lms.mintNft(
            apiKey=cfg['loopringApiKey'],
            exchange=cfg['exchange'],
            minterId=cfg['accountId'],
            minterAddress=cfg['minterAddress'],
            toAccountId=cfg['accountId'],
            toAddress=cfg['minterAddress'],
            nftType=cfg['nftType'],
            tokenAddress=counterfactual_nft['tokenAddress'],
            nftId=nft_id,
            amount=str(cfg['amount']),
            validUntil=cfg['validUntil'],
            creatorFeeBips=cfg['creatorFeeBips'],
            storageId=storage_id['offchainId'],
            maxFeeTokenId=cfg['maxFeeTokenId'],
            maxFeeAmount=off_chain_fee['fees'][cfg['maxFeeTokenId']]['fee'],
            forceToMint=False,
            counterFactualNftInfo=counterfactual_ntf_info,
            eddsaSignature=eddsa_signature
        )
        print(f"Nft Mint reponse: {json.dumps(nft_mint_response, sort_keys=True, indent=4)}")

if __name__ == '__main__':
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())