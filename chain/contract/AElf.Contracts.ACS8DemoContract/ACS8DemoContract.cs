using System.Linq;
using Acs8;
using AElf.Contracts.TokenConverter;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.ACS8DemoContract
{
    /// <summary>
    /// ACS1 can only used by system contracts.
    /// </summary>
    public class ACS8DemoContract : ACS8DemoContractContainer.ACS8DemoContractBase
    {
        public override Empty BuyResourceToken(BuyResourceTokenInput input)
        {
            if (State.TokenConverterContract.Value == null)
            {
                State.TokenConverterContract.Value =
                    Context.GetContractAddressByName(SmartContractConstants.TokenConverterContractSystemName);
            }

            Assert(
                Context.Variables.GetStringArray(ACS8DemoContractConstants.PayTxFeeSymbolListName)
                    .Contains(input.Symbol), $"{input.Symbol} isn't a valid resource token.");

            State.TokenConverterContract.Buy.Send(new BuyInput
            {
                Symbol = input.Symbol,
                Amount = input.Amount,
                PayLimit = input.PayLimit
            });

            return new Empty();
        }
    }
}