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
        
        

        public int CRT_Pledge(CRT_Pledge_Info input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "CRT not exist!");
            Assert(CRTfetch.Info.CRTStatus == 0 , "has been destoried or pledged");
            //发起者是CRT拥有者或者Approved者
            if ( !(CRTfetch.Info.CRTOwner == Context.Sender || CRTfetch.CRTApproved.Contains(Context.Sender)) )
            {
                return 2;//身份错误码2
            }
            
            //修改pledgeInfo
            CRTfetch.Info.CRTStatus = 1;//一旦出质人检查版权时，会由于status == 1而失败
            CRTfetch.PledgeInfo = input;
            
            State.CRT_Base[input.CRTID] = CRTfetch; //更新CRT的信息
            State.CRT_Account[input.Pledgee].CRTSet.Add(input.CRTID);//将token添加至质权人账户中，不从出质人手中移除。
            
            return 0;
        }

        public int CRT_UnPledge( Hash CRT_ID)
        {
            var CRTfetch = State.CRT_Base[CRT_ID];
            Assert(CRTfetch != null , "CRT not exist!");
            Assert(CRTfetch.Info.CRTStatus == 1 , "not pledged or has been destoried");
            //发起者是CRT拥有者或者Approved者
            if ( !(CRTfetch.Info.CRTOwner == Context.Sender || CRTfetch.CRTApproved.Contains(Context.Sender)) )
            {
                return 2;//身份错误码2
            }
            
            CRTfetch.Info.CRTStatus = 0;
            State.CRT_Account[CRTfetch.PledgeInfo.Pledgee].CRTSet.Remove(CRT_ID);//将token从质权人账户中移除。
            CRTfetch.PledgeInfo = null; //清除掉质押信息
            State.CRT_Base[CRT_ID] = CRTfetch; //更新CRT的信息

            return 0;
        }

        public int CRT_Authorize( Hash CRT_ID, CRT_Authorize_Info info )
        {
            //验证信息
            var CRTfetch = State.CRT_Base[CRT_ID];
            Assert(CRTfetch != null , "CRT not exist!");
            //发起者是CRT拥有者或者Approved者
            if ( !(CRTfetch.Info.CRTOwner == Context.Sender || CRTfetch.CRTApproved.Contains(Context.Sender)) )
            {
                return 2;//身份错误码2
            }

            //将新授权用户加入authorize数组
            CRTfetch.CRTAuthorized.Add( info.Authorized );
            //将新授权信息加入Authorize_Info数组
            CRTfetch.AuthorizeInfo.Add( info );
            
            //to do:对账户添加CRT的信息

            State.CRT_Base[CRT_ID] = CRTfetch;//更新CRT

            return 0;
        }

        public int CRT_ChangeOwner(Hash CRT_ID,Address newOwner)
        {
            var CRTfetch = State.CRT_Base[CRT_ID];
            Assert(CRTfetch != null , "not exist!");            
            var oldOwner = CRTfetch.Info.CRTOwner;
            CRTfetch.Info.CRTOwner = newOwner;
            State.CRT_Base[CRT_ID] = CRTfetch;//更新状态
            State.CRT_Account[newOwner].CRTSet.Add(CRTfetch.Info.CRTID);//将token添加至新所有者账户中
            State.CRT_Account[oldOwner].CRTSet.Remove(CRTfetch.Info.CRTID);//将token从旧所有者账户中移除
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