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


def main():
    parser = argparse.ArgumentParser(description="Loopring Hash and Sign Code Sample")
    parser.add_argument("-a", "--action", required=True, choices=['hash', 'sign'], help='choose action, "hash" calculates poseidon hash of inputs. "sign" signs the message.')
    parser.add_argument("-i", "--inputs", help='hash or sign message inputs. For poseidon hash, they should be number string list separated by "," like “1,2,3,4,5,6”, max len is 13 to compatible with loopring DEX config')
    parser.add_argument("-k", "--privatekey", default=None, help='private key to sign the inputs, should be a big int string, like “12345678”, user can try the key exported from loopring DEX')

    args = parser.parse_args()
    print(args)


if __name__ == '__main__':
    main()