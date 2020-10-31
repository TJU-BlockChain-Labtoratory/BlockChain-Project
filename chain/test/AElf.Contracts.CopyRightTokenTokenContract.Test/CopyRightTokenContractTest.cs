using System.Threading.Tasks;
using AElf.Types;
using Google.Protobuf;
using Shouldly;
using Xunit;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TestKit;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CopyRightTokenTokenContract
{
    public class CopyRightTokenContractTest : CopyRightTokenContractTestBase
    {
        private TokenContractContainer.TokenContractStub AliceTokenContractStub => GetTokenContractStub(AliceKeyPair);

        [Fact]
        public async Task CopyRightTokenCall_RegisterTest()
        {
            await CopyRightTokenContractStub.Initial.SendAsync(new Empty());
            await CopyRightTokenContractStub.Register.SendAsync(new Empty());

            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = AliceAddress,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });

            await AliceTokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = CopyRightTokenContractAddress,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });
        }

        [Fact]
        public async Task CopyRightTokenCall_ReturnsCopyRightTokenMessage()
        {
            await CopyRightTokenCall_RegisterTest();
            
            var creater = new Identity{
                Name = "tester"
            };
            var txResult = await CopyRightTokenContractStub.CR_Upload.SendAsync(new UploadData{
                Address = AliceAddress,
                Creater = creater,
                ContentHash = "asdf",
                Flags = 0
            });
            
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            var text  = await CopyRightTokenContractStub.Get_CR.CallAsync(AliceAddress);
            text.CRID[0].ShouldBe(txResult.TransactionResult.TransactionId);

        }
    }
}