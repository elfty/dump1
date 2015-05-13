using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.Data;
using System.Threading;

using YJ.AppLink;
using YJ.AppLink.Pricing;
using csclientnet;

namespace YJ.Sample
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class SampleForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
        
        /// 
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.GroupBox connBox;
		private System.Windows.Forms.Button connectBtn;
		private System.Windows.Forms.Button closeBtn;
		private System.Windows.Forms.Label portLbl;
		private System.Windows.Forms.Label ipLabel;
		private System.Windows.Forms.TextBox ipTb;
		private System.Windows.Forms.TextBox portTb;
		private System.Windows.Forms.Label statusLbl;
		private System.Windows.Forms.Label errorLbl;
		private System.Windows.Forms.GroupBox mktsGb;
		private System.Windows.Forms.ListBox markets;
		private System.Windows.Forms.CheckBox autoPrice;
		private System.Windows.Forms.TextBox defaultPriceTb;
		private System.Windows.Forms.ListBox legs;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox theoTb;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox deltaTb;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox gammaTb;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox vegaTb;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox thetaTb;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label mktInfo;
		private System.Windows.Forms.Button updateLeg;
		private System.Windows.Forms.Button overrideSelectedBtn;
		private System.Windows.Forms.Button priceSelectedBtn;
		private System.Windows.Forms.Button priceAllBtn;
		private System.Windows.Forms.TextBox levelTb;
		
		private bool connected;
        private bool autorun;
        private string ipaddress;

		private SamplePricingModel samplePricingModel;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private ColorDialog colorDialog1;
		private Session session;
        private ItgPricer itgpricer;

        private Color foreGround = Color.Empty;
        private GroupBox groupBox2;
        private Label label16;
        private Label label15;
        private ComboBox sendModeCb;
        private Button sendOrder;
        private TextBox offerSizeTb;
        private TextBox offerTb;
        private Label label14;
        private TextBox bidTb;
        private Label label12;
        private TextBox bidSizeTb;
        private Label label11;
        private Button sendMessageBtn;
        private ComboBox contactsCb;
        private TextBox sendMessageTb;
        private CheckBox autoReply;
        private GroupBox groupBox1;
        private Label label10;
        private Button button1;
        private Button foreGroundBtn;
        private Button autoAdd;
        private Button sendOpenAPIValue;
        private Label labelOpen;
        private TextBox openAPIValueTb;
        private Label label9;
        private TextBox openAPIKeyTB;
        private Label label18;
        private Label label17;
        private Label label13;
        private Color backGround = Color.Empty;

        private string m_sPart;
        private bool m_bConnected;

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

        /// </summary>


		public SampleForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// create and configure AppLink session 
			// and subscribe to events
            //itgpricer = new ItgPricer();

			session = new YJ.AppLink.Session();
			session.ThreadSafeGUIControl = this;
			session.StateChanged +=new StateChangedHandler(session_StateChanged);
            session.Logger.LogLevel = LogLevel.Info;
			session.Logger.MessageLogged +=new MessageLoggedHandler(Logger_MessageLogged);
			session.Error +=new ErrorHandler(session_Error);
			
            samplePricingModel = new YJ.Sample.SamplePricingModel();

			this.ipTb.Text = session.IpAddress;
			this.portTb.Text = session.Port.ToString();
			this.statusLbl.Text = "Status: " + session.State.ToString();
			this.autoPrice.Checked = samplePricingModel.AutoPriceNewMarkets;
			
			UpdateState();

            this.sendModeCb.SelectedIndex = 0;
            UpdateSendMode();

			this.markets.SelectedIndexChanged +=new EventHandler(markets_SelectedIndexChanged);
			this.legs.SelectedIndexChanged +=new EventHandler(legs_SelectedIndexChanged);
            this.sendModeCb.SelectedIndexChanged += new EventHandler(sendModeCb_SelectedIndexChanged);
            this.autoReply.CheckedChanged += new EventHandler(autoReply_CheckedChanged);

			this.Closing +=new CancelEventHandler(SampleForm_Closing);
            
		}

        void autoReply_CheckedChanged(object sender, EventArgs e)
        {
            samplePricingModel.AutoReply = autoReply.Checked;
        }

        void sendModeCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSendMode();
        }

        private void UpdateSendMode()
        {
            samplePricingModel.SendMode = GetSendMode();
        }

        private AppLink.Messaging.SendMode GetSendMode()
        {
            return (sendModeCb.SelectedItem.ToString().Equals("Auto Send")) ? AppLink.Messaging.SendMode.Auto : AppLink.Messaging.SendMode.InsertInTab;
        }

		private void connectBtn_Click(object sender, System.EventArgs e)
		{
			Connect();
		}

		private void Connect()
		{
			try
            {
                ClearError();
                if (autorun)
                {
                    session.IpAddress = ipaddress;
                    session.Port = 5511;
                }
                else
                {
                    session.IpAddress = this.ipTb.Text;
                    session.Port = Convert.ToInt32(this.portTb.Text);
                }

                //itgpricer.pConnect("tcp=simulation.itg.com:40002", "rbchansim", "Rb123!");
                connected = session.Connect();
                if (connected)
                {
                    
                    samplePricingModel.Start(session);
                    //session.PricingService.PriceUpdateRequested += new PriceUpdateRequestedHandler(PricingService_PriceUpdateRequestedPricer);
                    
                    session.PricingService.PriceUpdateRequested += new PriceUpdateRequestedHandler(PricingService_PriceUpdateRequested);
                    session.PricingService.MarketRemoved += new MarketRemovedHandler(PricingService_MarketRemoved);

                    session.PricingService.Start();
                    session.MessagingService.Start();
                    session.MessagingService.SendResponseReceived += new Action<AppLink.Messaging.ResponseReceivedEventArgs>(MessagingService_SendResponseReceived);

                }
            }
			catch (Exception e)
			{
				DisplayError(e.Message);
			}
		}

        void MessagingService_SendResponseReceived(AppLink.Messaging.ResponseReceivedEventArgs obj)
        {
            if (obj.Success == false)
            {
                ; // implement handling or logging for message sending errors
            }
        }

		private void PricingService_PriceUpdateRequested(object sender, PriceUpdateRequestedEventArgs args)
		{
			bool add = true;
            DatabaseLogger dblog = new DatabaseLogger();
			foreach (MarketDataWrapper m in this.markets.Items)
			{
				if (m.MarketData.Id == args.Market.Id)
				{
					m.MarketData  = args.Market;
					add = false;
				}
			}

			if (add)
			{
				this.markets.Items.Add(new MarketDataWrapper(args.Market));
			}

			MarketDataWrapper md = this.markets.SelectedItem as MarketDataWrapper;
			if (md != null &&
				md.MarketData.Id == args.Market.Id)
			{
				int selectionIndex = this.legs.SelectedIndex;
				UpdateLegList();
				if (selectionIndex >= 0)
					this.legs.SelectedIndex = selectionIndex;
			}

            if (args.Market != null && args.Market.LastQuote != null && args.Market.LastQuote.CanRespond())
            {
                bool deskMode = args.Market.LastQuote.Receiver != null && args.Market.LastQuote.Receiver.ParticipantType == AppLink.Api.ParticipantType.Desk;
                ParticipantWrapper pw = new ParticipantWrapper(args.Market.LastQuote.Sender, deskMode);
                if (this.contactsCb.Items.Contains(pw) == false)
                {
                    this.contactsCb.Items.Add(pw);
                }
                this.contactsCb.SelectedItem = pw;

            }

            dblog.insertNewMarket(args.Market);
            //itgpricer.getStockPrice(args.Market.Options[0].OSICode.Substring(0,args.Market.Options[0].OSICode.IndexOf(" ")));                      

            
		}

		private void PricingService_MarketRemoved(object sender, MarketRemovedEventArgs args)
		{
			try
			{				
				MarketDataWrapper mkt = this.markets.SelectedItem as MarketDataWrapper;
				if (mkt != null &&
					mkt.MarketData.Id == args.MarketId)
				{
					this.legs.Items.Clear();
					this.ClearInputs();
					this.markets.ClearSelected();
				}

				MarketDataWrapper removedMarket = null;
				foreach (MarketDataWrapper md in this.markets.Items)
				{
					if (md.MarketData.Id == args.MarketId)
					{
						removedMarket = md;
						break;
					}
				}
				if (removedMarket != null)
					this.markets.Items.Remove(removedMarket);

			}
			catch (Exception e)
			{
				DisplayError(e.Message);
			}
		}

		private void session_StateChanged(object sender, StateChangedEventArgs args)
		{
			this.statusLbl.Text = "Status: " + session.State.ToString();
			UpdateState();
		}

		private void UpdateState()
		{
			this.connectBtn.Enabled = !session.Connected;
			this.ipTb.Enabled = !session.Connected;
			this.portTb.Enabled = !session.Connected;
			
			this.closeBtn.Enabled = session.Connected;
			this.mktsGb.Enabled = session.Connected;

			try
			{
				
				if (!session.Connected)
				{
					samplePricingModel.Stop();
					session.PricingService.PriceUpdateRequested -=new PriceUpdateRequestedHandler(PricingService_PriceUpdateRequested);

                    session.MessagingService.SendResponseReceived -= new Action<AppLink.Messaging.ResponseReceivedEventArgs>(MessagingService_SendResponseReceived);

                
                }
			}
			catch
			{
				// exception ignored if not listening
			}
		}




		private void ClearInputs()
		{
			this.theoTb.Text = "";
			this.deltaTb.Text = "";
			this.gammaTb.Text = "";
			this.vegaTb.Text = "";
			this.thetaTb.Text = "";
			this.levelTb.Text = "";
		}

		private void SetInputValues(Option leg)
		{
			ClearInputs();
			if (!Double.IsNaN(leg.TheoreticalPrice))
				this.theoTb.Text = leg.TheoreticalPrice.ToString();

			if (!Double.IsNaN(leg.Delta))
				this.deltaTb.Text = leg.Delta.ToString();

			if (!Double.IsNaN(leg.Gamma))
				this.gammaTb.Text = leg.Gamma.ToString();

			if (!Double.IsNaN(leg.Vega))
				this.vegaTb.Text = leg.Vega.ToString();

			if (!Double.IsNaN(leg.Theta))
				this.thetaTb.Text = leg.Theta.ToString();
		}

        private void SetInputValues(Swap leg)
        {
            ClearInputs();

            if (!Double.IsNaN(leg.TheoreticalPrice))
                this.theoTb.Text = leg.TheoreticalPrice.ToString();

        }


		private void SetInputValues(Stock leg)
		{
			ClearInputs();

			if (!Double.IsNaN(leg.MyPrice))
				this.levelTb.Text = leg.MyPrice.ToString();

		}

		private void closeBtn_Click(object sender, System.EventArgs e)
		{
			try
			{
				ClearError();
				session.Close();
			}
			catch (Exception ex)
			{
				DisplayError(ex.Message);
			}
		}

		private void session_Error(object sender, ErrorEventArgs args)
		{
			DisplayError(args.Message);
		}

		private void Logger_MessageLogged(object sender, MessageLoggedEventArgs args)
		{
			System.Console.WriteLine(args.ToString());
		}

		private void ClearError()
		{
			DisplayError("");
		}	

		private void DisplayError(string error)
		{
			this.errorLbl.Text = error;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.connBox = new System.Windows.Forms.GroupBox();
            this.errorLbl = new System.Windows.Forms.Label();
            this.statusLbl = new System.Windows.Forms.Label();
            this.portTb = new System.Windows.Forms.TextBox();
            this.ipTb = new System.Windows.Forms.TextBox();
            this.ipLabel = new System.Windows.Forms.Label();
            this.portLbl = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.connectBtn = new System.Windows.Forms.Button();
            this.mktsGb = new System.Windows.Forms.GroupBox();
            this.legs = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.sendMessageTb = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.sendModeCb = new System.Windows.Forms.ComboBox();
            this.sendOrder = new System.Windows.Forms.Button();
            this.offerSizeTb = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.offerTb = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.bidTb = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.bidSizeTb = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.sendMessageBtn = new System.Windows.Forms.Button();
            this.contactsCb = new System.Windows.Forms.ComboBox();
            this.autoReply = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.foreGroundBtn = new System.Windows.Forms.Button();
            this.autoAdd = new System.Windows.Forms.Button();
            this.sendOpenAPIValue = new System.Windows.Forms.Button();
            this.labelOpen = new System.Windows.Forms.Label();
            this.openAPIValueTb = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.openAPIKeyTB = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.updateLeg = new System.Windows.Forms.Button();
            this.mktInfo = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.levelTb = new System.Windows.Forms.TextBox();
            this.priceSelectedBtn = new System.Windows.Forms.Button();
            this.priceAllBtn = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.thetaTb = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.vegaTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.gammaTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.deltaTb = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.theoTb = new System.Windows.Forms.TextBox();
            this.overrideSelectedBtn = new System.Windows.Forms.Button();
            this.defaultPriceTb = new System.Windows.Forms.TextBox();
            this.autoPrice = new System.Windows.Forms.CheckBox();
            this.markets = new System.Windows.Forms.ListBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.connBox.SuspendLayout();
            this.mktsGb.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // connBox
            // 
            this.connBox.Controls.Add(this.errorLbl);
            this.connBox.Controls.Add(this.statusLbl);
            this.connBox.Controls.Add(this.portTb);
            this.connBox.Controls.Add(this.ipTb);
            this.connBox.Controls.Add(this.ipLabel);
            this.connBox.Controls.Add(this.portLbl);
            this.connBox.Controls.Add(this.closeBtn);
            this.connBox.Controls.Add(this.connectBtn);
            this.connBox.Location = new System.Drawing.Point(8, 8);
            this.connBox.Name = "connBox";
            this.connBox.Size = new System.Drawing.Size(666, 80);
            this.connBox.TabIndex = 0;
            this.connBox.TabStop = false;
            this.connBox.Text = "Connect";
            // 
            // errorLbl
            // 
            this.errorLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorLbl.Location = new System.Drawing.Point(160, 48);
            this.errorLbl.Name = "errorLbl";
            this.errorLbl.Size = new System.Drawing.Size(485, 24);
            this.errorLbl.TabIndex = 7;
            this.errorLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusLbl
            // 
            this.statusLbl.Location = new System.Drawing.Point(8, 48);
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(146, 24);
            this.statusLbl.TabIndex = 6;
            this.statusLbl.Text = "Status:";
            this.statusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // portTb
            // 
            this.portTb.Location = new System.Drawing.Point(457, 16);
            this.portTb.Name = "portTb";
            this.portTb.Size = new System.Drawing.Size(64, 20);
            this.portTb.TabIndex = 5;
            // 
            // ipTb
            // 
            this.ipTb.Location = new System.Drawing.Point(243, 18);
            this.ipTb.Name = "ipTb";
            this.ipTb.Size = new System.Drawing.Size(112, 20);
            this.ipTb.TabIndex = 4;
            // 
            // ipLabel
            // 
            this.ipLabel.Location = new System.Drawing.Point(145, 16);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(80, 24);
            this.ipLabel.TabIndex = 3;
            this.ipLabel.Text = "YJ IP Address:";
            this.ipLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // portLbl
            // 
            this.portLbl.Location = new System.Drawing.Point(393, 16);
            this.portLbl.Name = "portLbl";
            this.portLbl.Size = new System.Drawing.Size(48, 24);
            this.portLbl.TabIndex = 2;
            this.portLbl.Text = "YJ Port:";
            this.portLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // closeBtn
            // 
            this.closeBtn.BackColor = System.Drawing.SystemColors.Control;
            this.closeBtn.Location = new System.Drawing.Point(541, 16);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(104, 23);
            this.closeBtn.TabIndex = 1;
            this.closeBtn.Text = "Close Connection";
            this.closeBtn.UseVisualStyleBackColor = false;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // connectBtn
            // 
            this.connectBtn.BackColor = System.Drawing.SystemColors.Control;
            this.connectBtn.Location = new System.Drawing.Point(8, 16);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(96, 23);
            this.connectBtn.TabIndex = 0;
            this.connectBtn.Text = "Connect to YJ";
            this.connectBtn.UseVisualStyleBackColor = false;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // mktsGb
            // 
            this.mktsGb.Controls.Add(this.legs);
            this.mktsGb.Controls.Add(this.groupBox2);
            this.mktsGb.Controls.Add(this.groupBox1);
            this.mktsGb.Controls.Add(this.label8);
            this.mktsGb.Controls.Add(this.label1);
            this.mktsGb.Controls.Add(this.updateLeg);
            this.mktsGb.Controls.Add(this.mktInfo);
            this.mktsGb.Controls.Add(this.label7);
            this.mktsGb.Controls.Add(this.levelTb);
            this.mktsGb.Controls.Add(this.priceSelectedBtn);
            this.mktsGb.Controls.Add(this.priceAllBtn);
            this.mktsGb.Controls.Add(this.label6);
            this.mktsGb.Controls.Add(this.thetaTb);
            this.mktsGb.Controls.Add(this.label5);
            this.mktsGb.Controls.Add(this.vegaTb);
            this.mktsGb.Controls.Add(this.label4);
            this.mktsGb.Controls.Add(this.gammaTb);
            this.mktsGb.Controls.Add(this.label3);
            this.mktsGb.Controls.Add(this.deltaTb);
            this.mktsGb.Controls.Add(this.label2);
            this.mktsGb.Controls.Add(this.theoTb);
            this.mktsGb.Controls.Add(this.overrideSelectedBtn);
            this.mktsGb.Controls.Add(this.defaultPriceTb);
            this.mktsGb.Controls.Add(this.autoPrice);
            this.mktsGb.Controls.Add(this.markets);
            this.mktsGb.Location = new System.Drawing.Point(8, 96);
            this.mktsGb.Name = "mktsGb";
            this.mktsGb.Size = new System.Drawing.Size(666, 536);
            this.mktsGb.TabIndex = 1;
            this.mktsGb.TabStop = false;
            this.mktsGb.Text = "Markets";
            // 
            // legs
            // 
            this.legs.Location = new System.Drawing.Point(234, 40);
            this.legs.Name = "legs";
            this.legs.Size = new System.Drawing.Size(287, 121);
            this.legs.TabIndex = 5;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.sendMessageTb);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.sendModeCb);
            this.groupBox2.Controls.Add(this.sendOrder);
            this.groupBox2.Controls.Add(this.offerSizeTb);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.offerTb);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.bidTb);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.bidSizeTb);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.sendMessageBtn);
            this.groupBox2.Controls.Add(this.contactsCb);
            this.groupBox2.Controls.Add(this.autoReply);
            this.groupBox2.Location = new System.Drawing.Point(8, 401);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(646, 129);
            this.groupBox2.TabIndex = 38;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Messaging";
            // 
            // sendMessageTb
            // 
            this.sendMessageTb.Location = new System.Drawing.Point(433, 61);
            this.sendMessageTb.Name = "sendMessageTb";
            this.sendMessageTb.Size = new System.Drawing.Size(194, 20);
            this.sendMessageTb.TabIndex = 3;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(263, 82);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(51, 13);
            this.label18.TabIndex = 48;
            this.label18.Text = "Send To:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(432, 45);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(53, 13);
            this.label17.TabIndex = 47;
            this.label17.Text = "Message:";
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(107, 89);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(122, 30);
            this.label16.TabIndex = 46;
            this.label16.Text = "Sends order on selected market ";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(5, 19);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(125, 13);
            this.label15.TabIndex = 19;
            this.label15.Text = "Message Sending Mode:";
            // 
            // sendModeCb
            // 
            this.sendModeCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sendModeCb.FormattingEnabled = true;
            this.sendModeCb.Items.AddRange(new object[] {
            "Auto Send",
            "Insert In Tab"});
            this.sendModeCb.Location = new System.Drawing.Point(135, 16);
            this.sendModeCb.Name = "sendModeCb";
            this.sendModeCb.Size = new System.Drawing.Size(137, 21);
            this.sendModeCb.TabIndex = 18;
            // 
            // sendOrder
            // 
            this.sendOrder.Location = new System.Drawing.Point(6, 96);
            this.sendOrder.Name = "sendOrder";
            this.sendOrder.Size = new System.Drawing.Size(95, 23);
            this.sendOrder.TabIndex = 17;
            this.sendOrder.Text = "Send Order";
            this.sendOrder.UseVisualStyleBackColor = true;
            this.sendOrder.Click += new System.EventHandler(this.sendOrder_Click);
            // 
            // offerSizeTb
            // 
            this.offerSizeTb.Location = new System.Drawing.Point(167, 63);
            this.offerSizeTb.Name = "offerSizeTb";
            this.offerSizeTb.Size = new System.Drawing.Size(47, 20);
            this.offerSizeTb.TabIndex = 15;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(164, 45);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(56, 13);
            this.label13.TabIndex = 16;
            this.label13.Text = "Offer Size:";
            // 
            // offerTb
            // 
            this.offerTb.Location = new System.Drawing.Point(114, 63);
            this.offerTb.Name = "offerTb";
            this.offerTb.Size = new System.Drawing.Size(47, 20);
            this.offerTb.TabIndex = 13;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(113, 45);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(33, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "Offer:";
            // 
            // bidTb
            // 
            this.bidTb.Location = new System.Drawing.Point(61, 61);
            this.bidTb.Name = "bidTb";
            this.bidTb.Size = new System.Drawing.Size(47, 20);
            this.bidTb.TabIndex = 11;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(59, 45);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(25, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "Bid:";
            // 
            // bidSizeTb
            // 
            this.bidSizeTb.Location = new System.Drawing.Point(6, 63);
            this.bidSizeTb.Name = "bidSizeTb";
            this.bidSizeTb.Size = new System.Drawing.Size(47, 20);
            this.bidSizeTb.TabIndex = 6;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(5, 45);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(48, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Bid Size:";
            // 
            // sendMessageBtn
            // 
            this.sendMessageBtn.Location = new System.Drawing.Point(504, 96);
            this.sendMessageBtn.Name = "sendMessageBtn";
            this.sendMessageBtn.Size = new System.Drawing.Size(122, 23);
            this.sendMessageBtn.TabIndex = 5;
            this.sendMessageBtn.Text = "Send Message";
            this.sendMessageBtn.UseVisualStyleBackColor = true;
            this.sendMessageBtn.Click += new System.EventHandler(this.sendMessageBtn_Click);
            // 
            // contactsCb
            // 
            this.contactsCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.contactsCb.FormattingEnabled = true;
            this.contactsCb.Location = new System.Drawing.Point(266, 98);
            this.contactsCb.Name = "contactsCb";
            this.contactsCb.Size = new System.Drawing.Size(137, 21);
            this.contactsCb.TabIndex = 4;
            // 
            // autoReply
            // 
            this.autoReply.Location = new System.Drawing.Point(494, 14);
            this.autoReply.Name = "autoReply";
            this.autoReply.Size = new System.Drawing.Size(132, 24);
            this.autoReply.TabIndex = 2;
            this.autoReply.Text = "Auto reply to quotes";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.foreGroundBtn);
            this.groupBox1.Controls.Add(this.autoAdd);
            this.groupBox1.Controls.Add(this.sendOpenAPIValue);
            this.groupBox1.Controls.Add(this.labelOpen);
            this.groupBox1.Controls.Add(this.openAPIValueTb);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.openAPIKeyTB);
            this.groupBox1.Location = new System.Drawing.Point(8, 277);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(646, 113);
            this.groupBox1.TabIndex = 37;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Open API";
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(445, 73);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(186, 30);
            this.label10.TabIndex = 45;
            this.label10.Text = "Sends Open API values on selected market";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(198, 80);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 23);
            this.button1.TabIndex = 44;
            this.button1.Text = "SetBackground";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // foreGroundBtn
            // 
            this.foreGroundBtn.Location = new System.Drawing.Point(102, 80);
            this.foreGroundBtn.Name = "foreGroundBtn";
            this.foreGroundBtn.Size = new System.Drawing.Size(89, 23);
            this.foreGroundBtn.TabIndex = 43;
            this.foreGroundBtn.Text = "SetForeground";
            this.foreGroundBtn.UseVisualStyleBackColor = true;
            this.foreGroundBtn.Click += new System.EventHandler(this.foreGroundBtn_Click);
            // 
            // autoAdd
            // 
            this.autoAdd.BackColor = System.Drawing.SystemColors.Control;
            this.autoAdd.Location = new System.Drawing.Point(102, 37);
            this.autoAdd.Name = "autoAdd";
            this.autoAdd.Size = new System.Drawing.Size(153, 23);
            this.autoAdd.TabIndex = 42;
            this.autoAdd.Text = "Add Key To Auto Pricing";
            this.autoAdd.UseVisualStyleBackColor = false;
            this.autoAdd.Click += new System.EventHandler(this.autoAdd_Click);
            // 
            // sendOpenAPIValue
            // 
            this.sendOpenAPIValue.BackColor = System.Drawing.SystemColors.Control;
            this.sendOpenAPIValue.Location = new System.Drawing.Point(301, 79);
            this.sendOpenAPIValue.Name = "sendOpenAPIValue";
            this.sendOpenAPIValue.Size = new System.Drawing.Size(138, 23);
            this.sendOpenAPIValue.TabIndex = 41;
            this.sendOpenAPIValue.Text = "Send Open API Value";
            this.sendOpenAPIValue.UseVisualStyleBackColor = false;
            this.sendOpenAPIValue.Click += new System.EventHandler(this.sendOpenAPIValue_Click);
            // 
            // labelOpen
            // 
            this.labelOpen.AutoSize = true;
            this.labelOpen.Location = new System.Drawing.Point(8, 66);
            this.labelOpen.Name = "labelOpen";
            this.labelOpen.Size = new System.Drawing.Size(83, 13);
            this.labelOpen.TabIndex = 40;
            this.labelOpen.Text = "Open API Value";
            // 
            // openAPIValueTb
            // 
            this.openAPIValueTb.Location = new System.Drawing.Point(8, 82);
            this.openAPIValueTb.Name = "openAPIValueTb";
            this.openAPIValueTb.Size = new System.Drawing.Size(87, 20);
            this.openAPIValueTb.TabIndex = 39;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 19);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 38;
            this.label9.Text = "Open API Key";
            // 
            // openAPIKeyTB
            // 
            this.openAPIKeyTB.Location = new System.Drawing.Point(8, 38);
            this.openAPIKeyTB.Name = "openAPIKeyTB";
            this.openAPIKeyTB.Size = new System.Drawing.Size(82, 20);
            this.openAPIKeyTB.TabIndex = 37;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(234, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(152, 24);
            this.label8.TabIndex = 24;
            this.label8.Text = "Legs";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(470, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(184, 24);
            this.label1.TabIndex = 23;
            this.label1.Text = "Selected Leg Values";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // updateLeg
            // 
            this.updateLeg.BackColor = System.Drawing.SystemColors.Control;
            this.updateLeg.Location = new System.Drawing.Point(574, 203);
            this.updateLeg.Name = "updateLeg";
            this.updateLeg.Size = new System.Drawing.Size(80, 23);
            this.updateLeg.TabIndex = 22;
            this.updateLeg.Text = "Update Leg";
            this.updateLeg.UseVisualStyleBackColor = false;
            this.updateLeg.Click += new System.EventHandler(this.updateLeg_Click);
            // 
            // mktInfo
            // 
            this.mktInfo.Location = new System.Drawing.Point(240, 168);
            this.mktInfo.Name = "mktInfo";
            this.mktInfo.Size = new System.Drawing.Size(201, 64);
            this.mktInfo.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(456, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(135, 24);
            this.label7.TabIndex = 20;
            this.label7.Text = "Stock Price:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // levelTb
            // 
            this.levelTb.Location = new System.Drawing.Point(598, 171);
            this.levelTb.Name = "levelTb";
            this.levelTb.Size = new System.Drawing.Size(56, 20);
            this.levelTb.TabIndex = 19;
            // 
            // priceSelectedBtn
            // 
            this.priceSelectedBtn.BackColor = System.Drawing.SystemColors.Control;
            this.priceSelectedBtn.Location = new System.Drawing.Point(510, 248);
            this.priceSelectedBtn.Name = "priceSelectedBtn";
            this.priceSelectedBtn.Size = new System.Drawing.Size(144, 23);
            this.priceSelectedBtn.TabIndex = 18;
            this.priceSelectedBtn.Text = "Price Selected Market";
            this.priceSelectedBtn.UseVisualStyleBackColor = false;
            this.priceSelectedBtn.Click += new System.EventHandler(this.priceSelectedBtn_Click);
            // 
            // priceAllBtn
            // 
            this.priceAllBtn.BackColor = System.Drawing.SystemColors.Control;
            this.priceAllBtn.Location = new System.Drawing.Point(400, 248);
            this.priceAllBtn.Name = "priceAllBtn";
            this.priceAllBtn.Size = new System.Drawing.Size(104, 23);
            this.priceAllBtn.TabIndex = 17;
            this.priceAllBtn.Text = "Price All Markets";
            this.priceAllBtn.UseVisualStyleBackColor = false;
            this.priceAllBtn.Click += new System.EventHandler(this.priceAllBtn_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(487, 147);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 24);
            this.label6.TabIndex = 16;
            this.label6.Text = "Theta:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // thetaTb
            // 
            this.thetaTb.Location = new System.Drawing.Point(598, 147);
            this.thetaTb.Name = "thetaTb";
            this.thetaTb.Size = new System.Drawing.Size(56, 20);
            this.thetaTb.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(487, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 24);
            this.label5.TabIndex = 14;
            this.label5.Text = "Vega:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // vegaTb
            // 
            this.vegaTb.Location = new System.Drawing.Point(598, 123);
            this.vegaTb.Name = "vegaTb";
            this.vegaTb.Size = new System.Drawing.Size(56, 20);
            this.vegaTb.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(508, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 24);
            this.label4.TabIndex = 12;
            this.label4.Text = "Gamma:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gammaTb
            // 
            this.gammaTb.Location = new System.Drawing.Point(598, 99);
            this.gammaTb.Name = "gammaTb";
            this.gammaTb.Size = new System.Drawing.Size(56, 20);
            this.gammaTb.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(530, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 24);
            this.label3.TabIndex = 10;
            this.label3.Text = "Delta:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // deltaTb
            // 
            this.deltaTb.Location = new System.Drawing.Point(598, 75);
            this.deltaTb.Name = "deltaTb";
            this.deltaTb.Size = new System.Drawing.Size(56, 20);
            this.deltaTb.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(519, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 24);
            this.label2.TabIndex = 8;
            this.label2.Text = "Theoretical:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // theoTb
            // 
            this.theoTb.Location = new System.Drawing.Point(598, 51);
            this.theoTb.Name = "theoTb";
            this.theoTb.Size = new System.Drawing.Size(56, 20);
            this.theoTb.TabIndex = 7;
            // 
            // overrideSelectedBtn
            // 
            this.overrideSelectedBtn.BackColor = System.Drawing.SystemColors.Control;
            this.overrideSelectedBtn.Location = new System.Drawing.Point(8, 248);
            this.overrideSelectedBtn.Name = "overrideSelectedBtn";
            this.overrideSelectedBtn.Size = new System.Drawing.Size(136, 23);
            this.overrideSelectedBtn.TabIndex = 6;
            this.overrideSelectedBtn.Text = "Override Selected Price";
            this.overrideSelectedBtn.UseVisualStyleBackColor = false;
            this.overrideSelectedBtn.Click += new System.EventHandler(this.overrideSelectedBtn_Click);
            // 
            // defaultPriceTb
            // 
            this.defaultPriceTb.Location = new System.Drawing.Point(160, 248);
            this.defaultPriceTb.Name = "defaultPriceTb";
            this.defaultPriceTb.Size = new System.Drawing.Size(64, 20);
            this.defaultPriceTb.TabIndex = 2;
            // 
            // autoPrice
            // 
            this.autoPrice.Checked = true;
            this.autoPrice.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoPrice.Location = new System.Drawing.Point(8, 16);
            this.autoPrice.Name = "autoPrice";
            this.autoPrice.Size = new System.Drawing.Size(80, 24);
            this.autoPrice.TabIndex = 1;
            this.autoPrice.Text = "Auto Price";
            this.autoPrice.CheckedChanged += new System.EventHandler(this.autoPrice_CheckedChanged);
            // 
            // markets
            // 
            this.markets.Location = new System.Drawing.Point(8, 40);
            this.markets.Name = "markets";
            this.markets.Size = new System.Drawing.Size(216, 186);
            this.markets.TabIndex = 0;
            // 
            // SampleForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(683, 635);
            this.Controls.Add(this.mktsGb);
            this.Controls.Add(this.connBox);
            this.Name = "SampleForm";
            this.Text = "AppLink Sample";
            this.Load += new System.EventHandler(this.SampleForm_Load);
            this.connBox.ResumeLayout(false);
            this.connBox.PerformLayout();
            this.mktsGb.ResumeLayout(false);
            this.mktsGb.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new SampleForm()); 
		}

        private void ProcessCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                string[] kv = arg.Split(':');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "-ip":
                            if (kv.Length < 2)
                            {
                                continue;
                            }

                            int value;
                            
                            ipaddress = kv[1];
                            if (int.TryParse(kv[1], out value))
                            {
                                //InsertNumOfRows = value;
                            }
                            break;

                        case "-autorun":
                            autorun = true;
                            break;
                    }
                }
            }
        }
		private void UpdateLegList()
		{
			try
			{
				ClearInputs();
				this.legs.Items.Clear();		
				MarketDataWrapper md = this.markets.SelectedItem as MarketDataWrapper;
				if (md != null)
				{
					MarketData mkt = md.MarketData;
					if (mkt.Options != null &&
                        mkt.Options.Length > 0)
					{
                        foreach (Option option in mkt.Options)
						{
							legs.Items.Add(option);	
						}

					}
					
					if (mkt.Stocks != null &&
                        mkt.Stocks.Length > 0)
					{
                        foreach (Stock s in mkt.Stocks)
						{
							legs.Items.Add(s);	
						}
					}

                    if (mkt.Swaps != null &&
                        mkt.Swaps.Length > 0)
                    {
                        foreach (Swap s in mkt.Swaps)
                        {
                            legs.Items.Add(s);
                        }
                    }

					if (mkt.LastPricedTime > DateTime.MinValue)
					{
						this.mktInfo.Text = "Last Priced At: \n" + mkt.LastPricedTime.ToLongTimeString();
					}
					else
					{
						this.mktInfo.Text = "Market not yet priced";
					}
				}
			}
			catch (Exception ex)
			{
				DisplayError(ex.Message);
			}
		}

		private void markets_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateLegList();
		}


		private void overrideSelectedBtn_Click(object sender, EventArgs e)
		{
			try
			{
				ClearError();
				MarketDataWrapper mkt = this.markets.SelectedItem as MarketDataWrapper;
				if (mkt != null)
				{
					double theo = Convert.ToDouble(this.defaultPriceTb.Text);
					samplePricingModel.SetOverridePrice(mkt.MarketData, theo);
				}
			}
			catch (Exception ex)
			{
				DisplayError(ex.Message);
			}
		}

		private void priceSelectedBtn_Click(object sender, EventArgs e)
		{
			try
			{
				ClearError();
				MarketDataWrapper mkt = this.markets.SelectedItem as MarketDataWrapper;
				if (mkt != null)
				{
                    samplePricingModel.getItgPrices(mkt.MarketData);
					//samplePricingModel.PriceMarket(mkt.MarketData);
					UpdateSelectedLegValues();
				}
			}
			catch (Exception ex)
			{
				DisplayError(ex.Message);
			}
		}

		private void autoPrice_CheckedChanged(object sender, EventArgs e)
		{
			samplePricingModel.AutoPriceNewMarkets = autoPrice.Checked;
		}

		private void legs_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateSelectedLegValues();
		}

		private void UpdateSelectedLegValues()
		{
			try
			{
				object leg = this.legs.SelectedItem; 
				if (leg == null)
					return;

				if (leg is Option)
				{
					this.SetInputValues((Option) leg);
					return;
				}

				if (leg is Stock)
				{
					this.SetInputValues((Stock) leg);
					return;
				}

                if (leg is Swap)
                {
                    this.SetInputValues((Swap)leg);
                    return;
                }

			}
			catch (Exception e)
			{
				DisplayError(e.Message);
			}
		}

		private void priceAllBtn_Click(object sender, System.EventArgs e)
		{
			try
			{
				ClearError();
				samplePricingModel.RepriceAll();
				UpdateSelectedLegValues();
			}
			catch (Exception ex)
			{
				DisplayError(ex.Message);
			}
		}

		private void updateLeg_Click(object sender, System.EventArgs e)
		{
			try
			{
				ClearError();
				AbstractLeg leg = this.legs.SelectedItem as AbstractLeg; 
				if (leg == null)
					return;

				double theo = Double.NaN;
				double delta = Double.NaN;
				double gamma = Double.NaN;
				double vega = Double.NaN;
				double theta = Double.NaN;
				double level = Double.NaN;

                if (!Double.TryParse((string)this.theoTb.Text,
                    NumberStyles.Number, new NumberFormatInfo(), out theo))
                {
                    theo = double.NaN;
                }

                if (!Double.TryParse((string)this.deltaTb.Text,
                    NumberStyles.Number, new NumberFormatInfo(), out delta))
                {
                    delta = double.NaN;
                }

                if (!Double.TryParse((string)this.gammaTb.Text,
                    NumberStyles.Number, new NumberFormatInfo(), out gamma))
                {
                    gamma = double.NaN;
                }

                if (!Double.TryParse((string)this.vegaTb.Text,
                    NumberStyles.Number, new NumberFormatInfo(), out vega))
                {
                    vega = double.NaN;
                }

                if (!Double.TryParse((string)this.thetaTb.Text,
                    NumberStyles.Number, new NumberFormatInfo(), out theta))
                {
                    theta = double.NaN;
                }

                if (!Double.TryParse((string)this.levelTb.Text,
                    NumberStyles.Number, new NumberFormatInfo(), out level))
                {
                    level = double.NaN;
                }


				samplePricingModel.UpdateLegValues(leg, theo, delta, gamma, vega, theta, level);
			}
			catch (Exception ex)
			{
				DisplayError(ex.Message);
			}

		}

		private void SampleForm_Closing(object sender, CancelEventArgs e)
		{
			try
			{
				session.Close();
			}
			catch
			{
				// exception ignored
			}
		}

        private void sendOpenAPIValue_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(openAPIKeyTB.Text))
                return;

            MarketDataWrapper mkt = this.markets.SelectedItem as MarketDataWrapper;
            if (mkt != null)
            {
                mkt.MarketData.AddOpenAPIKeyValue(openAPIKeyTB.Text, openAPIValueTb.Text, foreGround, backGround);
                mkt.MarketData.SendOpenApiValues();
            }            
         }

        private void autoAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openAPIKeyTB.Text))
                return;

            samplePricingModel.AddOpenAPIKey(openAPIKeyTB.Text);
        }

        private void foreGroundBtn_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                foreGround = colorDialog1.Color;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                backGround = colorDialog1.Color;
            }
        }

        private void sendOrder_Click(object sender, EventArgs e)
        {
            MarketDataWrapper mkt = this.markets.SelectedItem as MarketDataWrapper;
            if (mkt != null)
            {
                SendOrder(mkt.MarketData);
            }
        }

        private void SendOrder(MarketData marketData)
        {
            try
            {
                if (contactsCb.Items.Count < 1)
                    return;

                ParticipantWrapper participant = contactsCb.SelectedItem as ParticipantWrapper;
                if (participant == null)
                    return;

                YJ.AppLink.Messaging.SendOrderArgs order = new AppLink.Messaging.SendOrderArgs(marketData);

                if (bidTb.Text != string.Empty)
                {
                    order.Bid = Convert.ToDouble(bidTb.Text);
                    if (bidSizeTb.Text != string.Empty)
                    {
                        order.BidSize = Convert.ToInt32(bidSizeTb.Text);
                    }
                }

                if (offerTb.Text != string.Empty)
                {
                    order.Ask = Convert.ToDouble(offerTb.Text);
                    if (offerSizeTb.Text != string.Empty)
                    {
                        order.AskSize = Convert.ToInt32(offerSizeTb.Text);
                    }
                }

                order.Handle = participant.Participant.Name;
                order.SendFromDesk = participant.DeskMode;
                order.SendMode = GetSendMode();
                session.MessagingService.SendMarketOrder(order);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error sending order " + ex.Message);
            }
        }

        private void sendMessageBtn_Click(object sender, EventArgs e)
        {
            if (sendMessageTb.Text != string.Empty && contactsCb.Items.Count > 0)
            {
                ParticipantWrapper participant = contactsCb.SelectedItem as ParticipantWrapper;
                if (participant != null)
                {
                    AppLink.Messaging.SendMessageArgs args = new AppLink.Messaging.SendMessageArgs();
                    args.Handle = participant.Participant.Name;
                    args.SendFromDesk = participant.DeskMode;
                    args.MessageText = sendMessageTb.Text;
                    args.SendMode = GetSendMode();
                    session.MessagingService.SendIMMessage(args);
                }
            }
        }

        private void SampleForm_Load(object sender, EventArgs e)
        {
            
            ProcessCommandLineArgs();
            if (autorun)
            {
                connectBtn.PerformClick();
            }
        }

	}
	
	public class MarketDataWrapper
	{
		public MarketData MarketData;
		
		public MarketDataWrapper (MarketData marketData) 
		{
			this.MarketData = marketData;
		}
		
		public override string ToString()
		{
			return MarketData.ToString ();
		}
	}

    public class ParticipantWrapper
    {
        public YJ.AppLink.Api.ParticipantData Participant { get; set; }
        public bool DeskMode { get; set; }

        public ParticipantWrapper(YJ.AppLink.Api.ParticipantData participant, bool deskMode)
        {
            Participant = participant;
            DeskMode = deskMode;
        }

        public override string ToString()
        {
            return Participant.ToString() + (DeskMode ? "*" : string.Empty);
        }

        public override int GetHashCode()
        {
            return Participant.GetHashCode() + DeskMode.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ParticipantWrapper pw = obj as ParticipantWrapper;
            if (pw == null)
                return false;

            bool ret = (this.Participant.Equals(pw.Participant)) && (DeskMode == pw.DeskMode);
            return ret;
        }
    }

}
