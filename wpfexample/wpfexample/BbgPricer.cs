//----------------------------------------------------------------------------
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A  PARTICULAR PURPOSE.
//----------------------------------------------------------------------------

using Datetime = Bloomberglp.Blpapi.Datetime;
using Event = Bloomberglp.Blpapi.Event;
using Element = Bloomberglp.Blpapi.Element;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using DataType = Bloomberglp.Blpapi.Schema.Datatype;

using wpfexample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text;
using ArrayList = System.Collections.ArrayList;

namespace Bloomberglp.Blpapi.Examples
{

    class BbgPricer
    {
        public double bidPrice = 0;
        public double askPrice = 0;
        public double lastPrice = 0;
        public string id_imnt_ric = "";
        public Datetime last_update_dt;
        public Dictionary<int, double?> tickerList;
        public bool isrunning = false;

        private ArrayList d_fields;

        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name SECURITY_DATA = new Name("securityData");
        private static readonly Name SECURITY_NAME = new Name("security");
        private static readonly Name FIELD_DATA = new Name("fieldData");
        private static readonly Name DATE = new Name("date");
        private static readonly Name RESPONSE_ERROR = new Name("responseError");
        private static readonly Name SECURITY_ERROR = new Name("securityError");
        private static readonly Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static readonly Name ERROR_INFO = new Name("errorInfo");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name MESSAGE = new Name("message");

        internal Dictionary<int, double?> getTickerList()
        {
            lock (tickerList)
            {
                return tickerList;
            }

        }

        public void addToTickerList(int id_imnt)
        {
            isrunning = false;

            lock (tickerList)
            {
                tickerList.Add(id_imnt, 0f);
            }
            

        }

        //constructor
        public Dictionary<string, double?> BbgPricerList(Dictionary<string, double?> tickerList)
        {
            string serverHost = "localhost";
            int serverPort = 8194;
            //id_imnt_ric = OSICode;
            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                return null;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                return null;
            }
            Service refDataService = session.GetService("//blp/refdata");

            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");
            foreach (var v in tickerList)
            {
                securities.AppendValue(v.Key + " US Equity");
            }

            //securities.AppendValue("/cusip/912828GM6@BGN");
            Element fields = request.GetElement("fields");
            fields.AppendValue("TICKER");
            //fields.AppendValue("BID");
            //fields.AppendValue("ASK");
            fields.AppendValue("PX_LAST");

            session.SendRequest(request, null);
            int i = 0;
            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {
                    System.Console.WriteLine(msg.AsElement);
                    if (msg.HasElement("securityData"))
                    {
                        //msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsString("DS002");

                        try
                        {
                            id_imnt_ric = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsString("TICKER");
                            lastPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("PX_LAST");
                            tickerList[id_imnt_ric] = lastPrice;
                        }
                        catch
                        {
                            lastPrice = 0;
                        }

                        //lastPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("LAST_PRICE");
                        //bidPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("BID");
                        //askPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("ASK");

                        //return;

                        i++;
                    }
                }
                if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    break;
                }
            }

            session.Stop();

            return tickerList;
        }

        public double? BbgPricerTicker(string ticker, string tx_field)
        {
            string serverHost = "localhost";
            int serverPort = 8194;
            //id_imnt_ric = OSICode;
            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                return null;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                return null;
            }
            Service refDataService = session.GetService("//blp/refdata");

            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");

            securities.AppendValue(ticker);


            //securities.AppendValue("/cusip/912828GM6@BGN");
            Element fields = request.GetElement("fields");
            fields.AppendValue("TICKER");
            //fields.AppendValue("BID");
            //fields.AppendValue("ASK");
            fields.AppendValue(tx_field);

            session.SendRequest(request, null);
            id_imnt_ric = ticker;
            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {
                    System.Console.WriteLine(msg.AsElement);
                    if (msg.HasElement("securityData"))
                    {
                        //msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsString("DS002");

                        try
                        {
                            lastPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64(tx_field);

                        }
                        catch
                        {
                            lastPrice = -1;
                        }


                        //bidPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("BID");
                        //askPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("ASK");

                        //return;
                    }
                }
                if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    break;
                }
            }

            session.Stop();

            return lastPrice;
        }

        public void run(Dictionary<int, double?> _tickerList, Dictionary<int, Instrument> instArr, Dictionary<string, int> tickernameList)
        {
            string serverHost = "localhost";
            int serverPort = 8194;
            tickerList = _tickerList;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            System.Console.WriteLine("Connecting to " + serverHost + ":" + serverPort);
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                System.Console.Error.WriteLine("Failed to start session.");
                return;
            }
            if (!session.OpenService("//blp/mktdata"))
            {
                System.Console.Error.WriteLine("Failed to open //blp/mktdata");
                return;
            }

            System.Collections.Generic.List<Subscription> subscriptions
               = new System.Collections.Generic.List<Subscription>();

            // subscribe with a 1 second interval
            //string security = "IBM US Equity";

            foreach (var v in tickerList)
            {
                //Instrument item = instArr.FirstOrDefault(x => x.id_bberg == v.Key);
                Instrument item = instArr[(int)v.Key];
                if (item.id_typ_imnt == "STK")
                {
                    subscriptions.Add(new Subscription(
                        item.id_bberg + " Equity",
                        "PX_LAST_PRE_SESSION,LAST_PRICE,BID,ASK",
                        //"interval=1.0",
                        new CorrelationID(item.id_bberg + " Equity"))
                        );
                }
                else if (item.id_typ_imnt == "IND" || item.id_typ_imnt == "FUT")
                {
                    subscriptions.Add(new Subscription(
                        item.id_bberg + " INDEX",
                        "LAST_PRICE,BID,ASK",
                        //"interval=1.0",
                        new CorrelationID(item.id_bberg + " INDEX"))
                        );
                }

            }

            session.Subscribe(subscriptions);

            while (true && isrunning)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {

                    if (eventObj.Type == Event.EventType.SUBSCRIPTION_DATA ||
                        eventObj.Type == Event.EventType.SUBSCRIPTION_STATUS)
                    {
                        string topic = (string)msg.CorrelationID.Object;
                        //System.Console.WriteLine(topic + ": " + msg.AsElement);
                        /*if (msg.HasElement("BID", true) && msg.HasElement("ASK", true))
                        {
                            //topic.Substring(0, topic.IndexOf(" "));
                            //var dict = instArr.FirstOrDefault(x => x.Value.id_bberg == topic.Substring(0, topic.IndexOf(" ")));

                            //Instrument item = instArr[(int)dict.Key];
                            Instrument item = instArr[tickernameList[topic.Substring(0, topic.IndexOf(" "))]];
                            lastPrice = (msg.GetElementAsFloat64("BID") + msg.GetElementAsFloat64("ASK")) / 2f;
                            tickerList[(int)item.id_imnt] = lastPrice;
                        }*/
                        if (msg.HasElement("LAST_PRICE",true))
                        {
                            //topic.Substring(0, topic.IndexOf(" "));
                            //var dict = instArr.FirstOrDefault(x => x.Value.id_bberg == topic.Substring(0, topic.IndexOf(" ")));
                            
                            //Instrument item = instArr[(int)dict.Key];
                            Instrument item = instArr[tickernameList[topic.Substring(0, topic.IndexOf(" "))]];
                            lastPrice = msg.GetElementAsFloat64("LAST_PRICE");
                            
                            tickerList[(int)item.id_imnt] = lastPrice;
                        }
                    }
                    else
                    {
                        //System.Console.WriteLine(msg.AsElement);
                    }
                }
                //Thread.Sleep(1000);
            }

            session.Stop();
        }

        public double? BbgHist()
        {
            d_fields = new ArrayList();

            string serverHost = "localhost";
            int serverPort = 8194;
            //id_imnt_ric = OSICode;
            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;
            string ticker = "CONSSENT INDEX";
            string tx_field = "PX_LAST";
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                return null;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                return null;
            }
            Service refDataService = session.GetService("//blp/refdata");

            Request request = refDataService.CreateRequest("HistoricalDataRequest");
            Element securities = request.GetElement("securities");

            securities.AppendValue(ticker);


            //securities.AppendValue("/cusip/912828GM6@BGN");
            Element fields = request.GetElement("fields");
            fields.AppendValue("TICKER");
            //fields.AppendValue("BID");
            //fields.AppendValue("ASK");
            fields.AppendValue(tx_field);
            d_fields.Add("PX_LAST");

            // Add fields to request
            for (int i = 0; i < d_fields.Count; ++i)
            {
                fields.AppendValue((string)d_fields[i]);
            }

            request.Set("startDate", "20010101");
            request.Set("endDate", "20010601");
            request.Set("periodicitySelection", "DAILY");
            request.Set("nonTradingDayFillOption", "NON_TRADING_WEEKDAYS");
            request.Set("nonTradingDayFillMethod", "PREVIOUS_VALUE");

            session.SendRequest(request, null);
            id_imnt_ric = ticker;

            string date;
            while (true)
            {
                Event eventObj = session.NextEvent();
                if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    System.Console.WriteLine("Processing Partial Response");
                    processResponseEvent(eventObj);
                }
                else if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    System.Console.WriteLine("Processing Response");
                    processResponseEvent(eventObj);
                    break;
                }
                else
                {
                    foreach (Message msg in eventObj)
                    {
                        Debug.WriteLine(msg.AsElement);
                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                break;
                            }
                        }
                    }
                }
                
            }

            session.Stop();

            return lastPrice;
        }

        private void processResponseEvent(Event eventObj)
        {
            foreach (Message msg in eventObj)
            {
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }
                if ((eventObj.Type != Event.EventType.PARTIAL_RESPONSE) && (eventObj.Type != Event.EventType.RESPONSE))
                {
                    continue;
                }
                if (!ProcessErrors(msg))
                {
                    ProcessFields(msg);
                }
            }
        }//end processResponseEvent


        void ProcessFields(Message msg)
        {
            String delimiter = "\t";

            //Print out the date column header
            System.Console.Write("DATE" + delimiter + delimiter);

            // Print out the field column headers
            for (int k = 0; k < d_fields.Count; k++)
            {
                System.Console.Write(d_fields[k].ToString() + delimiter);
            }
            System.Console.Write("\n\n");

            Element securityData = msg.GetElement(SECURITY_DATA);
            Element fieldData = securityData.GetElement(FIELD_DATA);

            //Iterate through all field values returned in the message
            if (fieldData.NumValues > 0)
            {
                for (int j = 0; j < fieldData.NumValues; j++)
                {
                    Element element = fieldData.GetValueAsElement(j);

                    //Print out the date
                    Datetime date = element.GetElementAsDatetime(DATE);
                    System.Console.Write(date.DayOfMonth + "/" + date.Month + "/" + date.Year + delimiter);

                    //Check for the presence of all the fields requested
                    for (int k = 0; k < d_fields.Count; k++)
                    {
                        String temp_field_str = d_fields[k].ToString();
                        if (element.HasElement(temp_field_str))
                        {
                            Element temp_field = element.GetElement(temp_field_str);
                            Name TEMP_FIELD_STR = new Name(temp_field_str);

                            int datatype = temp_field.Datatype.GetHashCode();

                            //Extract the value dependent on the dataype and print to the console
                            switch (datatype)
                            {
                                case (int)DataType.BOOL://Bool
                                    {
                                        bool field1;
                                        field1 = element.GetElementAsBool(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.CHAR://Char
                                    {
                                        char field1;
                                        field1 = element.GetElementAsChar(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.INT32://Int32
                                    {
                                        Int32 field1;
                                        field1 = element.GetElementAsInt32(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.INT64://Int64
                                    {
                                        Int64 field1;
                                        field1 = element.GetElementAsInt64(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.FLOAT32://Float32
                                    {
                                        float field1;
                                        field1 = element.GetElementAsFloat32(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.FLOAT64://Float64
                                    {
                                        double field1;
                                        field1 = element.GetElementAsFloat64(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.STRING://String
                                    {
                                        String field1;
                                        field1 = element.GetElementAsString(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                                case (int)DataType.DATE://Date
                                    {
                                        Datetime field1;
                                        field1 = element.GetElementAsDatetime(TEMP_FIELD_STR);
                                        System.Console.Write(field1.Year + '/' + field1.Month + '/' + field1.DayOfMonth + delimiter);
                                        break;
                                    }
                                case (int)DataType.TIME://Time
                                    {
                                        Datetime field1;
                                        field1 = element.GetElementAsDatetime(TEMP_FIELD_STR);
                                        System.Console.Write(field1.Hour + '/' + field1.Minute + '/' + field1.Second + delimiter);
                                        break;
                                    }
                                case (int)DataType.DATETIME://Datetime
                                    {
                                        Datetime field1;
                                        field1 = element.GetElementAsDatetime(TEMP_FIELD_STR);
                                        System.Console.Write(field1.Year + '/' + field1.Month + '/' + field1.DayOfMonth + '/');
                                        System.Console.Write(field1.Hour + '/' + field1.Minute + '/' + field1.Second + delimiter);
                                        break;
                                    }
                                default:
                                    {
                                        String field1;
                                        field1 = element.GetElementAsString(TEMP_FIELD_STR);
                                        System.Console.Write(field1 + delimiter);
                                        break;
                                    }
                            }//end of switch
                        }//end of if
                        System.Console.WriteLine("");
                    }//end of for
                }
            }
        }//end of method

        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            Debug.WriteLine(leadingStr +
                                     errorInfo.GetElementAsString(CATEGORY) + " (" +
                                     errorInfo.GetElementAsString(MESSAGE) + ")");
        }//end printErrorInfo

        bool ProcessErrors(Message msg)
        {
            Element securityData = msg.GetElement(SECURITY_DATA);

            if (securityData.HasElement(SECURITY_ERROR))
            {
                Element security_error = securityData.GetElement(SECURITY_ERROR);
                Element error_message = security_error.GetElement(MESSAGE);
                Debug.WriteLine(error_message);
                return true;
            }
            return false;
        }

    }
}
