using AElf;
using AElf.Types;
using AElf.Sdk.CSharp.State;
using System.Collections.Generic;

namespace AElf.Contracts.CRContract
{
    /// <summary>
    /// The state class of the contract, it inherits from the AElf.Sdk.CSharp.State.ContractState type. 
    /// </summary>
    public partial class CRContractState : ContractState
    {
        // state definitions go here.
        public SingletonState<bool> Initialized { get; set; }

        public List<Hash> Pledge_CRTID_List = new List<Hash>();

        public MappedState<Address, Identity> UserInfo { get; set; }
        
        public MappedState<Hash,CRT> CRT_Base { get; set; }
        
        public MappedState<Address,CRT_List> CRT_Account { get; set; }
    }
}