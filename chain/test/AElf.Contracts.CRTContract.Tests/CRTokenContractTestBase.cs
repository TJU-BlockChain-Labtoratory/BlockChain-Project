using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using AElf.ContractTestBase.ContractTestKit;
using System.Linq;
using AElf.Contracts.MultiToken;
using Microsoft.Extensions.DependencyInjection;

namespace AElf.Contracts.CRTContract
{
    public class CRTContractTestBase : DAppContractTestBase<CRTokenContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        internal Address CRTContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);
        
        internal CRTContractContainer.CRTContractStub GetCRTContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<CRTContractContainer.CRTContractStub>(DAppContractAddress, senderKeyPair);
        }
        
        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return Application.ServiceProvider.GetRequiredService<IContractTesterFactory>()
                .Create<TokenContractContainer.TokenContractStub>(TokenContractAddress,
                    senderKeyPair);
        }
        
        internal TokenContractContainer.TokenContractStub tokenStub =>
            GetTokenContractStub(SampleAccount.Accounts.First().KeyPair);
        
        internal ECKeyPair keyPair = SampleAccount.Accounts.Last().KeyPair;
        internal Address addr => Address.FromPublicKey(keyPair.PublicKey);
        
        internal CRTContractContainer.CRTContractStub stub => GetCRTContractStub(keyPair);
        
        internal TokenContractContainer.TokenContractStub userTokenStub =>
            GetTokenContractStub(keyPair);
    }
}