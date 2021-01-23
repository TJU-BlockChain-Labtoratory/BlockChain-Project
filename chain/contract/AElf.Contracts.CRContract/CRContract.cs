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

            //验证额外信息（状态正常、用户是发起者、Approve和Authorized为空(尚未实现)）
            Assert(info.CRTStatus == 0 , "invalid status");
            Assert(Context.Sender == info.CRTOwner , "invalid sender");
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

        
        
        
    }
}