using Google.Protobuf.WellKnownTypes;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Contracts.MultiToken;
using AElf.Types;

namespace AElf.Contracts.CopyRightContract
{
    /// <summary>
    /// The C# implementation of the contract defined in copy_right_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>

    public class CopyRightContract : CopyRightContractContainer.CopyRightContractBase
    {
        /// <summary>
        /// The implementation of the Hello method. It takes no parameters and returns on of the custom data types
        /// defined in the protobuf definition file.
        /// </summary>
        /// <param name="input">Empty message (from Protobuf)</param>
        /// <returns>a HelloReturn</returns>
        public override Empty CR_Initial(Empty input)
        {
            Assert(!State.Initialized.Value, "Already initialized.");
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            // Create and issue token of this contract.
            State.Initialized.Value = true;
            return new Empty();
        }
        public override SInt64Value CR_Register(Identity input)
        {
            Assert(State.UserInfo[input.Address] == null , "already registered");
            State.UserInfo[input.Address] = input;
            State.UserState[input.Address] = false;
            return new SInt64Value{Value = 0};            
        }

        public override SInt64Value CR_Login(Identity input)
        {
            Assert(State.UserInfo[input.Address] != null , "not registered");
            Assert( State.UserState[input.Address] == false, "already login");
            State.UserState[input.Address] = true;
            return new SInt64Value{Value = 0};
        }
        
        public override SInt64Value CR_Logout(Identity input)
        {
            Assert(State.UserInfo[input.Address] != null , "not registered");
            Assert( State.UserState[input.Address] == true, "not login");
            State.UserState[input.Address] = false;
            return new SInt64Value{Value = 0};            
        }

        public override SInt64Value CR_Delete(Identity input)
        {           
            Assert(State.UserInfo[input.Address] != null , "not registered");
            State.UserInfo[input.Address] = null;
            State.UserState[input.Address] = false; 
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CR_Upload(UploadData input)
        {
            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[Context.Sender] != null
                    && State.UserState[Context.Sender] == true,"invalid user");

            //验证输入数据（包括Owner是否就是sender，Creator是否存在，Context是否合法（尚未实现），status是否合法）
            Assert(Context.Sender == input.CRTOwner,"invalid owner");
            Assert(input.CRTStatus >= 0 && input.CRTStatus <= 2 , "invalid status");

            //转移手续费
            State.TokenContract.TransferFrom.Send(new TransferFromInput{
                From = Context.Sender,
                To = Context.Self,
                Amount = 10000,
                Symbol = "ELF",
                Memo = "update"
            });
            
            //生成CRT
            State.CopyRightTokenContract.CRT_Create.Send(new AElf.Contracts.CopyRightTokenContract.CreateInput{
                CRTCreator = input.CRTCreator,
                CRTOwner = input.CRTOwner,
                CRTContent = input.CRTContent,
                CRTStatus = input.CRTStatus
            });
            return new SInt64Value{Value = 0};           
        }
        
        //买家发起交易
        public override SInt64Value CR_Transfer(TransferData input)
        {
            //验证发起者账户是否存在且已登陆
            Assert(State.UserInfo[Context.Sender] != null
                    && State.UserState[Context.Sender] == true,"invalid user");
            CopyRightTokenContract.CRT_Info info = State.CopyRightTokenContract.getAllInfo.Call(input.CRTID);

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
            State.CopyRightTokenContract.CRT_ChangeOwner.Send(new AElf.Contracts.CopyRightTokenContract.TransferInput{
                CRTID = input.CRTID,
                Addr = input.Addr
            });
            return new SInt64Value{Value = 0};
        }

        public override Identity Get_User_Info(Address input)
        {
            Assert(State.UserInfo[input] != null , "invalid input");
            return  State.UserInfo[input];
        }
    }
}