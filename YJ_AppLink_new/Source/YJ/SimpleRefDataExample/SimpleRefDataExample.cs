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

    public class SimpleRefDataExample
    {
        public SimpleRefDataExample()
        {
        }

        public float liveBidPrice;
        private float liveAskPrice;
        internal float blah = 0;



        /*internal SimpleRefDataExample()
        {

        //private void run(string[] args)
        //{
            string serverHost = "localhost";
            int serverPort = 8194;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            //System.Console.WriteLine("Connecting to " + serverHost + ":" + serverPort);
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                //System.Console.WriteLine("Failed to start session.");
                return;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                //System.Console.Error.WriteLine("Failed to open //blp/refdata");
                return;
            }
            Service refDataService = session.GetService("//blp/refdata");

            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");
            securities.AppendValue("BTU   120218C00036000 Equity");
            //securities.AppendValue("/cusip/912828GM6@BGN");
            Element fields = request.GetElement("fields");
            fields.AppendValue("BID");
			fields.AppendValue("ASK");
            fields.AppendValue("DS002");

            //System.Console.WriteLine("Sending Request: " + request);
            session.SendRequest(request, null);

            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {
                    //System.Console.WriteLine(msg.AsElement);
					//if (msg.HasElement("securityData"))
						//System.Console.WriteLine(msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsDatetime("PX_DT_1D").ToSystemDateTime().ToString());
                }
                if (eventObj.Type == Event.EventType.RESPONSE)
                {					
                    break;
                }
            }
        }*/

        public double getBid(string OSIcode)
        {
            string serverHost = "localhost";
            int serverPort = 8194;

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = serverHost;
            sessionOptions.ServerPort = serverPort;

            //System.Console.WriteLine("Connecting to " + serverHost + ":" + serverPort);
            Session session = new Session(sessionOptions);
            bool sessionStarted = session.Start();
            if (!sessionStarted)
            {
                //System.Console.WriteLine("Failed to start session.");
                return 0;
            }
            if (!session.OpenService("//blp/refdata"))
            {
                //System.Console.Error.WriteLine("Failed to open //blp/refdata");
                return 0;
            }
            Service refDataService = session.GetService("//blp/refdata");

            Request request = refDataService.CreateRequest("ReferenceDataRequest");
            Element securities = request.GetElement("securities");
            securities.AppendValue("BTU   120218C00036000 Equity");
            //securities.AppendValue("/cusip/912828GM6@BGN");
            Element fields = request.GetElement("fields");
            fields.AppendValue("BID");
            fields.AppendValue("ASK");
            fields.AppendValue("DS002");

            //System.Console.WriteLine("Sending Request: " + request);
            session.SendRequest(request, null);

            while (true)
            {
                Event eventObj = session.NextEvent();
                foreach (Message msg in eventObj)
                {
                    //System.Console.WriteLine(msg.AsElement);
                    if (msg.HasElement("securityData"))
                        System.Console.WriteLine(msg.GetElement("securityData").GetValueAsElement(0).GetElement("fieldData").GetElementAsDatetime("PX_DT_1D").ToSystemDateTime().ToString());
                }
                if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    break;
                }
            }
            return 0;
        }
    }
}
