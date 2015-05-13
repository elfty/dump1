using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading;

using YJ.AppLink.Api;
using YJ.AppLink;
using YJ.AppLink.Pricing;

namespace YJ.AppLink.Messaging
{

    /// <summary>
    /// MessagingService provides methods for sending IM messages and orders through the YellowJacket application
    /// </summary>
	public class MessagingService : Service
	{

        private IDictionary<string, object> requestMap = new Dictionary<string, object>();

        public event Action<ResponseReceivedEventArgs> SendResponseReceived;

        /// <summary>
        /// The current version of the API that the service service supports.
        /// </summary>
        public const string VERSION = "Messaging 1.0";

        internal MessagingService(Session session) : base(ServiceName.Messaging, MessagingService.VERSION, session) { }

		#region public methods

		/// <summary>
		/// Sends an order on a specified market to a contact
		/// </summary>
        public void SendMarketOrder(SendOrderArgs args)
		{
            try
            {
                if (Running == false)
                {
                    Start();
                }
                
                Session.Logger.Info("Sending order", this);

                SendOrder message = new Api.SendOrder();
                message.MarketId = args.MarketId;

                if (args.BidIsSet)
                {
                    message.BidSpecified = true;
                    message.Bid = args.Bid;

                    if (args.BidSizeIsSet)
                    {
                        message.BidSizeSpecified = true;
                        message.BidSize = args.BidSize;
                    }
                }

                if (args.AskIsSet)
                {
                    message.AskSpecified = true;
                    message.Ask = args.Ask;

                    if (args.AskSizeIsSet)
                    {
                        message.AskSizeSpecified = true;
                        message.AskSize = args.AskSize;
                    }
                }

                message.Color = args.Color;
                
                message.Handle = args.Handle;
                message.SendMode = args.SendMode.ToString();

                if (args.SendFromDeskIsSet)
                {
                    message.SendFromDeskSpecified = true;
                    message.SendFromDesk = args.SendFromDesk;
                }

                message.RequestId = System.Guid.NewGuid().ToString();
                requestMap.Add(message.RequestId, message);
                Sender.Send(message);
            }
            catch (Exception e)
            {
                Session.OnSessionError("Failure sending order", this, e);
                throw e;
            }
		}

        /// <summary>
        /// Sends a message to a contact
        /// </summary>
        public void SendIMMessage(SendMessageArgs args)
        {
            try
            {
                if (Running == false)
                {
                    Start();
                }
                
                Session.Logger.Info("Sending message", this);
                SendMessage message = new Api.SendMessage();
                message.Message = args.MessageText;

                message.Handle = args.Handle;
                message.SendMode = args.SendMode.ToString();

                if (args.SendFromDeskIsSet)
                {
                    message.SendFromDeskSpecified = true;
                    message.SendFromDesk = args.SendFromDesk;
                }

                message.RequestId = System.Guid.NewGuid().ToString();
                requestMap.Add(message.RequestId, message);
                Sender.Send(message);
            }
            catch (Exception e)
            {
                Session.OnSessionError("Failure sending message", this, e);
                throw e;
            }
        }

        #endregion

        #region protected methods

        protected override void HandleReceivedMessage(object sender, YJ.AppLink.Net.MessageReceivedEventArgs args)
		{
			 //note: if the session does not have a threadSafeGUIControl 
			 //this is invoked asynchronously and is not thread safe.
			
            try
            {
                Session.Logger.Debug("Message received", this);
                if (args.Envelope.Content != null &&
                    args.Envelope.Content is SendResponse)
                {
                    SendResponse resp = (SendResponse)args.Envelope.Content;

                    if (resp.Success == false)
                    {
                        Session.Logger.Error("Error response received for message or order sent: " + resp.Error, this);
                    }

                    SendMessage messageSent = null;
                    SendOrder orderSent = null;
                    if (string.IsNullOrEmpty(resp.RequestId) == false && requestMap.ContainsKey(resp.RequestId))
                    {
                        object request = requestMap[resp.RequestId];
                        if (request is SendMessage)
                        {
                            messageSent = request as SendMessage;
                        }
                        else if (request is SendOrder)
                        {
                            orderSent = request as SendOrder;
                        }

                        // no need to store a reference anymore
                        requestMap.Remove(resp.RequestId);
                    }

                    if (SendResponseReceived != null)
                    {
                        ResponseReceivedEventArgs respArgs = new ResponseReceivedEventArgs()
                            {
                                Error = resp.Error,
                                MessageSent = messageSent,
                                OrderSent = orderSent,
                                RequestId = resp.RequestId,
                                Success = resp.Success
                            };

                        SendResponseReceived(respArgs);
                    }
                }

            }
            catch (Exception e)
            {
                Session.OnSessionError("Failure handling received message", this, e);
                // do not throw exception
            }
		}


		#endregion

	}

    public enum SendMode
    {
        Auto,
        InsertInTab
    }

    public abstract class SendArgs
    {
        private bool sendFromDesk;
        private bool sendFromDeskIsSet;

        protected SendArgs() { SendMode = Messaging.SendMode.Auto; }
        
        public string Handle { get; set; }
        public SendMode SendMode { get; set; }

        public bool SendFromDesk
        {
            get { return sendFromDesk; }
            set
            {
                sendFromDesk = value;
                sendFromDeskIsSet = true;
            }
        }

        internal bool SendFromDeskIsSet
        {
            get { return sendFromDeskIsSet; }
        }
    }

    public class SendMessageArgs : SendArgs
    {
        public string MessageText { get; set; }
    }

    public class SendOrderArgs : SendArgs
    {
        private MarketData market;
        
        private double bid = double.NaN;
        private bool bidIsSet;
        private int bidSize = -1;
        private bool bidSizeIsSet;

        private double ask = double.NaN;
        private bool askIsSet;
        private int askSize = -1;
        private bool askSizeIsSet;

        public string Color { get; set; }

        public SendOrderArgs(MarketData market)
            : base ()
        {
            this.market = market;
        }

        public string MarketId
        {
            get { return market.Id; }
        }

        public double Bid
        {
            get { return bid; }
            set
            {
                bid = value;
                bidIsSet = true;
            }
        }

        internal bool BidIsSet
        {
            get { return bidIsSet; }
        }

        public int BidSize
        {
            get { return bidSize; }
            set
            {
                bidSize = value;
                bidSizeIsSet = true;
            }
        }

        internal bool BidSizeIsSet
        {
            get { return this.bidSizeIsSet; }
        }

        public double Ask
        {
            get { return ask; }
            set
            {
                ask = value;
                askIsSet = true;
            }
        }

        internal bool AskIsSet
        {
            get { return askIsSet; }
        }

        public int AskSize
        {
            get { return askSize; }
            set
            {
                askSize = value;
                askSizeIsSet = true;
            }
        }

        internal bool AskSizeIsSet
        {
            get { return askSizeIsSet; }
        }
    }

    public class ResponseReceivedEventArgs : System.EventArgs
    {
        public string RequestId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public SendOrder OrderSent { get; set; }
        public SendMessage MessageSent { get; set; }
    }
}
