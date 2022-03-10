from typing import TypedDict

class CounterFactualNft(TypedDict):
    tokenAddress: str
    
class CounterFactualNftInfo(TypedDict):
    nftOwner: str
    nftFactory: str
    nftBaseUri: str