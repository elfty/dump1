using System;
using System.Collections;
using System.Windows.Forms;

using YJ.AppLink.Net;
using YJ.AppLink.Api;
using YJ.AppLink.Pricing;
using YJ.AppLink.Messaging;

namespace YJ.AppLink
{
	public delegate void StateChangedHandler (object sender, StateChangedEventArgs args);
	public delegate void ErrorHandler (object sender, ErrorEventArgs args);

	/// <summary>
	/// The entry point/primary class used to configure and initiate the AppLink SDK.
	/// </summary>
    public class Session
	{
		/// <summary>
		/// Occurs when the connectivity state of the AppLink changes 
		/// </summary>
        public event StateChangedHandler StateChanged;			
		
        /// <summary>
        /// Occurs when an error is detected or exception is thrown
        /// </summary>
        public event ErrorHandler Error;	
								
		public const int DefaultPort = 5511;					
		public const string	DefaultIPAddress = "127.0.0.1";		

		private int	port = Session.DefaultPort;
		private string ipAddr = Session.DefaultIPAddress;
		private string domainName = "";
		
		private Connection conn;
		private MessageListener	listener;
		private MessageSender sender;
		
		private Logger logger;

		private ArrayList services = new ArrayList();
		private PricingService pricingService;
        private MessagingService messagingService;
	
		private Control	threadSafeGUIControl = null;

		private SessionState state = SessionState.Initial;
		private SessionState lastState = SessionState.Initial;
		
		public Session() 
		{
			logger = new Logger(this);
		}

		#region public properties

		/// <summary>
		/// Gets an enum indicating the state of the AppLink connection
		/// </summary>
        public SessionState State
		{
			get { return this.state; }
		}

        /// <summary>
        /// Gets an indication if the AppLink is currently connected
        /// </summary>
		public bool Connected
		{
			get { return (state == SessionState.Connected); }
		}

        /// <summary>
        /// Gets or sets the Port used to establish the connection
        /// </summary>
		public int Port
		{
			get { return this.port; }
			set 
			{ 
				this.port = value; 
				logger.Debug("Port set to: " + port.ToString(), this);
			}
		}

        /// <summary>
        /// Gets or sets the IpAddress to connect to
        /// </summary>
		public string IpAddress
		{
			get { return this.ipAddr; }
			set
			{ 
				this.ipAddr = value; 
				logger.Debug("IpAddress set to: " + ipAddr, this);
			}
		}

        /// <summary>
        /// Gets or sets a domain name to connect to
        /// </summary>
		public string DomainName
		{
			get { return this.domainName; }
			set
			{
				this.domainName = value;
				logger.Debug("DomainName set to: " + domainName, this);
			}
		}

        /// <summary>
        /// Gets or sets a Windows Form Control used to 
        /// marshall asynchronous/background thread events to the main GUI thread.
        /// </summary>
        /// <remarks>
        /// If the AppLink is being used inside a Window Form application it is
        /// recommended to set this to a persistent Form or Control to ensure thread safety.
        /// </remarks>
		public Control ThreadSafeGUIControl
		{
			get { return this.threadSafeGUIControl; }
			set 
			{ 
				this.threadSafeGUIControl = value; 
				logger.Debug("Setting threadSafeGUIControl", this);
			}
		}

        /// <summary>
        /// Gets a reference to the PricingService
        /// </summary>
		public PricingService PricingService
		{
			get
			{
				if (pricingService == null)
					pricingService = new PricingService(this);
			
				return pricingService;
			}
		}

        /// <summary>
        /// Gets a reference to the MessagingService
        /// </summary>
        public MessagingService MessagingService
        {
            get
            {
                if (messagingService == null)
                    messagingService = new MessagingService(this);

                return messagingService;
            }
        }

        /// <summary>
        /// Gets a reference to the Logger
        /// </summary>
		public Logger Logger
		{
			get { return logger; }
		}

		#endregion

		#region public methods

        /// <summary>
        /// Attempts to establish a TCP socket connection to the YJ Energy application
        /// using the specified IP address or domain and port
        /// </summary>
        /// <returns>
        /// Returns true if the connection attempt was successful, otherwise it throws an exception.
        /// </returns>
		public bool Connect()
		{
			try
			{
				if (state != SessionState.Connected)
				{
					logger.Info("Connecting", this);
					
					conn = new Connection(this);
					conn.IpAddress = ipAddr;
					conn.Port = port;
					conn.DomainName = domainName;
					bool success = conn.Connect();

                    //sessionCs.Connect("tcp=simulation.itg.com:40002", "rbchansim", "Rb123!", csclientnet.SecurityType.None);
					if (success)
					{
						logger.Debug("Socket connected", this);
						
						sender = conn.MessageSender;
						sender.Start();
						logger.Debug("Sender created", this);
						
						listener = conn.MessageListener;
						logger.Debug("Listener created", this);
						
						listener.Start();
						logger.Debug("Listener started", this);
						
						OnStateChanged(SessionState.Connected);
					}
					else
					{
						logger.Error("Unable to connect", this);
						throw new Exception("Unknown error connecting"); 
					}
				}
				else
				{
					logger.Info("Already connected", this);
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to establish connection", this, e);
				throw e;
			}

			return Connected;
		}

		/// <summary>
		/// Stops all services and closes the socket connection
		/// </summary>
		public void Close()
		{

			logger.Debug("Session closing", this);
			Exception finalException = null;
			try
			{
				if (services.Count > 0)
				{
					// gets cloned b/c services is modified when a service is stopped
					ArrayList tempList = (ArrayList)services.Clone();
					foreach (Service service in tempList)
					{
						service.Stop();
					}
					services.Clear();
				}
				logger.Debug("Services stopped", this);
			}
			catch (Exception e)
			{
				OnSessionError("Failure stopping services", this, e);
				finalException = e;
			}
				
			try
			{
				listener.Stop();
				sender.Stop();
				conn.Close();
				logger.Debug("Connection closed", this);
			}
			catch (Exception e)
			{
				OnSessionError("Failure closing connection", this, e);
				finalException = e;
			}

			OnStateChanged(SessionState.Closed);
			logger.Info("Session closed", this);
			
			if (finalException != null)
				throw finalException;
		}

		#endregion

		#region internal properties	
	
		internal MessageSender MessageSender
		{
			get { return this.sender; }
		}

		internal MessageListener MessageListener
		{
			get { return this.listener; }
		}

		#endregion

		#region internal methods
		
		internal delegate void ConnectionFailureCallback(Exception e);
		internal void ConnectionFailure(Exception e)
		{
			if (threadSafeGUIControl != null &&
				threadSafeGUIControl.InvokeRequired)
			{
				threadSafeGUIControl.BeginInvoke(new ConnectionFailureCallback(ConnectionFailure), 
					new object[] {e});
				return;

			}			
			
			if (Connected)
			{
				try
				{
					Close();
				}
				catch (Exception ex)
				{
					logger.Warn("Error closing on connection failure", this, ex);
				}

				OnSessionError("Connection failed: " + e.Message, this, e);
                OnStateChanged(SessionState.Error);
                if (e.Message == "An existing connection was forcibly closed by the remote host")
                {
                    Environment.Exit(0);  
                }
			}
		}

		internal delegate void OnSessionErrorCallback(string message, object sender, Exception e);
		internal void OnSessionError(string message, object sender, Exception e)
		{
			if (threadSafeGUIControl != null &&
				threadSafeGUIControl.InvokeRequired)
			{
				threadSafeGUIControl.BeginInvoke(new OnSessionErrorCallback(OnSessionError), 
					new object[] {message, sender, e});
				return;

			}
			
			if (Error != null)
			{
				Error(this, new ErrorEventArgs(message, e));
			}

			logger.Error(message, sender, e);
		}

		internal void StopService(Service service)
		{
			try
			{
				if (service == null)
				{	
					logger.Warn("Cannot stop null service", this);
					return;
				}
				
				logger.Info("Stopping service: " + service.ToString(), this);
				
				if (Connected)
				{
					StopServiceRequest req  = new StopServiceRequest();
					req.Service = service.ToString();
					sender.Send(req);
				}
				
				if (services.Contains(service))
				{
					services.Remove(service);
					logger.Debug("Service removed: " + service.ToString(), this);
				}
			}
			catch (Exception e)
			{	
				OnSessionError("Error stopping service: " + service.ToString(), this, e); 
				throw e;
			}
		}



		internal void StartService(Service service)
		{
			try
			{
				if (service == null)
				{	
					logger.Warn("Cannot start null service", this);
					return;
				}

				if (service.Running)
				{
					logger.Info("Service: " + service.ToString() + " already running", this);
					return;
				}
				
				if (!Connected)
				{
					throw new Exception("Session not connected");
				}

				if (!services.Contains(service))
					services.Add(service);
			
				StartServiceRequest req  = new StartServiceRequest();
                req.Version = service.Version;
				req.Service = service.ServiceName.ToString();
                sender.Send(req);

			}
			catch (Exception e)
			{
				logger.Error("Failure to start service: " + service.ToString(), this, e);
				throw e;
			}
		}
		
		#endregion
		
		#region private methods
		
		private void OnStateChanged(SessionState s)
		{
			if (s != state)
			{
				lastState = state;
				state = s;
				
				logger.Info("State changed to: " + state.ToString(), this);
				if (StateChanged != null)
				{
					StateChanged(this, new StateChangedEventArgs(state, lastState));
				}
			}
		}
		
		#endregion
	}

    /// <summary>
    /// Enum or AppLink SDK connection states
    /// </summary>
	public enum SessionState
	{
		Initial,            // state when AppLink is started, before a connection is made
		Connected,			// state when connected
		Closed,             // state after connection is manually closed
		Error               // state if connection fails or closes due to an error
	}

	public class StateChangedEventArgs : System.EventArgs
	{
		private SessionState state;
		private SessionState lastState;
		
		public StateChangedEventArgs (SessionState state, SessionState lastState) 
		{
			this.state = state;
			this.lastState = lastState;
		}

		public SessionState State
		{
			get { return this.state; }
		}

		public SessionState LastState
		{
			get { return this.lastState; }
		}

	}

	public class ErrorEventArgs : System.EventArgs
	{
		private string message;
		private Exception ex;

		public ErrorEventArgs(string message, Exception ex)
		{
			this.message = message;
			this.ex = ex;
		}

		public string Message
		{
			get { return this.message; }
		}

		public Exception Exception
		{
			get { return this.ex; }
		}
	}
}
