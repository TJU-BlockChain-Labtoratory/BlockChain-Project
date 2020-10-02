using System.Linq;
using AElf.Contracts.TestKit;
using AElf.Cryptography.ECDSA;
using AElf.Kernel;
using AElf.Types;
using Volo.Abp.Threading;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.SmartContract.Application;
using AElf.Kernel.Token;
using AElf.Contracts.MultiToken;
using AElf.Boilerplate.TestBase;
using Microsoft.Extensions.DependencyInjection;


namespace AElf.Contracts.CopyRightContract
{
    public class CopyRightContractTestBase : ContractTestBase<CopyRightContractTestModule>
    {
        internal Address CopyRightContractAddress => GetAddress(DAppContractAddressNameProvider.StringName);

        internal CopyRightContractContainer.CopyRightContractStub CopyRightContractStub =>
            GetCopyRightContractStub(SampleECKeyPairs.KeyPairs[0]);

        internal CopyRightContractContainer.CopyRightContractStub GetCopyRightContractStub(ECKeyPair senderKeyPair)
        {
            return Application.ServiceProvider.GetRequiredService<IContractTesterFactory>()
                .Create<CopyRightContractContainer.CopyRightContractStub>(CopyRightContractAddress,
                    senderKeyPair);
        }
        
        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return Application.ServiceProvider.GetRequiredService<IContractTesterFactory>()
                .Create<TokenContractContainer.TokenContractStub>(TokenContractAddress,
                    senderKeyPair);
        }

        internal TokenContractContainer.TokenContractStub TokenContractStub =>
            GetTokenContractStub(SampleECKeyPairs.KeyPairs[0]);

        protected ECKeyPair AliceKeyPair { get; set; } = SampleECKeyPairs.KeyPairs.Last();

        protected Address AliceAddress => Address.FromPublicKey(AliceKeyPair.PublicKey);

        protected Address TokenContractAddress => GetAddress(TokenSmartContractAddressNameProvider.StringName);

        private Address GetAddress(string contractStringName)
        {
            var addressService = Application.ServiceProvider.GetRequiredService<ISmartContractAddressService>();
            var blockchainService = Application.ServiceProvider.GetRequiredService<IBlockchainService>();
            var chain = AsyncHelper.RunSync(blockchainService.GetChainAsync);
            var chainCont = new ChainContext{
                BlockHash = chain.BestChainHash,
                BlockHeight = chain.BestChainHeight
            };
            var address = AsyncHelper.RunSync
            (() => addressService.GetSmartContractAddressAsync(chainCont, contractStringName));
            return address.SmartContractAddress.Address;
        }
    }
}