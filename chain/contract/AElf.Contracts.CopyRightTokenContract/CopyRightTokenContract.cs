using Google.Protobuf.WellKnownTypes;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Contracts.MultiToken;
using AElf.Types;

namespace AElf.Contracts.CopyRightTokenContract
{
    /// <summary>
    /// The C# implementation of the contract defined in copy_right_token_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>

    public class CopyRightTokenContract : CopyRightTokenContractContainer.CopyRightTokenContractBase
    {
        /// <summary>
        /// The implementation of the Hello method. It takes no parameters and returns on of the custom data types
        /// defined in the protobuf definition file.
        /// </summary>
        /// <param name="input">Empty message (from Protobuf)</param>
        /// <returns>a HelloReturn</returns>
        public override Hash CRT_Create(CreateInput input)
        {
            //生成CRT_ID
            var HashOfContent = HashHelper.ComputeFrom(input.CRTContent);
            var HashOfCreater = HashHelper.ComputeFrom(input.CRTCreator);
            var HashValue = HashHelper.ConcatAndCompute(HashOfContent,HashOfCreater);
            Assert(State.CRT_Base[HashValue] == null,"already updated!");
            //生成CRT_Info
            var nCRTInfo = new CRT_Info{
                CRTID = HashValue,
                CRTCreator = input.CRTCreator,
                CRTOwner = input.CRTOwner,
                CRTContent = input.CRTContent,
                CRTStatus = input.CRTStatus
            };
            //生成CRT
            var nCRT = new CRT{
                Info = nCRTInfo
            };
            //将CRT记录进State中
            State.CRT_Base[HashValue] = nCRT;
            return HashValue;
        }

        public override SInt64Value CRT_Approve(ApproveInput input)
        {
            var CRTfetch = State.CRT_Base[input.CRTID];
            Assert(CRTfetch != null , "not exist!");
            CRTfetch.CRTApproved.Add(input.Addr);
            State.CRT_Base[input.CRTID] = CRTfetch;
            return new SInt64Value{Value = 0};
        }

        public override SInt64Value CRT_UnApprove(ApproveInput input)
        {
           return new SInt64Value{Value = 0};
        }

        public override SInt64Value CRT_ChangeOwner(TransferInput input)
        {
           return new SInt64Value{Value = 0};
        }

        public override SInt64Value CRT_Destory(Hash input)
        {
           return new SInt64Value{Value = 0};
        }

        public override BoolValue isApproved(ApproveInput input)
        {
            return new BoolValue {Value = true};
        }

        public override BoolValue isAuthorized(AuthorizeInput input)
        {
            return new BoolValue {Value = true};
        }

        public override CRT_Info getAllInfo(Hash input)
        {
            return new CRT_Info();
        }
    }
}