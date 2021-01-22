using AElf.Contracts.MultiToken;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using AElf.Types;

namespace AElf.Contracts.CRTContract
{
    public class CRTContract : CRTContractContainer.CRTContractBase
    {
        public override Empty CRT_init(Empty input)
        {
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            return new Empty();
        }
        
        public override Hash CRT_Create(CreateInput input)
        {
            //生成CRT_ID
            var HashOfContent = HashHelper.ComputeFrom(input.CRTContent);
            var HashOfCreater = HashHelper.ComputeFrom(input.CRTCreator);
            var HashValue = HashHelper.ConcatAndCompute(HashOfContent,HashOfCreater);
            //生成CRT_Info
            State.TokenContract.TransferFrom.Send(new TransferFromInput{
                From = input.CRTOwner,
                To = Context.Self,
                Amount = 10000,
                Symbol = "ELF",
                Memo = "update"
            });
            
            var nCRTInfo = new CRT_Info{
                CRTID = HashValue,
                CRTCreator = input.CRTCreator,
                CRTOwner = input.CRTOwner,
                CRTContent = input.CRTContent,
                CRTStatus = input.CRTStatus
            };
            //生成CRT
            var nCRT = new CRT{
                Info = nCRTInfo
            };
            //将CRT记录进State中
            State.CRT_Base[HashValue] = nCRT;
            return HashValue;
        }

        public override SInt64Value CRT_Approve(ApproveInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            CRTfetch.CRTApproved.Add(input.Addr);
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CRT_UnApprove(ApproveInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            Assert(CRTfetch.CRTApproved.Contains(input.Addr) == true,"not already approved");            
            CRTfetch.CRTApproved.Remove(input.Addr);
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CRT_ChangeOwner(TransferInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");            
            CRTfetch.Info.CRTOwner = input.Addr;
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CRT_Destory(Hash input)
        {
            var CRTfetch = State.CRT_Base[input];
            Assert(CRTfetch != null , "not exist!");            
            CRTfetch.Info.CRTStatus = 2;
            State.CRT_Base[input] = CRTfetch;
            State.CRT_Account[CRTfetch.Info.CRTOwner].CRTSet.Remove(CRTfetch);
            return new SInt64Value{Value = 0};
        }

        public override BoolValue isApproved(ApproveInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            if(CRTfetch.CRTApproved.Contains(input.Addr))
                return new BoolValue {Value = true};
            return new BoolValue {Value = false};
        }

        public override BoolValue isAuthorized(AuthorizeInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            if(CRTfetch.CRTAuthorized.Contains(input.Addr))
                return new BoolValue {Value = true};
            return new BoolValue {Value = false};
        }

        public override CRT_Info getAllInfo(Hash input)
        {
            var CRTfetch = State.CRT_Base[input];
            return CRTfetch.Info;
        }
        
        public override CRT_List getAllCRTs(Address input)
        {
            return State.CRT_Account[input];
        }
    }
}