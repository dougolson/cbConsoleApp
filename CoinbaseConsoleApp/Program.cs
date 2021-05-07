using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using System;
using CoinbasePro.Services.Products.Types;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;


namespace CoinbaseConsoleApp
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {

            //create an authenticator with your apiKey, apiSecret and passphrase
            var authenticator = new Authenticator(
                ConfigurationManager.AppSettings["apiKey"],
                ConfigurationManager.AppSettings["unsignedSignature"],
                ConfigurationManager.AppSettings["passPhrase"]
        
           );

            //create the CoinbasePro client
            var coinbaseProClient = new CoinbasePro.CoinbaseProClient(authenticator);

            List<ProductType> products = new List<ProductType> {
                ProductType.BtcUsd, 
                ProductType.EthUsd,
                //ProductType.XlmUsd,
                //ProductType.BchUsd,
                //ProductType.MkrUsd,
                //ProductType.EosUsd
            };

            Dictionary<string, string> DBNamesDict = new Dictionary<string, string>()
            {
                { "BtcUsd", "Bitcoin"},
                { "EthUsd", "Ethereum"}
            };

            DateTime endTime = DateTime.UtcNow;
            foreach (var product in products)
            {
                string DBname = DBNamesDict[product.ToString()];

                SqlConnection conn = DataAccess.GetDBConnection();
                string latestTimestamp = DataAccess.GetMostRecentDataTimestamp(conn, DBname);
                DateTime startTime = DateTime.Parse(latestTimestamp);

                var history = await coinbaseProClient.ProductsService.GetHistoricRatesAsync(
                    product, 
                    startTime, 
                    endTime,
                    CandleGranularity.Minutes1
                    );
                // Write to database
                conn = DataAccess.GetDBConnection();
                int recordsWritten = DataAccess.WriteCandleDataToDB(conn, history, DBname);

                // Print basic stats to console
                var current = history[0];
                Console.WriteLine($"Current Stats: {DBname}");
                Console.WriteLine("------------------");
                Console.WriteLine($"High:   {current.High}");
                Console.WriteLine($"Low:    {current.Low}");
                Console.WriteLine($"Open:   {current.Open}");
                Console.WriteLine($"Close:  {current.Close}");
                Console.WriteLine($"Volume: {current.Volume}");
                Console.WriteLine($"{recordsWritten} records written.");
                Console.WriteLine("------------------");
            }
        }
    }
}
