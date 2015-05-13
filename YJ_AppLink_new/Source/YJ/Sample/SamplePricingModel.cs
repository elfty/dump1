using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using YJ.AppLink;
using YJ.AppLink.Pricing;

using Bloomberglp.Blpapi.Examples;



namespace YJ.Sample
{
	public class SamplePricingModel
	{
		private YJ.AppLink.Session session;
		private YJ.AppLink.Pricing.PricingService pricingService;
		private bool autoPrice = true;
        private ArrayList apiKeys;
        private ItgPricer itgpricer;
        private WotiMarketData wotipricer;

        private IList<string> acknwoledgedQuotes = new List<string>();

		public SamplePricingModel() {}

		public void Start(YJ.AppLink.Session session)
		{
			try
			{
                //itgpricer = new ItgPricer(this);
                //wotipricer = new WotiMarketData();

				this.session = session;
				pricingService = session.PricingService;
				pricingService.PriceUpdateRequested +=new PriceUpdateRequestedHandler(pricingService_PriceUpdateRequested);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void Stop()
		{
			try
			{
                if (pricingService != null)
                {
                    pricingService.PriceUpdateRequested -= new PriceUpdateRequestedHandler(pricingService_PriceUpdateRequested);
                }
            }
			catch
			{
				// exception ignored if not listening
			}
		}

        public bool AutoReply { get; set; }

		public bool AutoPriceNewMarkets
		{
			get { return this.autoPrice; }
			set { this.autoPrice = value; }
		}

		public void RepriceAll()
		{
			ICollection mkts = pricingService.GetAllMarkets();
			foreach (MarketData mkt in mkts)
			{
				try
				{
					PriceMarket(mkt);
				}
				catch (Exception e)
				{
					throw e;
				}
			}
		}

		public void SetOverridePrice(MarketData mkt, double theoretical)
		{
			try
			{
				mkt.SetTheoreticalOverride(theoretical);
				mkt.SendPriceResponse();
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		private void pricingService_PriceUpdateRequested(object sender, PriceUpdateRequestedEventArgs args)
		{			
			try
			{
				if (this.autoPrice)
				{

                    //getItgPrices(args.Market);
					PriceMarket(args.Market);

                    if (AutoReply)
                    {
                        SendResponseOrder(args.Market);
                    }
				}


			}
			catch (Exception ex)
			{
				// not thrown - not handled by UI
				System.Console.WriteLine("SamplePricingModel error pricing market " + ex.Message );
			}
		}


        private void pricingService_PriceUpdateRequestedPricer(object sender, PriceUpdateRequestedEventArgs args)
        {
            try
            {
                if (this.autoPrice)
                {
                    //getItgPrices(args.Market);
                    PriceMarket(args.Market);

                    if (AutoReply)
                    {
                        SendResponseOrder(args.Market);
                    }
                }


            }
            catch (Exception ex)
            {
                // not thrown - not handled by UI
                System.Console.WriteLine("SamplePricingModel error pricing market " + ex.Message);
            }
        }

        public AppLink.Messaging.SendMode SendMode { get; set; }


        private void SendResponseOrder(MarketData marketData)
        {
            if (marketData.LastQuote == null)
                return;

            if (acknwoledgedQuotes.Contains(marketData.LastQuote.Quote.Id))
                return;

            if (marketData.LastQuote.CanRespond() == false)
                return;


            // please note: this is only a sample calculation to determine 
            // the aggregate market theo, it does not correctly handle swap markets or 
            // correctly handle factoring in hedges
            double marketTheo = 0.0;
            foreach (Option option in marketData.Options)
            {
                marketTheo += option.TheoreticalPrice  * (option.Direction * option.Ratio);
            }

            YJ.AppLink.Messaging.SendOrderArgs order = new AppLink.Messaging.SendOrderArgs(marketData);
            order.Bid = Math.Round(marketTheo * 0.98, 2);
            order.Ask = Math.Round(marketTheo * 1.02, 2);
            order.Handle = marketData.LastQuote.Sender.Name;
            order.SendFromDesk = marketData.LastQuote.Receiver.ParticipantType == AppLink.Api.ParticipantType.Desk;
            order.SendMode = SendMode;
            session.MessagingService.SendMarketOrder(order);

            acknwoledgedQuotes.Add(marketData.LastQuote.Quote.Id);

        }


        public void AddOpenAPIKey(string key)
        {
            if (apiKeys == null)
                apiKeys = new ArrayList();

            apiKeys.Add(key);

        }

		public void UpdateLegValues(AbstractLeg leg, double theo, double delta, 
			double gamma, double vega, double theta, double level)
		{
			try
			{
				leg.MarketData.ClearTheoreticalOverride();

				if (leg is Option)
				{
					UpdateOptionLeg((Option) leg, theo, delta, gamma, vega, theta);
				}
				else if (leg is Stock)
				{
					UpdateStockLeg((Stock) leg, level);
				}
                else if (leg is Swap)
                {
                    UpdateSwapLeg((Swap)leg, theo);
                }

			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void UpdateOptionLeg(Option option, double theo, double delta, 
			double gamma, double vega, double theta)
		{
			option.TheoreticalPrice = theo;
			option.Delta = delta;
			option.Gamma = gamma;
			option.Vega = vega;
			option.Theta = theta;


			option.SendLegUpdate();

		}

		public void UpdateStockLeg(Stock stock, double price)
		{
			stock.MyPrice = price;
			
			stock.SendLegUpdate();
		}

        public void UpdateSwapLeg(Swap swap, double theo)
        {
            swap.TheoreticalPrice = theo;

            swap.SendLegUpdate();
        }

        public void getItgPrices(MarketData mkt)
        {
            itgpricer.mkts = pricingService.GetAllMarkets();
            itgpricer.getStockPrice(mkt.Description.Substring(0, mkt.Description.IndexOf(" ")));

            if (mkt.Options != null)
            {
                foreach (Option leg in mkt.Options)
                {
                    BbgPricer bbgPricer = null;
                    try
                    {
                        itgpricer.getOptPrice(leg.OSICode);
                    }
                    catch (Exception exception)
                    {
                        string errorMsg = "Exception: " + exception.Message;
                    }
                
                
                }

            }
        }
		public void PriceMarket(MarketData mkt)
		{
			try
			{
				mkt.ClearTheoreticalOverride();
                //itgpricer.pConnect("tcp=simulation.itg.com:40002", "rbchansim", "Rb123!");
                

                double undPrice = 0.0;
                double liveUndPrice = 0.0;
                double liveBid = 0.0;
                double liveAsk = 0.0;
                double theoDelta = 0.0;
                bool finishPricing = true;
                DatabaseLogger _dbhelper = new DatabaseLogger();
                if (mkt.Stocks != null)
                {
                    foreach (Stock leg in mkt.Stocks)
                    {
                        PriceStockLeg(leg);
                        undPrice = leg.CrossLevel;
                    }
                }

                

                if (mkt.Options != null)
                {
                    foreach (Option leg in mkt.Options)
                    {
                        try
                        {
                            //undPrice = (itgpricer.getPriceBid(mkt.Description.Substring(0, mkt.Description.IndexOf(" ")))
                            //    + itgpricer.getPriceAsk(mkt.Description.Substring(0, mkt.Description.IndexOf(" ")))) / 2;
                            liveUndPrice = undPrice;
                            //liveUndPrice = -1;
                        }
                        catch(Exception exception)
                        {
                            string errorMsg = "Exception: " + exception.Message;
                        }

                        //if (bbgPricer != null)
                        //{
                            if (leg.Direction > 0)
                            {
                                //liveAsk += (itgpricer.getPriceAsk(leg.OSICode) * leg.Ratio * leg.Direction);
                                //liveBid += (itgpricer.getPriceBid(leg.OSICode) * leg.Ratio * leg.Direction);
                            }
                            else
                            {
                                //liveAsk += (itgpricer.getPriceBid(leg.OSICode) * leg.Ratio * leg.Direction);
                                //liveBid += (itgpricer.getPriceAsk(leg.OSICode) * leg.Ratio * leg.Direction);
                            }
                        //}
                            liveAsk = -1;
                            liveBid = -1;
                        if (undPrice == 0)
                        {
                            undPrice = _dbhelper.getUndPriceWE(leg);
                        }
                        PriceOptionLeg(leg, undPrice);
                        theoDelta += leg.Delta * leg.Ratio * leg.Direction;
                    }
                    if (undPrice != 0) //if there is no stock leg
                    {
                        if (double.IsNaN(liveAsk))
                            liveAsk = -1;
                        else if (double.IsNaN(liveBid))
                            liveBid = -1;
                        else
                        {
                            if (!(mkt.Stocks.Length == 0))
                            {
                                if (theoDelta < 0)
                                {
                                    liveBid += (mkt.Stocks[0].CrossLevel - liveUndPrice) * mkt.Stocks[0].Ratio * mkt.Stocks[0].Direction;
                                    liveAsk += (mkt.Stocks[0].CrossLevel - liveUndPrice) * mkt.Stocks[0].Ratio * mkt.Stocks[0].Direction;
                                }
                                else
                                {
                                    liveBid += (liveUndPrice - mkt.Stocks[0].CrossLevel) * mkt.Stocks[0].Ratio * mkt.Stocks[0].Direction;
                                    liveAsk += (liveUndPrice - mkt.Stocks[0].CrossLevel) * mkt.Stocks[0].Ratio * mkt.Stocks[0].Direction;
                                }
                            }

                        }
                    }

                }


                if (mkt.Swaps != null)
                {
                    foreach (Swap leg in mkt.Swaps)
                    {
                        PriceSwapLeg(leg);
                    }
                }

                // add api keys
                if (apiKeys == null || !apiKeys.Contains("livebid"))
                {
                    AddOpenAPIKey("livebid");
                    AddOpenAPIKey("liveask");
                    AddOpenAPIKey("stockref");
                }

                DatabaseLogger dbHelper = new DatabaseLogger();
                string[] predResults;
                float vega3m = 0;
                string nm_sector = "";
                if (apiKeys != null && apiKeys.Count > 0)
                {
                    //foreach (string key in apiKeys)
                    //{
                        //mkt.AddOpenAPIKeyValue(key, GetRandomPrice().ToString("F2"));
                        mkt.AddOpenAPIKeyValue("livebid", Convert.ToString(Math.Round(liveBid,4)));
                        mkt.AddOpenAPIKeyValue("liveask", Convert.ToString(Math.Round(liveAsk,4)));
                        if (liveUndPrice == null || liveUndPrice == 0)
                            mkt.AddOpenAPIKeyValue("stockref", Convert.ToString(Math.Round(undPrice, 4)));
                        else
                            mkt.AddOpenAPIKeyValue("stockref", Convert.ToString(Math.Round(liveUndPrice, 4)));
                        predResults = dbHelper.getPrediction(mkt.Description.Substring(0, mkt.Description.IndexOf(" ")));
                        nm_sector = dbHelper.getSectorName(mkt.Description.Substring(0, mkt.Description.IndexOf(" ")));
                        vega3m = dbHelper.getVega3m(mkt.Description.Substring(0, mkt.Description.IndexOf(" ")));
                        mkt.AddOpenAPIKeyValue("vega3m", Convert.ToString(Math.Round(vega3m, 0)));
                        mkt.AddOpenAPIKeyValue("sector", nm_sector);
                        // if there is a bid
                        if (double.IsNaN(mkt.BestAsk) && !double.IsNaN(mkt.BestBid))
                        {
                            //buy
                            if (Convert.ToDouble(predResults[4]) > .7)
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4], System.Drawing.Color.Empty, System.Drawing.Color.Red);
                            else
                            {
                                //if we are long out of rank then SOUND
                              if (vega3m > 1500 && Convert.ToDouble(predResults[4]) > .3)
                                {
                                    //SELL!!!
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\apps\sounds\laser.wav");
                                    player.Play();
                                    
                                }
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4]);
                            }
                        }
                        else if (double.IsNaN(mkt.BestBid) && !double.IsNaN(mkt.BestBid))
                        {
                            //sell
                            if (Convert.ToDouble(predResults[4]) < .3)
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4], System.Drawing.Color.Empty, System.Drawing.Color.LightGreen);
                            else
                            {
                                //if we are short and out of rank then SOUND
                                if (vega3m < -1500 && Convert.ToDouble(predResults[4]) < .7)
                                {
                                    //buy back
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\apps\sounds\laser.wav");
                                    player.Play();                                    
                                }
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4]);
                            }
                        }
                        else
                        {
                            if (Convert.ToDouble(predResults[4]) > .7)
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4], System.Drawing.Color.Empty, System.Drawing.Color.Red);
                            else if (Convert.ToDouble(predResults[4]) < .3)
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4], System.Drawing.Color.Empty, System.Drawing.Color.LightGreen);
                            else
                                mkt.AddOpenAPIKeyValue("dualrank", predResults[4]);
                        }

                        //if we have a position then COLOR
                        if (vega3m > 1500 || vega3m < -1500)
                        {
                            mkt.AddOpenAPIKeyValue("dualrank", predResults[4], System.Drawing.Color.Empty, System.Drawing.Color.LightGray);
                        }
                        mkt.AddOpenAPIKeyValue("imppred", predResults[0]);
                        mkt.AddOpenAPIKeyValue("3mimp", predResults[1]);
                        mkt.AddOpenAPIKeyValue("rlzlinpred", predResults[2]);
                        mkt.AddOpenAPIKeyValue("rlzlogpred", predResults[3]);
                        mkt.AddOpenAPIKeyValue("predupdate", predResults[5]);
                        /*
                        if (Convert.ToDouble(predResults[2]) == 1)
                            mkt.AddOpenAPIKeyValue("predupdate", "live");
                        else if (Convert.ToDouble(predResults[2]) == 0)
                            mkt.AddOpenAPIKeyValue("predupdate", "close");
                        else
                            mkt.AddOpenAPIKeyValue("predupdate", "error");
                        */
                    //mkt.Description.Substring(0, mkt.Description.IndexOf(" "))
                    //}
                }
			
				mkt.SendPriceResponse();
			}
			catch (Exception e)
			{
				throw e;
			}
		}

        private void PriceOptionLeg(Option leg, double undPrice)
		{
            mhModels mh_models;
            double liveOptMidPrice= 0.0;
            DatabaseLogger db = new DatabaseLogger();
            //if (bbgPricer != null)
                //liveOptMidPrice = (itgpricer.getPriceBid(leg.OSICode) + itgpricer.getPriceAsk(leg.OSICode)) / 2.0;

            //if (bbgPricer == null)
            //    mh_models = new mhModels(leg, 0, 0, 0);
            //else if (undPrice == 0)
            //    mh_models = new mhModels(leg, undPrice, undPrice, liveOptMidPrice);
            //else
                mh_models = new mhModels(leg, undPrice, undPrice, liveOptMidPrice);
                WETheo wetheo = db.getWETheo(leg);
			//leg.TheoreticalPrice = mh_models.mh_thvalue;
            leg.Delta = mh_models.mh_delta;
            leg.Vega = mh_models.mh_vega;
            leg.Gamma = mh_models.mh_gamma;
            leg.Theta = mh_models.mh_theta;
            leg.Vol = mh_models.mh_impvol;
            if (undPrice > 0)
                leg.TheoreticalPrice = wetheo.pr_theo - ((wetheo.pr_stk - undPrice) * wetheo.am_delta);
            else
                leg.TheoreticalPrice = wetheo.pr_theo;
            /*if (id_buy_sell > 0)
                leg.TheoreticalPrice = bbgPricer.askPrice;
            else if (id_buy_sell < 0)
                leg.TheoreticalPrice = bbgPricer.bidPrice;
            else
                leg.TheoreticalPrice = (bbgPricer.bidPrice + bbgPricer.askPrice) / 2;
                */
		}

		private double GetRandomPrice()
		{
			return GetRandomVal(1000);
		}

		private double GetRandomGreek()
		{
			return GetRandomVal(100);
		}

		private double GetRandomVal(int range)
		{
			Thread.Sleep(10); // make sure gets a new random
			Random r = new Random();
			int i = r.Next(range);
			return ((double)i)/100.00;
		}

		private void PriceStockLeg(Stock leg)
		{
            leg.MyPrice = leg.CrossLevel;
		}

        private void PriceSwapLeg(Swap leg)
        {
            leg.TheoreticalPrice = GetRandomPrice();
        }

        private double AdjustTheoWithCrossLevels(double theo, Stock stock, BbgPricer bbgPricer)
        {
            double levelDiff = stock.CrossLevel - bbgPricer.undPrice;
            double termDelta = stock.Ratio;
                
            return theo;
        }

	}
}
