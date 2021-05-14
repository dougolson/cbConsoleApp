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
/*
            DataAccess dataAccess = new DataAccess();
            DateTime endTime = DateTime.UtcNow;
            foreach (var token in tokensToProcess)
            {
                string DbName = dataAccess.DBNamesDict[token];
                ProductType product = (ProductType)Enum.Parse(typeof(ProductType), token);
                string latestTimestamp = dataAccess.GetMostRecentDataTimestamp(DbName);
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
               
                int recordsWritten = dataAccess.WriteCandleDataToDB(history, DbName);

                // Print basic stats to console
                var current = history[0];
                Console.WriteLine($"Current Stats: {DbName}");
                Console.WriteLine("------------------");
                Console.WriteLine($"High:   {current.High}");
                Console.WriteLine($"Low:    {current.Low}");
                Console.WriteLine($"Open:   {current.Open}");
                Console.WriteLine($"Close:  {current.Close}");
                Console.WriteLine($"Volume: {current.Volume}");
                Console.WriteLine($"{recordsWritten} records written.");
                Console.WriteLine("------------------");
            }
*/
        }
    }
}

