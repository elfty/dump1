using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    public class pricesrvfields
    {
        public const int SERVICEID_PRICE = 2;

        // US options
        public const string PRICESRV_SUB_OPTION = "OQ";
        public const string PRICESRV_SUB_OPTION_DELAYED = "DOQ";
        public const string PRICESRV_SUB_OPTION_SERIES = "OQS";
        public const string PRICESRV_SUB_OPTION_SERIES_DELAYED = "DOQS";
        public const string PRICESRV_SUB_OPTION_MONTH = "OQM";
        public const string PRICESRV_SUB_OPTION_MONTH_DELAYED = "DOQM";


        // Futures
        public const string PRICESRV_SUB_FUTURE_DELAYED = "DFQ";

        public const string PRICESRV_SUB_STOCK = "SQ";
        public const string PRICESRV_SUB_STOCK_DELAYED = "DSQ";

        public const string PRICESRV_SUB_ARCA = "SQA";
        public const string PRICESRV_SUB_ARCA_DELAYED = "DSQA";
        public const string PRICESRV_SUB_ARCA_MM = "AQ";

        public const string PRICESRV_SUB_INET = "SQI";
        public const string PRICESRV_SUB_INET_DELAYED = "DSQI";
        public const string PRICESRV_SUB_INET_MM = "IQ";

        public const string PRICESRV_SUB_TEST = "SQTEST";
        public const string PRICESRV_SUB_TEST_DELAYED = "DSQTEST";
        public const string PRICESRV_SUB_TEST_MM = "TESTQ";

        public const string PRICESRV_SUB_SPREAD = "CSB";   //  To use ISE change it to "ISB"


        //
        // Partial update price feed field name consts.
        //
        public const string FLD_SYMBOL = "1";
        public const string FLD_BID = "2";
        public const string FLD_ASK = "3";
        public const string FLD_LAST = "4";
        public const string FLD_BIDSIZE = "5";
        public const string FLD_ASKSIZE = "6";
        public const string FLD_LASTSIZE = "7";
        public const string FLD_OPEN = "8";
        public const string FLD_HIGH = "9";
        public const string FLD_LOW = "10";
        public const string FLD_CLOSE = "11";
        public const string FLD_VOLUME = "12";

        // The primary exchange that this item trades on.		
        public const string FLD_PRIMEXCH = "13";
        public const string FLD_BIDEXCH = "14";
        public const string FLD_ASKEXCH = "15";
        public const string FLD_LASTEXCH = "16";

        // The direction of the price
        public const string FLD_TICK = "17";
        public const string FLD_LSEQ = "18";
        public const string FLD_DESCRIPTION = "19";

        // The quote diposition
        public const string FLD_DISPOSITION = "20";

        // Bid/Ask or blank if its a book order. 
        public const string FLD_BOOKORDER = "21";

        // Set to true if this quote should be processed.
        public const string FLD_VALID = "22";
        public const string FLD_UNDERLYING = "23";
        public const string FLD_UNDERLYINGDESC = "24";

        // Only present in options and futures
        public const string FLD_STRIKE = "25";
        public const string FLD_OPTION_TYPE = "26";
        public const string FLD_EXPIRY = "27";
        public const string FLD_INITIAL = "28";


        public const string FLD_SOURCE = "29";
        public const string FLD_MM = "30";

        // The side for depth quotes. 0=Bid, 1=Ask
        public const string FLD_SIDE = "31";

        // The action to take w/ a depth quote.  See FLD_ACTION_VALUE below. 
        public const string FLD_ACTION = "32";

        // The price on a depth quote
        public const string FLD_PRICE = "33";

        // The size of a depth quote.
        public const string FLD_SIZE = "34";

        // Not used:
        public const string FLD_SEQUENCE = "35";

        // The unique feed order id for this depth quote.
        public const string FLD_FEED_ORDERID = "36";

        // Used for series reads to show when a series read has started and stopped.
        public const string FLD_STREAM_START = "37";
        public const string FLD_STREAM_END = "38";

        // The option opra code.  This is only present if any user is doing a series db
        // read.
        public const string FLD_OPRACODE = "39";

        // The future or option multiplier
        public const string FLD_MULTIPLIER = "40";

        // The tick range blob.  Example Tick ranges:
        // R1, [0.0], [3.0] (uses a double array of size 2)
        // V1, 0.05
        // R2, [3.0], [9999999.0]
        // V2, 0.10
        public const string FLD_TICK_RANGE = "41";

        // Available ETB shares.  This is only present on the stock initial update.
        public const string FLD_ETB_AVAILABLE_SHARES = "42";

        // Expiry month array.  This will be present on the first option update from 
        // a by month option subscription.
        public const string FLD_EXPIRY_MONTHS = "43";

        //
        // The number of spread legs on a spread price update.
        //
        public const string FLD_SPREAD_LEGS = "44";

        //
        // The ratio on a spread price update.
        //
        public const string FLD_SPREAD_RATIO = "45";

        //
        //  The exchange code.
        //
        public const string FLD_EXCH = "46";

        //
        //  The institution type
        //
        public const string FLD_INSTTYPE = "47";

        //
        // The order type.
        //
        public const string FLD_ORDTYPE = "48";

        //
        // The time in force.
        //
        public const string FLD_TIF = "54";

        //
        // Depth quote action fields.
        //
        public const uint FLD_ACTION_VALUE_DELETE = 1;
        public const uint FLD_ACTION_VALUE_NORMAL = 0;

        //////////////////////////////////////////////////////////////////////
        // Price feed status messages.
        //
        // These are sent out by the price servers to indicate if the price server is running or not.
        //
        // Subscriptions are in the format of:
        // PS.<part>.STATUS
        //////////////////////////////////////////////////////////////////////

        // The current feed status.  See PRICESRV_STATUS_FLD_STATUS_VALUE for values.
        public const string PRICESRV_STATUS_FLD_FEEDSTATUS = "1";

        // Unique feedid of the feed.
        public const string PRICESRV_STATUS_FLD_FEEDID = "2";

        // Long description of the feed.
        public const string PRICESRV_STATUS_FLD_DESCRIPTION = "3";

        // Time in milliseconds since the last time the feed processed an update.
        public const string PRICESRV_STATUS_FLD_LASTUPDATE = "4";

        // The current feed download status.  See PRICESRV_STATUS_FLD_DOWNLOADSTATUS_VALUE for values. 
        public const string PRICESRV_STATUS_FLD_DOWNLOADSTATUS = "5";

        // The current config (ini file) used.
        public const string PRICESRV_STATUS_FLD_CONFIG = "6";

        //Futures feild names
        public const string FLD_FUTURES_INITIAL = "initial";
        public const string FLD_FUTURES_EXPIRY = "expiry";
        public const string FLD_FUTURES_TICK_SIZE = "tick-size";
        public const string FLD_FUTURES_EXCHANGES = "exchanges";
        public const string FLD_FUTURES_DESCRIPTION = "description";
        public const string FLD_FUTURES_LAST = "last";
        public const string FLD_FUTURES_QUANTITY = "qty";
        public const string FLD_FUTURES_OPEN = "open";
        public const string FLD_FUTURES_HIGH = "high";
        public const string FLD_FUTURES_LOW = "low";
        public const string FLD_FUTURES_CLOSE = "close";

        //Futures depth
        public const string FLD_FUTURES_BID_1 = "B1";
        public const string FLD_FUTURES_BID_2 = "B2";
        public const string FLD_FUTURES_BID_3 = "B3";
        public const string FLD_FUTURES_BID_4 = "B4";
        public const string FLD_FUTURES_BID_5 = "B5";

        public const string FLD_FUTURES_ASK_1 = "A1";
        public const string FLD_FUTURES_ASK_2 = "A2";
        public const string FLD_FUTURES_ASK_3 = "A3";
        public const string FLD_FUTURES_ASK_4 = "A4";
        public const string FLD_FUTURES_ASK_5 = "A5";

        public const string FLD_FUTURES_BID_SIZE_1 = "BS1";
        public const string FLD_FUTURES_BID_SIZE_2 = "BS2";
        public const string FLD_FUTURES_BID_SIZE_3 = "BS3";
        public const string FLD_FUTURES_BID_SIZE_4 = "BS4";
        public const string FLD_FUTURES_BID_SIZE_5 = "BS5";

        public const string FLD_FUTURES_ASK_SIZE_1 = "AS1";
        public const string FLD_FUTURES_ASK_SIZE_2 = "AS2";
        public const string FLD_FUTURES_ASK_SIZE_3 = "AS3";
        public const string FLD_FUTURES_ASK_SIZE_4 = "AS4";
        public const string FLD_FUTURES_ASK_SIZE_5 = "AS5";

        //Exchange sub-blobs
        public const string FLD_EXCH_AMEX = "exch-A";
        public const string FLD_EXCH_BOX = "exch-B";
        public const string FLD_EXCH_CBOE = "exch-C";
        public const string FLD_EXCH_ISE = "exch-I";
        public const string FLD_EXCH_PSE = "exch-P";
        public const string FLD_EXCH_PHLX = "exch-X";

        // Valid values for PRICESRV_STATUS_FLD_STATUS.
        public enum pricesrv_status_value
        {
            pricesrv_status_value_initialized = 1,
            pricesrv_status_value_disconnected = 2
        };

        // Valid values for PRICESRV_STATUS_FLD_DOWNLOADSTATUS_VALUE 
        public enum pricesrv_downloadstatus_value
        {
            pricesrv_downloadstatus_value_download_complete = 3,
            pricesrv_downloadstatus_value_download_on_hold = 4,
            pricesrv_downloadstatus_value_download_restart = 5,
            pricesrv_downloadstatus_value_download_failed = 6,
            pricesrv_downloadstatus_value_download_pending = 7
        };
    }
}
