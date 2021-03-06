namespace CoinbasePro.Specs.JsonFixtures.Services.Accounts
{
    public static class AccountsHistoryResponseFixture
    {
        public static string Create(string type)
        {
            var json = $@"
[
    {{
        ""id"": ""100"",
        ""created_at"": ""2014-11-07T08:19:27.028459Z"",
        ""amount"": ""0.001"",
        ""balance"": ""239.669"",
        ""type"": ""{type}"",
        ""details"": {{
            ""order_id"": ""d50ec984-77a8-460a-b958-66f114b0de9b"",
            ""trade_id"": ""74"",
            ""product_id"": ""BTC-USD""
        }}
    }}
]";

            return json;
        }
    }
}
