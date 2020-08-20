using Google.Protobuf.WellKnownTypes;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Types;

namespace AElf.Contracts.CRContract
{
    /// <summary>
    /// The C# implementation of the contract defined in hello_world_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>
    public class CRContract : CRContractContainer.CRContractBase
    {
        /// <summary>
        /// The implementation of the Hello method. It takes no parameters and returns on of the custom data types
        /// defined in the protobuf definition file.
        /// </summary>
        /// <param name="input">Empty message (from Protobuf)</param>
        /// <returns>a HelloReturn</returns>
        public override BoolValue CR_Upload(UploadData input)
        {
            return new BoolValue{Value = true};
        }

        public override BoolValue CR_Transfer(TransferData input)
        {
            return new BoolValue{Value = true};
        }

        public override CR_Set Get_CR(Address input)
        {
            return new CR_Set();
        }
    }
}