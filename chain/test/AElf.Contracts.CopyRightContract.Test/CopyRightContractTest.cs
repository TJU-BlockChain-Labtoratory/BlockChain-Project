using System.Threading.Tasks;
using AElf.Types;
using Google.Protobuf;
using Shouldly;
using Xunit;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TestKit;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CopyRightContract
{
    public class CopyRightContractTest : CopyRightContractTestBase
    {
        private TokenContractContainer.TokenContractStub AliceTokenContractStub => GetTokenContractStub(AliceKeyPair);

        [Fact]
        public async Task CopyRightCall_RegisterTest()
        {
            await CopyRightContractStub.Initial.SendAsync(new Empty());
            await CopyRightContractStub.Register.SendAsync(new Empty());

            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = AliceAddress,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });

            await AliceTokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = CopyRightContractAddress,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });
        }

        [Fact]
        public async Task CopyRightCall_ReturnsCopyRightMessage()
        {
            await CopyRightCall_RegisterTest();
            
            var creater = new Identity{
                Name = "tester"
            };
            var txResult = await CopyRightContractStub.CR_Upload.SendAsync(new UploadData{
                Address = AliceAddress,
                Creater = creater,
                ContentHash = "asdf",
                Flags = 0
            });
            
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            var text  = await CopyRightContractStub.Get_CR.CallAsync(AliceAddress);
            text.CRID[0].ShouldBe(txResult.TransactionResult.TransactionId);

        }
    }
}