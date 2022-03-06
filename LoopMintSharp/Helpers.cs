using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum;
using Nethereum.Web3;
using Org.BouncyCastle.Crypto.Digests;

namespace LoopMintSharp
{
    public static class Helpers
    {
        public static string ComputerNftTokenAddress(CounterFactualNftInfo counterFactualNftInfo)
        {
            string nftOwner = "";
            string nftFactory = "0xc852aC7aAe4b0f0a0Deb9e8A391ebA2047d80026";
            string nftBaseUri = "";
            if (counterFactualNftInfo.nftOwner.StartsWith("0x"))
            {
                nftOwner = counterFactualNftInfo.nftOwner.Substring(2, counterFactualNftInfo.nftOwner.Length - 2);
            }


            byte[] nftContractCreationBytes = Encoding.UTF8.GetBytes("NFT_CONTRACTION_CREATION");
            byte[] nftOwnerBytes = StringToByteArray(nftOwner);
            byte[] nftBaseUriBytes = Encoding.UTF8.GetBytes(nftBaseUri);
            byte[] saltBytes = new byte[nftContractCreationBytes.Length + nftOwnerBytes.Length + nftBaseUriBytes.Length];
            System.Buffer.BlockCopy(nftContractCreationBytes, 0, saltBytes, 0, nftContractCreationBytes.Length);
            System.Buffer.BlockCopy(nftOwnerBytes, 0, saltBytes, nftContractCreationBytes.Length, nftOwnerBytes.Length);
            System.Buffer.BlockCopy(nftBaseUriBytes, 0, saltBytes, nftContractCreationBytes.Length + nftOwnerBytes.Length, nftBaseUriBytes.Length);

            byte[] creationCodeBytes = StringToByteArray("3d602d80600a3d3981f3363d3d373d3d3d363d73b25f6d711aebf954fb0265a3b29f7b9beba7e55d5af43d82803e903d91602b57fd5bf3");
            var creationCodeDigest = new KeccakDigest(256);
            creationCodeDigest.BlockUpdate(creationCodeBytes, 0, creationCodeBytes.Length);
            var calculatedCreationCodeHash = new byte[creationCodeDigest.GetByteLength()];
            creationCodeDigest.DoFinal(calculatedCreationCodeHash, 0);
            byte[] creationCodeHashBytes = Encoding.UTF8.GetBytes(BitConverter.ToString(calculatedCreationCodeHash).Replace("-", "").ToLower());

            var saltDigest = new KeccakDigest(256);
            saltDigest.BlockUpdate(saltBytes, 0, saltBytes.Length);
            var calculatedSaltHash = new byte[saltDigest.GetByteLength()];
            saltDigest.DoFinal(calculatedSaltHash, 0);
            byte[] saltHashBytes = Encoding.UTF8.GetBytes(BitConverter.ToString(calculatedSaltHash).Replace("-", "").ToLower());


            byte[] ffBytes = StringToByteArray("ff");
            byte[] nftFactoryBytes = StringToByteArray(nftFactory.Substring(2, nftFactory.Length - 2));

            byte[] rawBytes = new byte[ffBytes.Length + nftFactoryBytes.Length + saltHashBytes.Length + creationCodeHashBytes.Length];
            System.Buffer.BlockCopy(ffBytes, 0, rawBytes, 0, ffBytes.Length);
            System.Buffer.BlockCopy(nftFactoryBytes, 0, rawBytes, ffBytes.Length, nftFactoryBytes.Length);
            System.Buffer.BlockCopy(saltHashBytes, 0, rawBytes, ffBytes.Length + nftFactoryBytes.Length, saltHashBytes.Length);
            System.Buffer.BlockCopy(creationCodeHashBytes, 0, rawBytes, ffBytes.Length + nftFactoryBytes.Length + saltHashBytes.Length, creationCodeHashBytes.Length);

            var tokenDigest = new KeccakDigest(256);
            tokenDigest.BlockUpdate(rawBytes, 0, rawBytes.Length);
            var calculatedTokenHash = new byte[tokenDigest.GetByteLength()];
            tokenDigest.DoFinal(calculatedTokenHash, 0);
            var tokenHash = BitConverter.ToString(calculatedTokenHash).Replace("-", "").ToLower();

            return  tokenHash;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
