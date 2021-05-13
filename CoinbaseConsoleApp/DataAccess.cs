using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using CoinbasePro.Services.Products.Types;
using System.IO;
using System.Globalization;



namespace CoinbaseConsoleApp
{
    public class DataAccess
    {
        private List<string> TokensToProcess { get; set; }
        private ConnectionString { get { return ConfigurationManager.AppSettings["connectionString"] } }
        public DataAccess(List<string> tokensToProcess)
        {
            this.TokensToProcess = tokensToProcess;
        }


        private  Dictionary<string, string> DBNamesDict = new Dictionary<string, string>()
            {
                { "BtcUsd", "Bitcoin"},
                { "EthUsd", "Ethereum"},
                { "AnkrUsd", "ANKR" },
                { "AdaUsd", "Cardano" },
                { "EosUsd", "EOS" },
                { "LrcUsd", "Loopring" },
                { "MkrUsd", "Maker" },
                { "NknUsd", "NKN" },
                { "NuUsd", "NuCypher" },
                { "OmgUsd", "OmgNetwork" },
                { "OxtUsd", "Orchid" },
                { "RenUsd", "REN" },
                { "XlmUsd", "Stellar" },
                //{ "XrpUsd", "XRP" }
            };

        private List<ProductType> products = new List<ProductType> {
                //ProductType.BtcUsd, 
                //ProductType.EthUsd,
                ProductType.XlmUsd,
                ProductType.BchUsd,
                ProductType.MkrUsd,
                ProductType.EosUsd,
                ProductType.RenUsd,
                ProductType.AdaUsd,
                ProductType.OxtUsd,
                ProductType.OmgUsd,
                ProductType.NknUsd,
                ProductType.LrcUsd,
                ProductType.AnkrUsd,
                ProductType.NuUsd,
               // ProductType.XrpUsd
            };
        private  SqlConnection GetDBConnection()
        {
            string connectionString;
            connectionString = ConfigurationManager.AppSettings["connectionString"];
            SqlConnection conn;

		    conn = new SqlConnection(connectionString);
            return conn;
        }

        private int WriteCandleDataToDB(SqlConnection conn, IList<CoinbasePro.Services.Products.Models.Candle> history, string DBname)
        {
            int recordsWritten = 0;
            conn.Open();
            // History comes newest to oldest, loop backwards to make db idx follow oldest to newest
            for (int i = history.Count - 1; i > 0; i--)
            {
                var candle = history[i];
                var time = candle.Time;
                var high = candle.High;
                var low = candle.Low;
                var open = candle.Open;
                var close = candle.Close;
                var vol = candle.Volume;

                string sql = $"BEGIN IF '{time}' NOT IN(SELECT Time FROM {DBname}) INSERT INTO {DBname} ([Time],[High],[Low],[Open],[Close],[Volume]) VALUES('{time}',{high},{low},{open},{close},{vol}) END";
                   
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                recordsWritten++;
            }
            conn.Close();
            return recordsWritten;
        }

        private string GetMostRecentDataTimestamp(SqlConnection conn, string DBname)
        {
            conn.Open();
            SqlCommand cmd;
            SqlDataReader dreader;
            string sql, output = "";
            sql = $"Select MAX(Time) from {DBname}";
            cmd = new SqlCommand(sql, conn);
            dreader = cmd.ExecuteReader();

            while (dreader.Read())
            {
                output = output + dreader.GetValue(0);
            }
            dreader.Close();
            cmd.Dispose();
            conn.Close();
            return output;
        }


    }
}
