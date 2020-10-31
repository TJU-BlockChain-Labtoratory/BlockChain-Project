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
            return new SInt64Value{Value = 0};            
        }

        public override SInt64Value CR_Login(Identity input)
        {
            return new SInt64Value{Value = 0};
        }
        
        public override SInt64Value CR_Logout(Identity input)
        {
            return new SInt64Value{Value = 0};            
        }

        public override SInt64Value CR_Delete(Identity input)
        {            
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CR_Upload(UploadData input)
        {
            return new SInt64Value{Value = 0};           
        }
        
        //买家发起交易
        public override SInt64Value CR_Transfer(TransferData input)
        {
            return new SInt64Value{Value = 0};
        }

        public override Identity Get_User_Info(Address input)
        {
            return  new Identity();
        }
    }
}