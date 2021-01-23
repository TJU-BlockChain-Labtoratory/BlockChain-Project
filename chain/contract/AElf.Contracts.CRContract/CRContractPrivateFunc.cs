using AElf.Contracts.MultiToken;
using AElf.Types;

namespace AElf.Contracts.CRContract
{
    public partial class CRContract
    {
        private int CRT_Create(Address creator, Address owner , string content , long status)
        {
            //生成CRT_ID
            var HashOfContent = HashHelper.ComputeFrom(creator);
            var HashOfCreater = HashHelper.ComputeFrom(owner);
            var HashValue = HashHelper.ConcatAndCompute(HashOfContent, HashOfCreater);
            
            State.TokenContract.Approve.Send(new ApproveInput{
                Amount = 10000,
                Symbol = "ELF",
                Spender = Context.Self
            });
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                From = owner,
                To = Context.Self,
                Amount = 10000,
                Symbol = "ELF",
                Memo = "update"
            });
            //生成CRT_Info
            var nCRTInfo = new CRT_Info
            {
                CRTID = HashValue,
                CRTCreator = creator,
                CRTOwner = owner,
                CRTContent = content,
                CRTStatus = status
            };
            //生成CRT
            State.CRT_Base[HashValue] = new CRT
            {
                Info = nCRTInfo,
                CRTApproved = { },
                CRTAuthorized = { }
            };
            State.CRT_Account[owner].CRTSet.Add(HashValue);
            return 0;
        }
        
        private int  CRT_Approve(Hash CRT_ID,Address spender)
        {
            var CRTfetch = State.CRT_Base[CRT_ID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            CRTfetch.CRTApproved.Add(spender);
            State.CRT_Base[CRT_ID] = CRTfetch;
            return 0;
        }

        public int CRT_UnApprove(Hash CRT_ID,Address spender)
        {
            var CRTfetch = State.CRT_Base[CRT_ID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            CRTfetch.CRTApproved.Remove(spender);
            State.CRT_Base[CRT_ID] = CRTfetch;
            return 0;
        }

        public int CRT_ChangeOwner(Hash CRT_ID,Address newOwner)
        {
            var CRTfetch = State.CRT_Base[CRT_ID];
            Assert(CRTfetch != null , "not exist!");            
            var oldOwner = CRTfetch.Info.CRTOwner;
            CRTfetch.Info.CRTOwner = newOwner;
            State.CRT_Base[CRT_ID] = CRTfetch;
            State.CRT_Account[newOwner].CRTSet.Add(CRTfetch.Info.CRTID);
            State.CRT_Account[oldOwner].CRTSet.Remove(CRTfetch.Info.CRTID);
            return 0;
        }

        public int CRT_Destory(Hash input)
        {
            var CRTfetch = State.CRT_Base[input];
            Assert(CRTfetch != null , "not exist!");            
            CRTfetch.Info.CRTStatus = 2;
            State.CRT_Base[input] = CRTfetch;
            State.CRT_Account[CRTfetch.Info.CRTOwner].CRTSet.Remove(CRTfetch.Info.CRTID);
            return 0;
        }

    }
}