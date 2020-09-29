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
        public override Empty Initial(Empty input)
        {
            Assert(!State.Initialized.Value, "Already initialized.");
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            
            // Create and issue token of this contract.
            State.Initialized.Value = true;
            return new Empty();
        }
        

        public override BoolValue CR_Upload(UploadData input)
        {
            //验证输入信息的正确性
            

            //验证版权是否上链

            //验证作者信息
            var CRInformation = State.CR_Set_Base[Context.Sender];
            if(CRInformation == null)
                CRInformation = new CR_Set();
           //Assert(CRInformation != null, $"User {Context.Sender} not registered before.");//检验用户是否存在

            //生成上链交易
            //TimeSpan uploadTime = DateTime.Now - new DateTime(1970,1,1,0,0,0,0);//获取系统时间函数
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                Symbol = "ELF",
                Amount = 100,
                From = Context.Sender,
                To = Context.Self,
                Memo = "UpLoad"
            });
            
            CRInformation.CRID.Add(Context.TransactionId);
            //Assert(CRInformation != null, $"User {Context.Sender} not registered before.");//检验用户是否存在

            State.CR_Set_Base[Context.Sender] = CRInformation;
            
            //存储到State中

            return new BoolValue{Value = true};
        }
        
        //买家发起交易
        public override BoolValue CR_Transfer(TransferData input)
        {

            State.TokenContract.TransferFrom.Send( new TransferFromInput {
                Symbol = "ELF", //???
                Amount = input.Price,  // 商定的价格
                From = Context.Sender,
                To = input.DestAddr,
                Memo = "Transfer From Aelf To DestAddr"
            });




            //记录到State中

            return new BoolValue{Value = true};
        }

        public override CR_Set Get_CR(Address input)
        {
            return State.CR_Set_Base[input] ?? new CR_Set();
        }
    }
}