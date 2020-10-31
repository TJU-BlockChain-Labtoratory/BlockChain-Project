using AElf;
using AElf.Types;
using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.CopyRightTokenContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public partial class CopyRightTokenContractState : ContractState
    {
        // state definitions go here.
        public SingletonState<bool> Initialized { get; set; }

        public MappedState<Hash, CRT> CRT_Base { get; set; }
    }
}