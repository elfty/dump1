using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using csclientnet;
using System.Linq;

namespace wpfexample
{

    class ItgPricer
    {
        private string m_sPart;
        private bool m_bConnected;
        private Hashtable priceDictBid = new Hashtable();
        private Hashtable priceDictAsk = new Hashtable();
        double dBid = -1;
        double dAsk = -1;
        double dLast = -1;
        int iBidSize = -1;
        int iAskSize = -1;
        int iLastSize = -1;
        double dOpen = -1;
        double dHigh = -1;
        double dLow = -1;
        double dClose = -1;
        int iVolume = -1;
        string sDescription = "";
        string sExpiry = "";
        string sBookOrder = "";
        string sDisposition = "";
        string sExchange = "";
        string id_imnt_ric = "";
        bool updateHandlerAttached = false;
        private csclientnet.ClientSession session;
        private csclientnet.Client listener;
        public ICollection mkts;
        private ICollection mktsNotPriced;
        public Dictionary<int, double?> tickerList;
        Dictionary<int, Instrument> instArr;
        internal Dictionary<int, double?> getTickerList()
        {
            lock (tickerList)
            {
                return tickerList;
            }

        }

        public ItgPricer()
        {
            if (m_bConnected)
            {
                pDisconnect();
            }
            else
            {
                //samplePricingModel = new SamplePricingModel();
                string txtServer = "tcp=simulation.itg.com:40002";
                string txtUserName = "rbchansim";
                string txtPassword = "Rb123!";
                //pConnect(txtServer, txtUserName, txtPassword);
                pConnect("tcp=simulation.itg.com:40002", "rbchansim", "Rb123!");
                //id_imnt_ric = ricCode;
                //getStockPrice(id_imnt_ric);

            }
        }

        public void run(Dictionary<int, double?> _tickerList, Dictionary<int, Instrument> _instArr)
        {
            instArr = _instArr;
            tickerList = _tickerList;
            foreach (var v in tickerList)
            {
                Instrument item = instArr[(int)v.Key];
                if (item.id_typ_imnt == "FUT")
                {
                    if (item.id_bberg.Contains("RTA"))
                        getFuturePrice("C_TF" + item.id_bberg.Substring(3, 1) + "1" + item.id_bberg.Substring(4, 1));
                    else
                        getFuturePrice(item.id_bberg);
                }
                else
                    getStockPrice(item.id_bberg);
            }
            //getFuturePrice("C_TFZ12");
        }
        public void pDisconnect()
        {
            m_bConnected = false;

            session.Disconnect();
            listener = null;
            session = null;

            pClearQuotes();
        }

        public void pConnect(string sServer, string sUsername, string sPassword)
        {

            session = new csclientnet.ClientSession();
            listener = new csclientnet.Client(System.Threading.SynchronizationContext.Current);
            listener.Connect += OnConnect;
            //listener.Disconnect += OnDisconnect;
            //listener.Reconnect += OnReconnect;

            listener.Update += OnUserUpdate;
            updateHandlerAttached = true;

            listener.SetClientSession(session);
            session.Connect(sServer, sUsername, sPassword, csclientnet.SecurityType.None);
        }

        private void pClearQuotes()
        {
            //lvQuotes.Items.Clear();
        }

        private void OnConnect(Client sender)
        {
            m_bConnected = true;

        }

        private void OnDisconnect(Client sender, ClientDisconnectReason reason)
        {
            m_bConnected = false;
        }

        private void OnReconnect(Client sender)
        {
            m_bConnected = true;
        }

        public void getStockPrice(string ricCode)
        {
            if (!updateHandlerAttached)
                listener.Update += OnUserUpdate;

            string sPriceSub;

            id_imnt_ric = ricCode;
            m_sPart = "TEST";
            //txtSymbol.Text = txtSymbol.Text.ToUpper();
            sPriceSub = pricesrvfields.PRICESRV_SUB_STOCK_DELAYED + "." + m_sPart + ". ." + id_imnt_ric;
            listener.Subscribe(pricesrvfields.SERVICEID_PRICE, sPriceSub, true);

            if (!priceDictBid.ContainsKey(ricCode))
            {
                priceDictBid.Add(ricCode, -1.0);
                priceDictAsk.Add(ricCode, -1.0);
            }


        }

        public void getFuturePrice(string ricCode)
        {
            if (!updateHandlerAttached)
                listener.Update += OnUserUpdate;

            string sPriceSub;

            id_imnt_ric = ricCode;
            m_sPart = "TEST";
            //txtSymbol.Text = txtSymbol.Text.ToUpper();
            sPriceSub = pricesrvfields.PRICESRV_SUB_FUTURE_DELAYED + "." + m_sPart + "." + id_imnt_ric;
            listener.Subscribe(pricesrvfields.SERVICEID_PRICE, sPriceSub, true);

        }

        public double getPriceAsk(string ricCode)
        {

            if (priceDictAsk.ContainsKey(ricCode))
                return (double)priceDictAsk[ricCode];
            else
                return -1;
        }

        public double getPriceBid(string ricCode)
        {

            if (priceDictBid.ContainsKey(ricCode))
                return (double)priceDictBid[ricCode];
            else
                return -1;
        }

        public void getOptPrice(string ricCode)
        {
            if (!updateHandlerAttached)
                listener.Update += OnUserUpdate;

            string sPriceSub;
            id_imnt_ric = ricCode;
            m_sPart = "TEST";
            //txtSymbol.Text = txtSymbol.Text.ToUpper();
            sPriceSub = pricesrvfields.PRICESRV_SUB_OPTION_DELAYED + "." + m_sPart + "." + id_imnt_ric;
            listener.Subscribe(pricesrvfields.SERVICEID_PRICE, sPriceSub, true);
            if (!priceDictBid.ContainsKey(ricCode))
            {
                priceDictBid.Add(ricCode, -1.0);
                priceDictAsk.Add(ricCode, -1.0);
            }
        }

        private void OnUserUpdate(Client sender, ClientUpdate update)
        {
            string[] sArr;
            sArr = update.Subscription.Split('.');

            if ((sArr[0] == "CSB") || (sArr[0] == "ISB"))
            {
                //DisplaySpreadUpdate(sArr[sArr.Length - 1], update.Update);
            }
            else
            {
                DisplayUpdate(sArr[sArr.Length - 1], update.Update);
                //pDisconnect();

            }
        }

        private void DisplayUpdate(string sSymbol, csclientnet.Blob csMsg)
        {

            dBid = 0;
            dAsk = 0;
            dLast = 0;
            for (int i = 0; i < csMsg.FieldCount; i++)
            {
                switch (csMsg.GetFieldDescription(i))
                {
                    case pricesrvfields.FLD_BID:
                    case pricesrvfields.FLD_FUTURES_BID_1:
                        {
                            dBid = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_ASK:
                    case pricesrvfields.FLD_FUTURES_ASK_1:
                        {
                            dAsk = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_LAST:
                    case pricesrvfields.FLD_FUTURES_LAST:
                        {
                            dLast = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_BIDSIZE:
                    case pricesrvfields.FLD_FUTURES_BID_SIZE_1:
                        {
                            iBidSize = csMsg.GetInt(i);
                            break;
                        }
                    case pricesrvfields.FLD_ASKSIZE:
                    case pricesrvfields.FLD_FUTURES_ASK_SIZE_1:
                        {
                            iAskSize = csMsg.GetInt(i);
                            break;
                        }
                    case pricesrvfields.FLD_LASTSIZE:
                        {
                            iLastSize = csMsg.GetInt(i);
                            break;
                        }
                    case pricesrvfields.FLD_OPEN:
                    case pricesrvfields.FLD_FUTURES_OPEN:
                        {
                            dOpen = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_HIGH:
                    case pricesrvfields.FLD_FUTURES_HIGH:
                        {
                            dHigh = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_LOW:
                    case pricesrvfields.FLD_FUTURES_LOW:
                        {
                            dLow = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_CLOSE:
                    case pricesrvfields.FLD_FUTURES_CLOSE:
                        {
                            dClose = csMsg.GetDouble(i);
                            break;
                        }
                    case pricesrvfields.FLD_VOLUME:
                    case pricesrvfields.FLD_FUTURES_QUANTITY:
                        {
                            iVolume = csMsg.GetInt(i);
                            break;
                        }
                    case pricesrvfields.FLD_DESCRIPTION:
                    case pricesrvfields.FLD_FUTURES_DESCRIPTION:
                        {
                            sDescription = csMsg.GetString(i);
                            break;
                        }
                    case pricesrvfields.FLD_EXCH_AMEX:
                        {
                            GetOptionsExchUpdateInfo(csMsg.GetBlob(i), ref dBid, ref dAsk, ref dLast, ref iBidSize, ref iAskSize, ref iLastSize,
                                ref dOpen, ref dHigh, ref dLow, ref dClose, ref iVolume, ref sBookOrder, ref sDisposition);
                            sExchange = "AMEX";
                            break;
                        }
                    case pricesrvfields.FLD_EXCH_BOX:
                        {
                            GetOptionsExchUpdateInfo(csMsg.GetBlob(i), ref dBid, ref dAsk, ref dLast, ref iBidSize, ref iAskSize, ref iLastSize,
                                ref dOpen, ref dHigh, ref dLow, ref dClose, ref iVolume, ref sBookOrder, ref sDisposition);
                            sExchange = "BOX";
                            break;
                        }
                    case pricesrvfields.FLD_EXCH_CBOE:
                        {
                            GetOptionsExchUpdateInfo(csMsg.GetBlob(i), ref dBid, ref dAsk, ref dLast, ref iBidSize, ref iAskSize, ref iLastSize,
                                ref dOpen, ref dHigh, ref dLow, ref dClose, ref iVolume, ref sBookOrder, ref sDisposition);
                            sExchange = "CBOE";
                            break;
                        }
                    case pricesrvfields.FLD_EXCH_ISE:
                        {
                            GetOptionsExchUpdateInfo(csMsg.GetBlob(i), ref dBid, ref dAsk, ref dLast, ref iBidSize, ref iAskSize, ref iLastSize,
                                ref dOpen, ref dHigh, ref dLow, ref dClose, ref iVolume, ref sBookOrder, ref sDisposition);
                            sExchange = "ISE";
                            break;
                        }
                    case pricesrvfields.FLD_EXCH_PSE:
                        {
                            GetOptionsExchUpdateInfo(csMsg.GetBlob(i), ref dBid, ref dAsk, ref dLast, ref iBidSize, ref iAskSize, ref iLastSize,
                                ref dOpen, ref dHigh, ref dLow, ref dClose, ref iVolume, ref sBookOrder, ref sDisposition);
                            sExchange = "PSE";
                            break;
                        }
                    case pricesrvfields.FLD_EXCH_PHLX:
                        {
                            GetOptionsExchUpdateInfo(csMsg.GetBlob(i), ref dBid, ref dAsk, ref dLast, ref iBidSize, ref iAskSize, ref iLastSize,
                                ref dOpen, ref dHigh, ref dLow, ref dClose, ref iVolume, ref sBookOrder, ref sDisposition);
                            sExchange = "PHLX";
                            break;
                        }
                    case pricesrvfields.FLD_EXPIRY:
                    case pricesrvfields.FLD_FUTURES_EXPIRY:
                        {
                            sExpiry = csMsg.GetString(i);
                            break;
                        }
                }
            }

            /*if (sSymbol.Length > 0)
            {
                bool bNew = true;
                foreach (ListViewItem lviItem in lvQuotes.Items)
                {
                    if (0 == lviItem.Text.CompareTo(sSymbol))
                    {
                        bNew = false;
                        foreach (ListViewItem.ListViewSubItem lvsiSubItem in lviItem.SubItems)
                        {

                            switch (lviItem.SubItems.IndexOf(lvsiSubItem))
                            {
                                case COL_BID:
                                    if (-1 != dBid)
                                        lvsiSubItem.Text = dBid.ToString();
                                    break;
                                case COL_ASK:
                                    if (-1 != dAsk)
                                        lvsiSubItem.Text = dAsk.ToString();
                                    break;
                                case COL_LAST:
                                    if (-1 != dLast)
                                        lvsiSubItem.Text = dLast.ToString();
                                    break;
                                case COL_BIDSIZE:
                                    if (-1 != iBidSize)
                                        lvsiSubItem.Text = iBidSize.ToString();
                                    break;
                                case COL_ASKSIZE:
                                    if (-1 != iAskSize)
                                        lvsiSubItem.Text = iAskSize.ToString();
                                    break;
                                case COL_LASTSIZE:
                                    if (-1 != iLastSize)
                                        lvsiSubItem.Text = iLastSize.ToString();
                                    break;
                                case COL_OPEN:
                                    if (-1 != dOpen)
                                        lvsiSubItem.Text = dOpen.ToString();
                                    break;
                                case COL_HIGH:
                                    if (-1 != dHigh)
                                        lvsiSubItem.Text = dHigh.ToString();
                                    break;
                                case COL_LOW:
                                    if (-1 != dLow)
                                        lvsiSubItem.Text = dLow.ToString();
                                    break;
                                case COL_CLOSE:
                                    if (-1 != dClose)
                                        lvsiSubItem.Text = dClose.ToString();
                                    break;
                                case COL_VOLUME:
                                    if (-1 != iVolume)
                                        lvsiSubItem.Text = iVolume.ToString();
                                    break;
                                case COL_DESCRIPTION:
                                    if (sDescription.Length > 0)
                                        lvsiSubItem.Text = sDescription;
                                    break;
                                case COL_EXPIRY:
                                    if (sExpiry.Length > 0)
                                        lvsiSubItem.Text = sExpiry;
                                    break;
                                case COL_EXCHANGE:
                                    if (sExchange.Length > 0)
                                        lvsiSubItem.Text = sExchange;
                                    break;
                            }
                        }
                        break;
                    }
                }
            
                if (bNew)
                {
                    ListViewItem lviNewItem;
                    lviNewItem = lvQuotes.Items.Add(sSymbol);
                    if (-1 != dBid)
                        lviNewItem.SubItems.Add(dBid.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != dAsk)
                        lviNewItem.SubItems.Add(dAsk.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != dLast)
                        lviNewItem.SubItems.Add(dLast.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != iBidSize)
                        lviNewItem.SubItems.Add(iBidSize.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != iAskSize)
                        lviNewItem.SubItems.Add(iAskSize.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != iLastSize)
                        lviNewItem.SubItems.Add(iLastSize.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != dOpen)
                        lviNewItem.SubItems.Add(dOpen.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != dHigh)
                        lviNewItem.SubItems.Add(dHigh.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != dLow)
                        lviNewItem.SubItems.Add(dLow.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != dClose)
                        lviNewItem.SubItems.Add(dClose.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    if (-1 != iVolume)
                        lviNewItem.SubItems.Add(iVolume.ToString());
                    else
                        lviNewItem.SubItems.Add("");

                    lviNewItem.SubItems.Add(sDescription);
                    lviNewItem.SubItems.Add(sExpiry);
                    lviNewItem.SubItems.Add(sExchange);
                }
             
            }*/


            try
            {
                //topic.Substring(0, topic.IndexOf(" "));
                int dict = 0;
                if (sSymbol.Contains("C_TF"))
                {
                    dict = instArr.FirstOrDefault(x => x.Value.id_bberg == "RTA" + sSymbol.Substring(4, 1) + sSymbol.Substring(6, 1)).Key;
                }
                else
                    dict = instArr.FirstOrDefault(x => x.Value.id_bberg == sSymbol).Key;
                Instrument item = instArr[dict];

                if (dLast != 0)
                {
                    tickerList[(int)item.id_imnt] = dLast;
                }

            }
            catch
            {
                
            }

            

            //if ((double)priceDictBid[sSymbol] != -1.0 && (double)priceDictAsk[sSymbol] != -1.0)
            //{
                //listener.Update -= OnUserUpdate;
                //updateHandlerAttached = false;
            //}
            System.Threading.Thread.Sleep(500);
        }

        private void GetOptionsExchUpdateInfo(csclientnet.Blob csMsg, ref double dBid, ref double dAsk, ref double dLast, ref int iBidSize,
                                                ref int iAskSize, ref int iLastSize, ref double dOpen, ref double dHigh, ref double dLow, ref double dClose, ref int iVolume,
                                                ref string sBookOrder, ref string sDisposition)
        {
            for (int i = 0; i < csMsg.FieldCount; i++)
            {
                switch (csMsg.GetFieldDescription(i))
                {
                    case pricesrvfields.FLD_BID:
                        dBid = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_ASK:
                        dAsk = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_LAST:
                        dLast = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_BIDSIZE:
                        iBidSize = csMsg.GetInt(i);
                        break;
                    case pricesrvfields.FLD_ASKSIZE:
                        iAskSize = csMsg.GetInt(i);
                        break;
                    case pricesrvfields.FLD_LASTSIZE:
                        iLastSize = csMsg.GetInt(i);
                        break;
                    case pricesrvfields.FLD_OPEN:
                        dOpen = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_HIGH:
                        dHigh = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_LOW:
                        dLow = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_CLOSE:
                        dClose = csMsg.GetDouble(i);
                        break;
                    case pricesrvfields.FLD_VOLUME:
                        iVolume = csMsg.GetInt(i);
                        break;
                    case pricesrvfields.FLD_BOOKORDER:
                        sBookOrder = csMsg.GetString(i);
                        break;
                    case pricesrvfields.FLD_DISPOSITION:
                        sDisposition = csMsg.GetString(i);
                        break;
                }
            }
        }

    }
}
