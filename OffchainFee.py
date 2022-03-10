from typing import TypedDict

class Fee(TypedDict):
    token: str
    fee: str
    discount: int
    
class OffchainFee(TypedDict):
    gasPrice: str
    fees: 'list[Fee]'