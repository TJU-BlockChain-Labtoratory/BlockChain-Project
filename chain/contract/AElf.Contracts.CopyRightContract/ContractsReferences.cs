using AElf.Contracts.MultiToken;
using AElf.Contracts.CopyRightTokenContract;

namespace AElf.Contracts.CopyRightContract
{
    public partial class CopyRightContractState
    {
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }

        internal CopyRightTokenContractContainer.CopyRightTokenContractReferenceState CopyRightTokenContract { get; set; }
    }
}