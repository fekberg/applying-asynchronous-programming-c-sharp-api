using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StockAnalyzer.Core.Domain;

namespace StockAnalyzer.Core
{
    public class DataStore
    {
        public static Dictionary<string, IEnumerable<StockPrice>> Stocks 
            = new Dictionary<string, IEnumerable<StockPrice>>();

        private string basePath { get; }

        public DataStore(string basePath)
        {
            this.basePath = basePath;
        }

        public async Task<Dictionary<string, IEnumerable<StockPrice>>> LoadStocks()
        {
            if (Stocks.Any()) return Stocks;

            var prices = await GetStockPrices();

            Stocks = prices
                .GroupBy(x => x.Identifier)
                .ToDictionary(x => x.Key, x => x.AsEnumerable());

            return Stocks;
        }

        private async Task<IList<StockPrice>> GetStockPrices()
        {
            var prices = new List<StockPrice>();

            using (var stream =
                new StreamReader(File.OpenRead(Path.Combine(basePath, @"StockPrices_Small.csv"))))
            {
                await stream.ReadLineAsync(); // Skip headers

                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                {
                    var segments = line.Split(',');

                    for (var i = 0; i < segments.Length; i++) segments[i] = segments[i].Trim('\'', '"');
                    var price = new StockPrice
                    {
                        Identifier = segments[0],
                        TradeDate = DateTime.ParseExact(segments[1], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                        Volume = Convert.ToInt32(segments[6], CultureInfo.InvariantCulture),
                        Change = Convert.ToDecimal(segments[7], CultureInfo.InvariantCulture),
                        ChangePercent = Convert.ToDecimal(segments[8], CultureInfo.InvariantCulture),
                    };
                    prices.Add(price);
                }
            }

            return prices;
        }
    }
}
