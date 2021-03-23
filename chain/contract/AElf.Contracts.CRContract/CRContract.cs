using Google.Protobuf.WellKnownTypes;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using System;
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
            Assert(State.UserInfo[input.CRTOwner] != null,"not registered");
            var user = State.UserInfo[input.CRTOwner];
            Assert(user.Online == true, "not login");
            
            //验证Creator是否均已注册（Owner无需验证，因为Owner必须为Sender，否则会报错）
            Assert(State.UserInfo[input.CRTCreator] != null,"invalid creator");
            
            //验证输入数据（包括Owner是否就是sender，Creator是否存在，Context是否合法（尚未实现），status是否合法）
            Assert(input.CRTStatus >= 0 && input.CRTStatus <= 2 , "invalid status");
            Assert(Context.Sender == input.CRTOwner , "invalid owner");
            
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
            
            //验证输入数据（包括addr是否存在，CRT_ID是否存在，price是否合法）
            Assert(State.UserInfo[input.Addr] != null, "invalid buyer");
            Assert(info != null,"CRT_ID not exist");
            Assert(input.Price > 0 , "invalid price");

            //验证操作权限（发起者是否为Owner或者位于Approve里，同时区块链也在操作权限中)
            Assert(State.CRT_Base[input.CRTID].CRTApproved.Contains(Context.Sender) 
                || Context.Sender == info.CRTOwner, "invalid sender");//质权人不可转让版权，只能等到成为拥有者才行
            Assert(State.CRT_Base[input.CRTID].CRTApproved.Contains(Context.Self)
                    ,"the blockchain is not approved");
            
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
            
            State.CRT_Base[input.CRTID].CRTApproved.Remove(Context.Self);//自动取消对区块链的approve，防止越权
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
            Assert(input.Price > 0 , "invalid price");
            
            //判断Pledgee和Pledger用户存在（如果不存在，则输出输入信息错误码4）
            Assert(State.UserInfo[input.Pledgee] != null,"invalid Pledgee");
            Assert(State.UserInfo[input.Pledger] != null,"invalid Pledger");
            
            //Time_limit是否正常（大于1天）
            var d = input.TimeLimit - Context.CurrentBlockTime;
            Assert(d.Seconds > 86400, "duration too short");

            //验证额外信息（状态正常、用户是发起者、Authorized为空）
            Assert(info.CRTStatus != 2 , "invalid status");
            Assert(Context.Sender == info.CRTOwner , "invalid sender");
            Assert(CRT.CRTAuthorized.Count == 0 ,"CRT is Authorized to other users." );
            Assert(State.CRT_Base[input.CRTID].CRTApproved.Contains(Context.Self)
                ,"the blockchain is not approved");
            
            //合约需要调用PledgeInfo.Pledgee的代币发起交易，需要PledgeInfo.Pledgee提前为合约授权
            State.TokenContract.TransferFrom.Send(new TransferFromInput{
                From = input.Pledgee,
                To = input.Pledger,
                Amount = input.Price,
                Symbol = "ELF",
                Memo = "Pledged"
            });
            
            //修改CRT的pledgeInfo，并将CRT纳入到质权人的账户中
            var txID = Context.TransactionId;//本语句待定是否正确
            var newPledgeInfo = new CRT_Pledge_Info
            {
                CRTID = input.CRTID,
                Pledgee = input.Pledgee, //质权人
                Pledger = input.Pledger, //出质人
                Price = input.Price,//质押价格
                TxID = txID, //质押交易ID
                Notice = "Pledged",//备注
                TimeLimit = input.TimeLimit//质押持续时间，质押结束时间到，如果还没有赎回，则将版权的拥有权从出质人转变为质权人
            };

            var ret = CRT_Pledge(newPledgeInfo);
            State.CRT_Base[input.CRTID].CRTApproved.Remove(Context.Self);//自动取消对区块链的approve，防止越权
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
            
            //验证额外信息（用户是质权人）
            Assert(Context.Sender == CRT.PledgeInfo.Pledgee , "invalid sender");//只有质权人可以主动取消质权
            Assert(State.CRT_Base[CRT_ID].CRTApproved.Contains(Context.Self)
                ,"the blockchain is not approved");

            var ret = CRT_UnPledge(CRT_ID);
            
            State.CRT_Base[CRT_ID].CRTApproved.Remove(Context.Self);//自动取消对区块链的approve，防止越权
            return new SInt64Value{Value = ret};
        }

        public override SInt64Value CR_Approve(ApproveReqInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            
            //获取实际拥有者：owner（未质押）或者pledgee（已质押）
            var owner = CRTfetch.Info.CRTStatus == 1 ? CRTfetch.PledgeInfo.Pledgee : CRTfetch.Info.CRTOwner;
            
            Assert(Context.Sender == owner, "You have no right to approve");
            
            CRTfetch.CRTApproved.Add(input.Addr);
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CR_UnApprove(ApproveReqInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            Assert(CRTfetch.Info.CRTStatus != 2 , "has been destoried");
            
            //获取实际拥有者：owner（未质押）或者pledgee（已质押）
            var owner = CRTfetch.Info.CRTStatus == 1 ? CRTfetch.PledgeInfo.Pledgee : CRTfetch.Info.CRTOwner;
            
            Assert(Context.Sender == owner, "You have no right to approve");
            
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
            
            //获取实际拥有者：owner（未质押）或者pledgee（已质押）
            var owner = info.CRTStatus == 1 ? CRT.PledgeInfo.Pledgee : info.CRTOwner;
            Assert(Context.Sender == owner, "You have no right to approve");

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
            var ret = CRT_Authorize( input.CRTID, input.AuthorizeInfo );

            return new SInt64Value{Value = ret};
        }

        public override SInt64Value Timecheck(Empty input)
        {
            //需要检查时限需要确认身份吗？
            //string s = State.CRT_Base.ToString;
            foreach ( Hash onehash in State.Pledge_CRTID_List )
            {
                var fetchCRT = State.CRT_Base[onehash];
                if( fetchCRT.Info.CRTStatus == 1 ){
                    //获取当前时间
                    //当前时间与 fetchCRT.PledgeInfo.TimeLimit  时间进行对比
                    //如果小于等于，则无需更改
                    //如果大于，则发出取消质押的交易
                }
                    
            }
            
            return new SInt64Value{ Value=0};
        }
        
    }
}