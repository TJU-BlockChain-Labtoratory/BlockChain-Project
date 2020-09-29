using AElf;
using AElf.Types;
using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.CopyRightContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public partial class CopyRightContractState : ContractState
    {
        // state definitions go here.
        public SingletonState<bool> Initialized { get; set; }

        public MappedState<Address, CR_Set> CR_Set_Base { get; set; }
    }
}