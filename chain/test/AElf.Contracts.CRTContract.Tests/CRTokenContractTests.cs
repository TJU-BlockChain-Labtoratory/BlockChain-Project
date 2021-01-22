using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Kernel.Token;
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
            
            await stub.CRT_init.SendAsync(new Empty());
            await tokenStub.Transfer.SendAsync(new MultiToken.TransferInput
            {
                To = addr,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });
            var balance = await tokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = addr,
                Symbol = "ELF"
            });
            balance.Balance.ShouldBe(100000000_00000000);
            await userTokenStub.Approve.SendAsync(new MultiToken.ApproveInput
            {
                Amount = 10000,
                Spender = CRTContractAddress,
                Symbol = "ELF"
            });
            var allowence = await userTokenStub.GetAllowance.CallAsync(new GetAllowanceInput
            {
                Owner = addr,
                Spender = CRTContractAddress,
                Symbol = "ELF"
            });

            allowence.Spender.ShouldBe(CRTContractAddress);
            allowence.Allowance.ShouldBe(10000);
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