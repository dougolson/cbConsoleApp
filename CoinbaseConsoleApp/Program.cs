using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using System;
using CoinbasePro.Services.Products.Types;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;


namespace CoinbaseConsoleApp
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            List<string> tokensToProcess = new List<string>
            {
                "AdaUsd",
                "AnkrUsd",
                "BchUsd",
                "BtcUsd",
                "EosUsd",
                "EthUsd",
                "LrcUsd",
                "MkrUsd",
                "NknUsd",
                "NuUsd",
                "OmgUsd",
                "OxtUsd",
                "RenUsd",
                "XlmUsd",
            };
            

            //create an authenticator with your apiKey, apiSecret and passphrase
            var authenticator = new Authenticator(
                ConfigurationManager.AppSettings["apiKey"],
                ConfigurationManager.AppSettings["unsignedSignature"],
                ConfigurationManager.AppSettings["passPhrase"]
        
           );

            //create the CoinbasePro client
            var coinbaseProClient = new CoinbasePro.CoinbaseProClient(authenticator);
            /*
            var accounts = await coinbaseProClient.AccountsService.GetAllAccountsAsync();
            List<string> accountIDs = new List<string>();
            foreach(var account in accounts)
            {
                if (account.Balance > 0)
                {
                    accountIDs.Add(account.Id.ToString());
                }
            }

            foreach(var accountId in accountIDs)
            {
                var accountInfo = await coinbaseProClient.AccountsService.GetAccountHistoryAsync(accountId, 1000, 10);
                var data = accountInfo[0][0];
                Console.WriteLine($"Amount: {data.Amount}\tBalance: {data.Balance}\tDate: {data.CreatedAt}\tProduct: {data.Details.ProductId}");
            }
            Console.ReadKey();
 */
            DateTime endTime = DateTime.UtcNow;
            DataAccess dataAccess = new DataAccess();

            foreach (var token in tokensToProcess)
            {
                string tableName = dataAccess.DBTableNamesDict[token];
                string historicalHigh = dataAccess.GetHistoricalHigh(tableName);
                string lastDayHigh = dataAccess.Get24HourHigh(tableName);
                string latestTimestamp = dataAccess.GetMostRecentDataTimestamp(tableName);
                ProductType product = (ProductType)Enum.Parse(typeof(ProductType), token);
                //DateTime startTime = new DateTime(2021,1,1);
                if (!DateTime.TryParse(latestTimestamp, out DateTime startTime))
                {
                    startTime = DateTime.Now;
                }

                var history = await coinbaseProClient.ProductsService.GetHistoricRatesAsync(
                    product, 
                    startTime, 
                    endTime,
                    CandleGranularity.Minutes1
                    );
                // Write to database

                int recordsWritten = dataAccess.WriteCandleDataToDB(history, tableName);

                // Print basic stats to console
                var current = history[0];
                Console.WriteLine($"Current Stats: {tableName}");
                Console.WriteLine("------------------");
                Console.WriteLine($"Db Historical High: {historicalHigh}");
                Console.WriteLine($"24 Hour High: {lastDayHigh}");
                Console.WriteLine($"24 Hour % Change: {100 * (current.High / StringToDecimal(lastDayHigh) -1):F}%");
                Console.WriteLine($"High:   {current.High}");
                Console.WriteLine($"Low:    {current.Low}");
                Console.WriteLine($"Open:   {current.Open}");
                Console.WriteLine($"Close:  {current.Close}");
                Console.WriteLine($"Volume: {current.Volume}");
                Console.WriteLine($"{recordsWritten} records written.");
                Console.WriteLine("------------------");
            }
            
            decimal StringToDecimal(string num)
            {
                if (!decimal.TryParse(num, out decimal number))
                {
                    return 1;
                }
                return number;
            }

        }
    }
}

