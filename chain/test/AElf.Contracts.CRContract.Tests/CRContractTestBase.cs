using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using AElf.ContractTestBase.ContractTestKit;
using System.Linq;
using AElf.Contracts.MultiToken;
using Microsoft.Extensions.DependencyInjection;
using AElf.Kernel.Token;

namespace AElf.Contracts.CRContract
{
    public class CRContractTestBase : DAppContractTestBase<CRContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);
        
        internal Address CRContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);
        internal CRContractContainer.CRContractStub GetCRContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<CRContractContainer.CRContractStub>(DAppContractAddress, senderKeyPair);
        }

        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return Application.ServiceProvider.GetRequiredService<IContractTesterFactory>()
                .Create<TokenContractContainer.TokenContractStub>(TokenContractAddress,
                    senderKeyPair);
        }
        internal CRContractContainer.CRContractStub stub => GetCRContractStub(keyPair);
        internal TokenContractContainer.TokenContractStub tokenStub =>
            GetTokenContractStub(SampleAccount.Accounts.First().KeyPair);
        
        internal ECKeyPair keyPair = SampleAccount.Accounts.Last().KeyPair;
        internal Address addr => Address.FromPublicKey(keyPair.PublicKey);
        
        internal TokenContractContainer.TokenContractStub userTokenStub =>
            GetTokenContractStub(keyPair);
    }
}