using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using AElf.ContractTestBase.ContractTestKit;
using System.Linq;

namespace AElf.Contracts.CRTContract
{
    public class CRTContractTestBase : DAppContractTestBase<CRTokenContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);
        internal ECKeyPair keyPair = SampleAccount.Accounts.First().KeyPair;
        internal Address addr => Address.FromPublicKey(keyPair.PublicKey);
        internal CRTContractContainer.CRTContractStub GetCRTContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<CRTContractContainer.CRTContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}