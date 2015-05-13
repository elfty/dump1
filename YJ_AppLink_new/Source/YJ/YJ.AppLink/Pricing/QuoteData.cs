using System;
using System.Collections.Generic;
using System.Text;

using YJ.AppLink.Api;

namespace YJ.AppLink.Pricing
{
    public class QuoteData
    {
        private Quote quote;
        private ParticipantData sender;
        private ParticipantData receiver;

        internal QuoteData(Quote q)
        {
            this.quote = q;
            if (q.Receiver != null)
            {
                receiver = new ParticipantData(q.Receiver);
            }

            if (q.Sender != null)
            {
                sender = new ParticipantData(q.Sender);
            }
        }

        public ParticipantData Sender
        {
            get { return this.sender; }
        }

        public ParticipantData Receiver
        {
            get { return this.receiver; }
        }

        public Quote Quote
        {
            get { return this.quote; }
        }

        public QuoteSource Source
        {
            get
            {
                return quote.SentOrReceived.Equals("Sent") ? QuoteSource.Sent : QuoteSource.Received;
            }
        }

        public bool CanRespond()
        {
            // do not respond to quotes sent from me or my company
            if (Source == QuoteSource.Sent)
                return false;

            // only respond to quotes sent from a contact
            if (Sender == null || Sender.ParticipantType != AppLink.Api.ParticipantType.Contact)
                return false;

            // only respond to quotes sent to me or my desk, not my coworkers
            if (Receiver != null &&
                (Receiver.ParticipantType == AppLink.Api.ParticipantType.User ||
                (Receiver.ParticipantType == AppLink.Api.ParticipantType.Desk)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }

    public enum QuoteSource
    {
        Sent,
        Received
    }
}
