#!/usr/bin/env python3
import sys
from os import path

sys.path.insert(0, path.abspath(path.join(path.dirname(__file__), "hello_loopring")))

import hashlib
from sdk.ethsnarks.eddsa import PureEdDSA, PoseidonEdDSA
from sdk.ethsnarks.field import FQ, SNARK_SCALAR_FIELD
from sdk.ethsnarks.poseidon import poseidon_params, poseidon
from sdk.sig_utils.eddsa_utils import *
import argparse

cfg = {}

def setup():
    # Changes these variables to suit
    cfg['loopringApiKey']       = "LOOPRINGAPIKEY"                                  # TODO: Get from env variable. you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
    cfg['loopringPrivateKey']   = "LOOPRINGPRIVATEKEY"                              # TODO: Get from env variable. you can either set an environmental variable or input it here directly. You can export this from your account using loopring.io
    cfg['ipfsCid']              = "QmVQ4jnoHAMrRycy4JMrndjrLVXDzPn6d7tjabZmCX5eXy"  # the ipfs cid of your metadata.json
    cfg['minterAddress']        = "0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd"      # your loopring address
    cfg['accountId']            = 40940		                                        # your loopring account id
    cfg['nftType']              = 0		                                            # nfttype 0 = ERC1155, shouldn't need to change this unless you want ERC721 which is 1
    cfg['creatorFeeBips']       = 50		                                        # i wonder what setting this to something other than 0 would do?
    cfg['amount']               = 1		                                            # leave this to one so you only mint 1
    cfg['validUntil']           = 1700000000		                                # the examples seem to use this number
    cfg['maxFeeTokenId']        = 0		                                            # 0 should be for ETH, 1 is for LRC?
    cfg['nftFactory']           = "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026"	    # current nft factory of loopring, shouldn't need to change unless they deploye a new contract again, sigh...
    cfg['exchange']             = "0x0BABA1Ad5bE3a5C0a66E7ac838a129Bf948f1eA4"	    # loopring exchange address, shouldn't need to change this

def main():
    setup()
    from pprint import pprint
    pprint(cfg)

    # parser = argparse.ArgumentParser(description="Loopring Hash and Sign Code Sample")
    # parser.add_argument("-a", "--action", required=True, choices=['hash', 'sign'], help='choose action, "hash" calculates poseidon hash of inputs. "sign" signs the message.')
    # parser.add_argument("-i", "--inputs", help='hash or sign message inputs. For poseidon hash, they should be number string list separated by "," like “1,2,3,4,5,6”, max len is 13 to compatible with loopring DEX config')
    # parser.add_argument("-k", "--privatekey", default=None, help='private key to sign the inputs, should be a big int string, like “12345678”, user can try the key exported from loopring DEX')

    # args = parser.parse_args()
    # pprint(args)


if __name__ == '__main__':
    main()