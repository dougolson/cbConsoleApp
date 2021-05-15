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
        public Dictionary<string, string> DBTableNamesDict = new Dictionary<string, string>()
        {
            { "AdaUsd", "Cardano" },
            { "AnkrUsd", "ANKR" },
            { "BchUsd", "BitcoinCash" },
            { "BtcUsd", "Bitcoin"},
            { "EosUsd", "EOS" },
            { "EthUsd", "Ethereum"},
            { "LrcUsd", "Loopring" },
            { "MkrUsd", "Maker" },
            { "NknUsd", "NKN" },
            { "NuUsd", "NuCypher" },
            { "OmgUsd", "OmgNetwork" },
            { "OxtUsd", "Orchid" },
            { "RenUsd", "REN" },
            { "XlmUsd", "Stellar" }
            //{ "XrpUsd", "XRP" },
        };
        private SqlConnection GetDBConnection()
        {
            string connectionString;
            connectionString = ConfigurationManager.AppSettings["connectionString"];
            SqlConnection conn;

		    conn = new SqlConnection(connectionString);
            return conn;
        }

        public int WriteCandleDataToDB( IList<CoinbasePro.Services.Products.Models.Candle> history, string tableName)
        {
            SqlConnection conn = GetDBConnection();
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

                string sql = $"BEGIN IF '{time}' NOT IN(SELECT Time FROM {tableName}) INSERT INTO {tableName} ([Time],[High],[Low],[Open],[Close],[Volume]) VALUES('{time}',{high},{low},{open},{close},{vol}) END";
                   
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                recordsWritten++;
            }
            conn.Close();
            return recordsWritten;
        }

        public string GetMostRecentDataTimestamp( string tableName)
        {
            string sql = $"Select MAX(Time) from {tableName}";
            return Query(tableName, sql);
        }

        public string GetHistoricalHigh(string tableName)
        {
            string sql = $"Select MAX(High) from {tableName}";
            return Query(tableName, sql);
        }

        public string Get24HourHigh(string tableName)
        {
            string sql = $"SELECT MAX(High) FROM {tableName} WHERE[Time] >= CONVERT(VARCHAR(20),DATEADD(DAY,-1, GETDATE()), 100)";
            return Query(tableName, sql);
        }
        

        private string Query (string tableName, string sql)
        {
            SqlConnection conn = GetDBConnection();
            conn.Open();
            SqlCommand cmd;
            SqlDataReader reader;
            string output = "";
            cmd = new SqlCommand(sql, conn);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                output = output + reader.GetValue(0);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();
            return output;
        }
    }
}
