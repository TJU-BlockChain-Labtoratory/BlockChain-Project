using AElf.Kernel.Infrastructure;
using AElf.Kernel.SmartContract;
using AElf.Types;

namespace AElf.Boilerplate.DAppContract
{
    public class CopyRightSmartContractAddressNameProvider : ISmartContractAddressNameProvider
    {
        public static readonly Hash Name = HashHelper.ComputeFrom("AElf.ContractNames.CopyRightContract");

        public static readonly string StringName = Name.ToStorageKey();
        public Hash ContractName => Name;
        public string ContractStringName => StringName;
    }
}