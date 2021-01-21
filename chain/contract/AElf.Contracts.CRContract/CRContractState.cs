using AElf;
using AElf.Types;
using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.CRContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public partial class CRContractState : ContractState
    {
        // state definitions go here.
        public SingletonState<bool> Initialized { get; set; }

        public MappedState<Address, Identity> UserInfo { get; set; }
        
    }
}