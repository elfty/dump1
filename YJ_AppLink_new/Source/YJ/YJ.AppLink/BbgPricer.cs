//----------------------------------------------------------------------------
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A  PARTICULAR PURPOSE.
//----------------------------------------------------------------------------

using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using Name = Bloomberglp.Blpapi.Name;
using Request = Bloomberglp.Blpapi.Request;
using Service = Bloomberglp.Blpapi.Service;
using Session = Bloomberglp.Blpapi.Session;

namespace Bloomberglp.Blpapi.Examples
{

    public class BbgPricer
    {
        public double askPrice = 0;
        public double bidPrice = 0;
        public double undPrice = 0;
        public string id_imnt_ric = "";

        //constructor
        public BbgPricer(string OSICode)
        {
            /*string serverHost = "localhost";
            int serverPort = 8194;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                return;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                return;
            }
            Service refDataService = session.GetService("//blp/refdata");

            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");
            securities.AppendValue(OSICode + " Equity");
            //securities.AppendValue("/cusip/912828GM6@BGN");
            Element fields = request.GetElement("fields");
            fields.AppendValue("BID");
            fields.AppendValue("ASK");
            fields.AppendValue("TICKER");
            fields.AppendValue("OPT_UNDL_PX");

            session.SendRequest(request, null);

            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {
                    System.Console.WriteLine(msg.AsElement);
                    if (msg.HasElement("securityData"))
                    {
                        //msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsString("DS002");
                        id_imnt_ric = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsString("TICKER");
                        try
                        {
                            bidPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("BID");
                        }
                        catch
                        {
                            bidPrice = 0;
                        }

                        askPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("ASK");
                        undPrice = msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsFloat64("OPT_UNDL_PX");
                        //return;
                    }
                }
                if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    break;
                }
            }

            session.Stop();
             */
            bidPrice = -1;
            askPrice = -1;
            undPrice = -1;
            return;
        }
    }
}
