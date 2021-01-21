using AElf.Contracts.MultiToken;
using AElf.Contracts.CRTContract;

namespace AElf.Contracts.CRContract
{
    public partial class CRContractState
    {
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }

        internal CRTContractContainer.CRTContractReferenceState CRTContract { get; set; }
    }
}