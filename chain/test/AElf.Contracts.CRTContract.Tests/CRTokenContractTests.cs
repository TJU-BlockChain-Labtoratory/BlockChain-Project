using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CRTContract
{
    public class CRTokenContractTests : CRTContractTestBase
    {
        [Fact]
        public async Task Test()
        {
            // Get a stub for testing.
            
            var stub = GetCRTContractStub(keyPair);
            var firstCRTHash = await stub.CRT_Create.SendAsync(new CreateInput
            {
                CRTContent = "ASDF",
                CRTCreator = addr,
                CRTOwner = addr,
                CRTStatus = 0
            });

            var firstCRT = await stub.getAllInfo.CallAsync(firstCRTHash.Output);
            firstCRT.CRTOwner.ShouldBe(addr);
            // Use CallAsync or SendAsync method of this stub to test.
            // await stub.Hello.SendAsync(new Empty())

            // Or maybe you want to get its return value.
            // var output = (await stub.Hello.SendAsync(new Empty())).Output;

            // Or transaction result.
            // var transactionResult = (await stub.Hello.SendAsync(new Empty())).TransactionResult;
        }
    }
}