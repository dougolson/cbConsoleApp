using System.Runtime.Serialization;

namespace CoinbasePro.Services.Accounts.Types
{
    public enum AccountEntryType
    {
        [EnumMember(Value = "Unknown")]
        Unknown,
        [EnumMember(Value = "transfer")]
        Transfer,
        [EnumMember(Value = "match")]
        Match,
        [EnumMember(Value = "fee")]
        Fee,
        [EnumMember(Value = "rebate")]
        Rebate,
        [EnumMember(Value = "conversion")]
        Conversion
    }
}
