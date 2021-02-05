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
        
        internal TokenContractContainer.TokenContractStub TokenContractStub =>
            GetTokenContractStub(SampleAccount.Accounts.First().KeyPair);
        
        internal CRContractContainer.CRContractStub CRContractStub =>
            GetCRContractStub(SampleAccount.Accounts.First().KeyPair);

        internal ECKeyPair AliceKeyPair { get; set; } = SampleAccount.Accounts.Last().KeyPair;
        internal ECKeyPair BobKeyPair { get; set; } = SampleAccount.Accounts.Reverse().Skip(1).First().KeyPair;
        internal Address AliceAddress => Address.FromPublicKey(AliceKeyPair.PublicKey);
        internal Address BobAddress => Address.FromPublicKey(BobKeyPair.PublicKey);
        
        internal CRContractContainer.CRContractStub AliceCRContractStub =>
            GetCRContractStub(AliceKeyPair);

        internal TokenContractContainer.TokenContractStub AliceTokenContractStub => GetTokenContractStub(AliceKeyPair);

        internal CRContractContainer.CRContractStub BobCRContractStub =>
            GetCRContractStub(BobKeyPair);

        internal TokenContractContainer.TokenContractStub BobTokenContractStub => GetTokenContractStub(BobKeyPair);

    }
}