using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.CRContract
{
    public partial class CRContract
    {
        public override Identity Get_User_Info(Address input)
        {
            Assert(State.UserInfo[input] != null , "invalid input");
            return  State.UserInfo[input];
        }
        
        public override BoolValue isApproved(ApproveReqInput input)
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
        
        public override CRT_Pledge_Info getPledgeInfo(Hash input)
        {
            var CRTfetch = State.CRT_Base[input];
            return CRTfetch.PledgeInfo;
        }
        
        public override CRT_List getAllCRTs(Address input)
        {
            return State.CRT_Account[input];
        }

        public override Hash getCRTID( GetCRTIDInput input )
        {
            var list = State.CRT_Account[input.Owner];
            foreach ( Hash one in list.CRTSet )
            {
                var oneCRT = State.CRT_Base[one];
                if ( oneCRT.Info.CRTContent == input.CRTContent )
                {
                    return one;
                }
            }
            return new Hash(); //????如果错误，返回什么???
        }
    }
}