using Google.Protobuf.WellKnownTypes;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Contracts.MultiToken;
using AElf.Types;

namespace AElf.Contracts.CRContract
{
    public partial class CRContract : CRContractContainer.CRContractBase
    {
        private void CR_Initial()
        {
            Assert(!State.Initialized.Value, "Already initialized.");
            var hash = HashHelper.ComputeFrom("AElf.ContractNames.CRToken");
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);

            // Create and issue token of this contract.
            State.Initialized.Value = true;
        }
        public override SInt64Value CR_Register(Address input)
        {
            if (State.Initialized.Value == false)
                CR_Initial();
            
            Assert(State.UserInfo[input] == null , "already registered");
            var user = new Identity
            {
                Address = input,
                Online = false
            };
            State.UserInfo[input] = user;
            State.CRT_Account[input] = new CRT_List();
            return new SInt64Value{Value = 0};            
        }

        public override SInt64Value CR_Login(Address input)
        {
            if (State.Initialized.Value == false)
                CR_Initial();
            Assert(State.UserInfo[input] != null , "not registered");
            var user = State.UserInfo[input];
            Assert( user.Online == false, "already login");
            user.Online = true;
            State.UserInfo[input] = user;
            return new SInt64Value{Value = 0};
        }
        
        public override SInt64Value CR_Logout(Address input)
        {
            Assert(State.UserInfo[input] != null , "not registered");
            var user = State.UserInfo[input];
            Assert( user.Online == true, "not login");
            user.Online = false;
            State.UserInfo[input] = user;
            return new SInt64Value{Value = 0};            
        }

        public override SInt64Value CR_Delete(Address input)
        {           
            Assert(State.UserInfo[input] != null , "not registered");
            State.UserInfo[input] = null;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CR_Upload(UploadData input)
        {
            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[input.CRTOwner] != null,"invalid user");
            var user = State.UserInfo[input.CRTOwner];
            Assert(user.Online == true, "not login");
            //验证输入数据（包括Owner是否就是sender，Creator是否存在，Context是否合法（尚未实现），status是否合法）
            Assert(input.CRTStatus >= 0 && input.CRTStatus <= 2 , "invalid status");
            Assert(Context.Sender == input.CRTOwner , "invalid sender");
            //转移手续费与生成CRT
            var ret = CRT_Create(input.CRTCreator, input.CRTOwner, input.CRTContent, input.CRTStatus);

            return new SInt64Value{Value = ret};           
        }
        
        //买家发起交易
        public override SInt64Value CR_Transfer(TransferData input)
        {
            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[Context.Sender] != null,"invalid user");
            var user = State.UserInfo[Context.Sender];
            Assert(user.Online == true, "not login");
            var info = State.CRT_Base[input.CRTID].Info;

            //验证输入数据（包括addr是否存在（尚未实现），CRT_ID是否存在，price是否合法）
            Assert(info != null,"CRT_ID not exist");
            Assert(input.Price > 0 , "invalid price");

            //验证操作权限（发起者是否为Owner或者位于Approve里，同时区块链也在操作权限中)
            Assert(State.CRT_Base[input.CRTID].CRTApproved.Contains(Context.Sender) 
                || Context.Sender == info.CRTOwner, "invalid sender");
            
            //验证额外信息（状态正常、用户是发起者、Authorized为空）
            Assert(info.CRTStatus == 0 , "invalid status");
            Assert(State.CRT_Base[input.CRTID].CRTAuthorized.Count == 0 ,"CRT is Authorized to other users." );
            
            
            //转移手续费
            State.TokenContract.TransferFrom.Send(new TransferFromInput{
                From = input.Addr,
                To = Context.Sender,
                Amount = input.Price,
                Symbol = "ELF",
                Memo = "transfer"
            });
            
            //改变CRT所有者
            var ret = CRT_ChangeOwner(input.CRTID, input.Addr);
            
            return new SInt64Value{Value = ret};
        }
        
        public override SInt64Value CR_Pledge(PledgeData input){

            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[Context.Sender] != null,"invalid user");
            var user = State.UserInfo[Context.Sender];
            Assert(user.Online == true, "not login");
            var CRT = State.CRT_Base[input.CRTID];
            var info = CRT.Info;

            //验证输入数据（包括CRT_Pledge_Info.pledgee和CRT_Pledge_Info.Pledger是否存在（尚未实现），CRT_ID是否存在，price是否合法）
            Assert(info != null,"CRT_ID not exist");//验证CRT_ID是否存在且处于可以被质押的状态
            Assert(info.CRTStatus == 0, "CRT_ID status error"); //如何返回错误码3
            Assert(input.PledgeInfo.Price > 0 , "invalid price");
            
            //判断Pledgee和Pledger用户存在（如果不存在，则输出输入信息错误码4）
            Assert(State.UserInfo[input.PledgeInfo.Pledgee] != null,"invalid Pledgee");
            Assert(State.UserInfo[input.PledgeInfo.Pledger] != null,"invalid Pledger");
            //Time_limit是否正常
            

            //验证额外信息（状态正常、用户是发起者、Authorized为空）
            Assert(info.CRTStatus != 2 , "invalid status");
            Assert(Context.Sender == info.CRTOwner , "invalid sender");
            Assert(CRT.CRTAuthorized.Count == 0 ,"CRT is Authorized to other users." );
            
            //合约需要调用PledgeInfo.Pledgee的代币发起交易，需要PledgeInfo.Pledgee提前为合约授权
            State.TokenContract.TransferFrom.Send(new TransferFromInput{
                From = input.PledgeInfo.Pledgee,
                To = input.PledgeInfo.Pledger,
                Amount = input.PledgeInfo.Price,
                Symbol = "ELF",
                Memo = "Pledged"
            });
            
            //修改CRT的pledgeInfo，并将CRT纳入到质权人的账户中
            var txID = Context.TransactionId;//本语句待定是否正确
            var newPledgeInfo = input.PledgeInfo;
            newPledgeInfo.TxID = txID;
            
            var ret = CRT_Pledge(input);

            return new SInt64Value{Value = ret};
        }

        public override SInt64Value CR_UnPledge(Hash CRT_ID)
        {
            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[Context.Sender] != null,"invalid user");
            var user = State.UserInfo[Context.Sender];
            Assert(user.Online == true, "not login");
            var CRT = State.CRT_Base[CRT_ID];
            var info = CRT.Info;
            
            //验证输入数据（CRT_ID是否存在）
            Assert(info != null,"CRT_ID not exist");//验证CRT_ID是否存在且处于可以被质押的状态
            Assert(info.CRTStatus == 1, "CRT_ID is not Pledged"); //如何返回错误码3
            
            //验证额外信息（用户是发起者）
            Assert(Context.Sender == info.CRTOwner , "invalid sender");
            
            State.TokenContract.Approve.Send( new ApproveInput{
                Amount = CRT.PledgeInfo.Price,
                Symbol = "ELF",
                Spender = Context.Self
            } );

            //检查输入上面交易是否完成（未完成）
            //如果完成获取交易ID（未完成）

            var ret = CRT_UnPledge(CRT_ID);
            
            return new SInt64Value{Value = ret};
        }

        public override SInt64Value CR_Approve(ApproveReqInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            CRTfetch.CRTApproved.Add(input.Addr);
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CR_UnApprove(ApproveReqInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            CRTfetch.CRTApproved.Remove(input.Addr);
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }
        

        public override SInt64Value CR_Authorize( AuthorizeData input )
        {
            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[Context.Sender] != null,"invalid user");
            var user = State.UserInfo[Context.Sender];
            Assert(user.Online == true, "not login");
            var CRT = State.CRT_Base[input.CRTID];
            var info = CRT.Info;

            //验证输入数据（包括CRT_Pledge_Info.pledgee和CRT_Pledge_Info.Pledger是否存在（尚未实现），CRT_ID是否存在，price是否合法）
            Assert(info != null,"CRT_ID not exist");//验证CRT_ID是否存在且处于可以被质押的状态
            Assert(info.CRTStatus == 0, "CRT_ID status error"); //如何返回错误码3
            Assert(input.AuthorizeInfo.Price > 0 , "invalid price");
            //判断Pledgee和Pledger用户存在（如果不存在，则输出输入信息错误码4）
            Assert(State.UserInfo[input.AuthorizeInfo.Authorized] != null,"invalid authorized person");
            //Time_limit是否正常

            //验证额外信息（状态正常、用户是发起者、Approve和Authorized为空(尚未实现)）//Approve不用为空吧，只需要Authorized为空就行
            Assert(info.CRTStatus == 2 , "invalid status");
            Assert(Context.Sender == info.CRTOwner , "invalid sender");
            
            //为合约本身授权（以下直到UnApprove，都是原子操作）
            CRT_Approve( input.CRTID, Context.Self );

            //合约需要调用PledgeInfo.Pledgee的代币发起交易，需要PledgeInfo.Pledgee提前为合约授权
            State.TokenContract.TransferFrom.Send(new TransferFromInput{
                From = input.AuthorizeInfo.Authorized,
                To = info.CRTOwner,
                Amount = input.AuthorizeInfo.Price,
                Symbol = "ELF",
                Memo = "Authorize"
            });

            //如果完成获取交易ID（未完成）
            var txID = Context.TransactionId;//本语句待定是否正确
            var newAuthorizeInfo = input.AuthorizeInfo;
            newAuthorizeInfo.TxID = txID;
            var updateAuthorizeInfo = CRT.CRTAuthorized;
            //to do


            var ret = 0;
            return new SInt64Value{Value = ret};
        }

        
    }
}