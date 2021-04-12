using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Types;
using AElf.Kernel.Token;
using AElf.Contracts.MultiToken;
using TExtensions = AElf.CSharp.Core.Extension.TimestampExtensions;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CRContract
{
    public class CRContractTests : CRContractTestBase
    {

        [Fact]
        public async Task Init()
        {
            await AliceCRContractStub.CR_Register.SendAsync(AliceAddress);
            await BobCRContractStub.CR_Login.SendAsync(AliceAddress);
            
            await AliceCRContractStub.CR_Register.SendAsync(BobAddress);
            await BobCRContractStub.CR_Login.SendAsync(BobAddress);
            
            var Auser = AliceCRContractStub.Get_User_Info.CallAsync(AliceAddress);
            Auser.Result.Address.ShouldBe(AliceAddress);
            
            var Buser = BobCRContractStub.Get_User_Info.CallAsync(BobAddress);
            Buser.Result.Address.ShouldBe(BobAddress);
            
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = AliceAddress,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });
            var Abalance = await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = AliceAddress,
                Symbol = "ELF"
            });
            Abalance.Balance.ShouldBe(100000000_00000000);
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = BobAddress,
                Symbol = "ELF",
                Amount = 100000000_00000000
            });
            var Bbalance = await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = BobAddress,
                Symbol = "ELF"
            });
            Bbalance.Balance.ShouldBe(100000000_00000000);
        }
        
        [Fact]
        public async Task UpLoad()
        {
            await Init();
            await AliceTokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Amount = 10000,
                Spender = CRContractAddress,
                Symbol = "ELF"
            });
            var test = await AliceCRContractStub.CR_Upload.SendAsync(new UploadData
            {
                CRTContent = "ASDF",
                CRTCreator = AliceAddress,
                CRTOwner = AliceAddress,
                CRTStatus = 0
            });
            var allowence = await AliceTokenContractStub.GetAllowance.CallAsync(new GetAllowanceInput
            {
                Owner = AliceAddress,
                Spender = CRContractAddress,
                Symbol = "ELF"
            });
            test.Output.ShouldBe(new SInt64Value {
                    Value = 0
            });

            var ret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            var hashcode = ret.CRTSet.First();
            var result = await CRContractStub.getAllInfo.CallAsync(hashcode);
            result.CRTContent.ShouldBe("ASDF");
        }

        [Fact]
        public async Task Transfer()
        {
            await UpLoad();
            await BobTokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Amount = 1000000,
                Spender = CRContractAddress,
                Symbol = "ELF"
            });
            var Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            var Ahashcode = Aret.CRTSet.First();
            var test = await AliceCRContractStub.CR_Transfer.SendAsync(new TransferData
            {
                Addr = BobAddress,
                CRTID = Ahashcode,
                Price = 1000000
            });
            test.Output.ShouldBe(new SInt64Value {
                Value = 0
            });
            var Bret = await CRContractStub.getAllCRTs.CallAsync(BobAddress);
            var Bhashcode = Bret.CRTSet.First();
            var result = await CRContractStub.getAllInfo.CallAsync(Bhashcode);
            result.CRTContent.ShouldBe("ASDF");
            
        }
        
        [Fact]
        public async Task Pledge()
        {
            await UpLoad();
            await BobTokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Amount = 1000000,
                Spender = CRContractAddress,
                Symbol = "ELF"
            });
            var Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            var Ahashcode = Aret.CRTSet.First();
            var dt = DateTime.Now;
            var currTime = Timestamp.FromDateTime(dt);
            var limit =  TExtensions.AddMinutes(currTime , 2);
            var test = await AliceCRContractStub.CR_Pledge.SendAsync(new PledgeData
            {
                Pledgee = BobAddress,
                Pledger = AliceAddress,
                Price = 2000,
                CRTID = Ahashcode,
                TimeLimit = limit,
                Notice = "hello"
            });
            test.Output.ShouldBe(new SInt64Value
            {
                Value = 0
            });

            var Bret = await CRContractStub.getAllCRTs.CallAsync(BobAddress);
            var Bhashcode = Bret.CRTSet.First();
            var result = await CRContractStub.getPledgeInfo.CallAsync(Bhashcode);
            result.Pledger.ShouldBe(AliceAddress);
            var r = await CRContractStub.getAllInfo.CallAsync(Bhashcode);
            r.CRTStatus.ShouldBe(1);
        }
        
        [Fact]
        public async Task UnPledge()
        {
            await Pledge();
            var Bret = await CRContractStub.getAllCRTs.CallAsync(BobAddress);
            var Bhashcode = Bret.CRTSet.First();
            var test = await AliceCRContractStub.CR_UnPledge.SendAsync(Bhashcode);
            test.Output.ShouldBe(new SInt64Value
            {
                Value = 0
            });

            Bret = await CRContractStub.getAllCRTs.CallAsync(BobAddress);
            Bret.CRTSet.Count.ShouldBe(0);
            
            var Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            var Ahashcode = Aret.CRTSet.First();
            var result = await CRContractStub.getPledgeInfo.CallAsync(Ahashcode);
            result.ShouldBe(null);
            var r = await CRContractStub.getAllInfo.CallAsync(Ahashcode);
            r.CRTStatus.ShouldBe(0);
        }
        [Fact]
        public async Task Approve()
        {
            await UpLoad();
            var Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            var Ahashcode = Aret.CRTSet.First();
            var test = await AliceCRContractStub.CR_Approve.SendAsync(new ApproveReqInput
            {
                Addr = BobAddress,
                CRTID = Ahashcode
            });
            test.Output.ShouldBe(new SInt64Value
            {
                Value = 0
            });
            
            
            Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            Ahashcode = Aret.CRTSet.First();
            var result = await CRContractStub.isApproved.CallAsync(new ApproveReqInput
            {
                Addr = BobAddress,
                CRTID = Ahashcode
            });
            result.ShouldBe(new BoolValue{Value = true});
            
        }
        
        [Fact]
        public async Task UnApprove()
        {
            await UpLoad();
            var Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            var Ahashcode = Aret.CRTSet.First();
            var test = await AliceCRContractStub.CR_UnApprove.SendAsync(new ApproveReqInput
            {
                Addr = BobAddress,
                CRTID = Ahashcode
            });
            test.Output.ShouldBe(new SInt64Value
            {
                Value = 0
            });
            
            
            Aret = await CRContractStub.getAllCRTs.CallAsync(AliceAddress);
            Ahashcode = Aret.CRTSet.First();
            var result = await CRContractStub.isApproved.CallAsync(new ApproveReqInput
            {
                Addr = BobAddress,
                CRTID = Ahashcode
            });
            result.ShouldBe(new BoolValue{Value = false});
            
        }
    }
}