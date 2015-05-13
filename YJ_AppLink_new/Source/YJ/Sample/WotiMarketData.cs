using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Woti;
using Woti.Wmd;

namespace YJ.AppLink
{
    class WotiMarketData
    {
        protected CWormsSession session_;
        private CWormsWmdRecord record_;
        public bool woti_connected = false;
        Dictionary<string, WotiPriceDict> wotiprice_dict = new Dictionary<string, WotiPriceDict>();

        public WotiMarketData()
        {
            session_ = new CWormsSession();
            session_.ConnectionStateChanged += OnConnectionStateChanged_;
            Login("worms4.nyc.tradewex.com", "ANDROMEDA", "SChan", "Dealreview1");
            


            //GetMarketData("GOOG");
            //GetMarketData("INTC");
            //GetMarketData("EXPE");
            //GetMarketData("SPY");
        }

        public void Login(string tradeServer, string firm, string user, string pass)
        {
            session_.LogOn(tradeServer, firm, user, pass, false, "wotiTestApplication");
        }

        private void OnConnectionStateChanged_(WOTI_ConnectionState connectionState, string reason)
        {
            Console.WriteLine("ConnectionState: {0} (1)", connectionState, reason);
            if (connectionState.ToString() == "CS_Connected")
                woti_connected = true;
            else
                woti_connected = false;
           
        }

        /// <summary>
        /// Gets information about a Stock
        /// </summary>
        public void GetMarketData(string symbol)
        {
            if (!wotiprice_dict.ContainsKey(symbol))
            {
                record_ = session_.WmdSession.GetWmdRecord(symbol);
                record_.RecordUpdated -= OnEquityRecordUpdated_;
                record_.RecordUpdated += OnEquityRecordUpdated_;

                wotiprice_dict.Add(symbol, new WotiPriceDict { bid = -1, ask = -1 });
            }
        }

        /// <summary>
        /// Gets information about an Option
        /// </summary>
        public void GetMarketData(string underlier, string root, string expiration, bool isPut, double strike)
        {
            string optionSymbol = session_.StaticDataCache.BuildOptionSymbol(underlier, root,
                    DateTime.Parse(expiration), isPut, strike);
            if (!wotiprice_dict.ContainsKey(optionSymbol))
            {
                record_ = session_.WmdSession.GetWmdRecord(optionSymbol);
                record_.RecordUpdated -= OnOptionRecordUpdated_;
                record_.RecordUpdated += OnOptionRecordUpdated_;

                wotiprice_dict.Add(optionSymbol, new WotiPriceDict { bid = -1, ask = -1 });
            }
        }



        /// <summary>
        /// CWormsRecord.RecordUpdated event handler for equity
        /// </summary>
        private void OnEquityRecordUpdated_(CWormsWmdRecord record, WOTI_WmdEvent eEventType)
        {
            // Add fields below with their EFid's
            Console.WriteLine("Bid price for equity with symbol {0}: {1}", record.Symbol,
                    record.GetDouble(WOTI_EFid.RX_BID));
        }


        /// <summary>
        /// CWormsRecord.RecordUpdated event handler for Option
        /// </summary>
        private void OnOptionRecordUpdated_(CWormsWmdRecord record, WOTI_WmdEvent eEventType)
        {
            // Add fields below with their EFid's
            Console.WriteLine("Bid price for option with symbol {0}: {1}", record.Symbol,
                    record.GetDouble(WOTI_EFid.RX_BID));
            wotiprice_dict[record.Symbol].bid = record.GetDouble(WOTI_EFid.RX_BID);
            wotiprice_dict[record.Symbol].ask = record.GetDouble(WOTI_EFid.RX_ASK);
        }

    }

    class WotiPriceDict
    {
        public string symbol;
        public double bid;
        public double ask;

    }
}
