using System.Collections.Generic;
using AElf.Contracts.CopyRightContract;
using AElf.Kernel.SmartContractInitialization;
using AElf.Types;
using Google.Protobuf;

namespace AElf.Boilerplate.DAppContract
{
    public class CopyRightContractInitializationProvider : IContractInitializationProvider
    {
        public List<InitializeMethod> GetInitializeMethodList(byte[] contractCode)
        {
            return new List<InitializeMethod>
            {
                new InitializeMethod
                {
                    MethodName = nameof(CopyRightContractContainer.CopyRightContractStub.CR_Initial),
                    Params = ByteString.Empty
                }
            };
        }

        public Hash SystemSmartContractName => CopyRightSmartContractAddressNameProvider.Name;
        public string ContractCodeName => "AElf.Contracts.CopyRightContract";
    }
}