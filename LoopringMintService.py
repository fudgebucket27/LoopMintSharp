# import sys
# from os import path

# sys.path.insert(0, path.abspath(path.join(path.dirname(__file__), "hello_loopring")))

import aiohttp
import asyncio
import json
from typing import TypedDict, cast
from pprint import pprint

from StorageId import StorageId
from CounterFactualNft import CounterFactualNft, CounterFactualNftInfo
from OffchainFee import OffchainFee
from hello_loopring.sdk.ethsnarks import eddsa

from hello_loopring.sdk.ethsnarks.eddsa import PureEdDSA, PoseidonEdDSA
from hello_loopring.sdk.ethsnarks.field import FQ, SNARK_SCALAR_FIELD
from hello_loopring.sdk.ethsnarks.poseidon import poseidon_params, poseidon
from hello_loopring.sdk.sig_utils.eddsa_utils import *

class NFTDataEddsaSignHelper(EddsaSignHelper):
    MAX_INPUTS: int = 6

    def __init__(self, private_key="0x1"):
        super(NFTDataEddsaSignHelper, self).__init__(
            poseidon_params = poseidon_params(SNARK_SCALAR_FIELD, self.MAX_INPUTS+1, 6, 52, b'poseidon', 5, security_target=128),
            private_key = private_key
        )

    def serialize_data(self, inputs):
        return [int(data) for data in inputs][:self.MAX_INPUTS]

class NFTEddsaSignHelper(EddsaSignHelper):
    MAX_INPUTS: int = 9

    def __init__(self, private_key="0x1"):
        super(NFTEddsaSignHelper, self).__init__(
            poseidon_params = poseidon_params(SNARK_SCALAR_FIELD, self.MAX_INPUTS+1, 6, 53, b'poseidon', 5, security_target=128),
            private_key = private_key
        )

    def serialize_data(self, inputs):
        return [int(data) for data in inputs][:self.MAX_INPUTS]

class MintResponseData(TypedDict):
    hash: str
    nftTokenId: int
    nftData: str
    status: str
    isIdempotent: bool
    accountId: int
    storageId: int

class LoopringMintService(object):
    base_url: str = "https://api3.loopring.io"
    session: aiohttp.ClientSession

    def __init__(self) -> None:
        self.session = aiohttp.ClientSession(base_url=self.base_url)
    
    async def getNextStorageId(self, apiKey: str, accountId: int, sellTokenId: int) -> StorageId:
        params = {"accountId": accountId, 
                  "sellTokenId": sellTokenId}
        headers = {"x-api-key": apiKey}
        storage_id = None

        try:
            response = await self.session.request("get", "/api/v3/storageId", params=params, headers=headers)
            response.raise_for_status()
            storage_id = cast(StorageId, await response.json())
            # print(storage_id)
        except aiohttp.ClientError as client_err:
            print(f"Error getting storage id: {client_err}")
        except Exception as err:
            print(f"An error ocurred getting storage id: {err}")

        return storage_id

    async def computeTokenAddress(self, apiKey: str, counterFactualNftInfo: CounterFactualNftInfo) -> CounterFactualNft:
        params = {"nftFactory": counterFactualNftInfo['nftFactory'], 
                  "nftOwner": counterFactualNftInfo['nftOwner'],
                  "nftBaseUri": counterFactualNftInfo['nftBaseUri']}
        headers = {"x-api-key": apiKey}
        counterfactual_nft = None

        try:
            response = await self.session.request("get", "/api/v3/nft/info/computeTokenAddress", params=params, headers=headers)
            response.raise_for_status()
            counterfactual_nft = cast(CounterFactualNft, await response.json())
            # print(counterfactual_nft)
        except aiohttp.ClientError as client_err:
            print(f"Error computing token address: {client_err}")
        except Exception as err:
            print(f"An error ocurred computing token address: {err}")

        return counterfactual_nft

    async def getOffChainFee(self, apiKey: str, accountId: int, requestType: int, tokenAddress: str) -> OffchainFee:
        params = {"accountId": accountId, 
                  "requestType": requestType,
                  "tokenAddress": tokenAddress}
        headers = {"x-api-key": apiKey}
        off_chain_fee = None

        try:
            response = await self.session.request("get", "/api/v3/user/nft/offchainFee", params=params, headers=headers)
            response.raise_for_status()
            off_chain_fee = cast(OffchainFee, await response.json())
            # print(off_chain_fee)
        except aiohttp.ClientError as client_err:
            print(f"Error getting off chain fee: {client_err}")
        except Exception as err:
            print(f"An error ocurred getting off chain fee: {err}")

        return off_chain_fee
        

    async def mintNft(
            self, 
            apiKey: str,
            exchange: str,
            minterId: int,
            minterAddress: str,
            toAccountId: int,
            toAddress: str,
            nftType: int,
            tokenAddress: str,
            nftId: str,
            amount: str,
            validUntil: int,
            creatorFeeBips: int,
            storageId: int,
            maxFeeTokenId: int,
            maxFeeAmount: str,
            forceToMint: bool,
            counterFactualNftInfo: CounterFactualNftInfo,
            eddsaSignature: str) -> MintResponseData:
        params = {"exchange": exchange, 
                  "minterId": minterId,
                  "minterAddress": minterAddress,
                  "toAccountId": toAccountId,
                  "toAddress": toAddress,
                  "nftType": nftType,
                  "tokenAddress": tokenAddress,
                  "nftId": nftId,
                  "amount": amount,
                  "validUntil": validUntil,
                  "creatorFeeBips": creatorFeeBips,
                  "storageId": storageId,
                  "maxFee": {
                      "tokenId": maxFeeTokenId,
                      "amount": maxFeeAmount
                  },
                  "forceToMint": forceToMint,
                  "counterFactualNftInfo": {
                      "nftFactory": counterFactualNftInfo['nftFactory'],
                      "nftOwner": counterFactualNftInfo['nftOwner'],
                      "nftBaseUri": counterFactualNftInfo['nftBaseUri']
                  },
                  "eddsaSignature": eddsaSignature}
        headers = {"x-api-key": apiKey}
        nft_mint_data = None

        try:
            response = await self.session.post("/api/v3/nft/mint", json=params, headers=headers)
            pprint(await response.json())
            response.raise_for_status()
            nft_mint_data = cast(MintResponseData, await response.json())
            # print(nft_mint_data)
        except aiohttp.ClientError as client_err:
            print(f"Error minting nft: ")
            pprint(client_err)
        except Exception as err:
            print(f"An error ocurred minting nft: ")
            pprint(err)

        return nft_mint_data

    async def __aenter__(self) -> 'LoopringMintService':
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        await self.session.close()
        