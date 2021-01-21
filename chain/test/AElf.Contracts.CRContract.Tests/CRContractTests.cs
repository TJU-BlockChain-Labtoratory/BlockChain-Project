using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Types;
using AElf.Kernel.Token;
using AElf.Contracts.MultiToken;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CRContract
{
    public class CRContractTests : CRContractTestBase
    {
        [Fact]
        public async Task Test()
        {
            // Get a stub for testing.
            var stub = GetCRContractStub(SampleAccount.Accounts.First().KeyPair);
            var tokenStub =
                GetTester<TokenContractContainer.TokenContractStub>(
                    GetAddress(TokenSmartContractAddressNameProvider.StringName), SampleAccount.Accounts.First().KeyPair);
            await stub.CR_Register.SendAsync(addr);

            await stub.CR_Login.SendAsync(addr);
            var user = stub.Get_User_Info.CallAsync(addr);
            user.Result.Address.ShouldBe(addr);
            await tokenStub.Transfer.SendAsync(new TransferInput
            {
                To = addr,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });
            
            var test = await stub.CR_Upload.SendAsync(new UploadData
            {
                CRTContent = "ASDF",
                CRTCreator = addr,
                CRTOwner = addr,
                CRTStatus = 0
            });
                
            test.Output.ShouldBe(
                new SInt64Value
                {
                    Value = 0
                });
            // Use CallAsync or SendAsync method of this stub to test.
            // await stub.Hello.SendAsync(new Empty())

            // Or maybe you want to get its return value.
            // var output = (await stub.Hello.SendAsync(new Empty())).Output;

            // Or transaction result.
            // var transactionResult = (await stub.Hello.SendAsync(new Empty())).TransactionResult;
        }
    }
}