using System;
using System.Collections;

using YJ.AppLink.Api;
using YJ.AppLink.Net;
using csclientnet;

namespace YJ.AppLink
{
	/// <summary>
	/// An abstract base class for AppLink services.  Provides methods for starting and stoping the service.
	/// </summary>
	public abstract class Service
	{
        private Session session;
       // private csclientnet.ClientSession sessionCs;
       // private csclientnet.Client listenerCs;
		private MessageSender sender;
		private MessageListener listener;
		private ServiceName serviceName;
        private string version;

		protected bool running = false;

		protected Service(ServiceName serviceName, string version, Session session) 
		{
			this.serviceName = serviceName;
            this.version = version;
			this.session = session;
		}

		private Service () {}

		#region public properties

		/// <summary>
		/// Gets a value indicating whether the service was started and is currently running
		/// </summary>
        public bool Running
		{
			get { return running; }
		}

        /// <summary>
        /// Gets an enum value of the service's name
        /// </summary>
		public ServiceName ServiceName
		{
			get { return this.serviceName; }
		}

        /// <summary>
        /// Gets the version of the API that the service supports
        /// </summary>
        public string Version
        {
            get { return this.version; }
        }

		#endregion

		#region public methods

        /// <summary>
        /// Starts the service by sending a StartServiceMessage to YJ Energy. 
        /// Throws an exception if the service cannot be started, if for example the AppLink is not connected.
        /// </summary>
		public virtual void Start()
		{
			session.Logger.Debug("Service: " + serviceName + " starting", this);
			if (running)
			{
				session.Logger.Warn("Service: " + serviceName + " already running", this);
				return;
			}
			
			try
			{
                session.StartService(this);
			}
			catch (Exception e)
			{
				session.OnSessionError("Failure to start service: " + this.serviceName, this, e);
				throw e;
			}
			
			this.sender = session.MessageSender;
			this.listener = session.MessageListener;
			listener.MessageReceived +=new MessageReceivedHandler(listener_MessageReceived);
			running = true;
			
			session.Logger.Info("Service: " + serviceName + " started", this);
		}

        /// <summary>
        /// Stops the service by sending a StopServiceMessage to YJ Energy. 
        /// Throws an exception if the service cannot be stopped.
        /// </summary>
		public virtual void Stop()
		{
			session.Logger.Debug("Service: " + serviceName + " stopping", this);
			if (!running)
			{
				session.Logger.Warn("Service: " + serviceName + " not running", this);
				return;
			}

			try
			{
				running = false;
				if (listener != null)
					listener.MessageReceived -=new MessageReceivedHandler(listener_MessageReceived);			
					
				session.StopService(this);
				
				session.Logger.Info("Service: " + serviceName + " stopped", this);
			}
			catch (Exception e)
			{
				session.OnSessionError("Failure to stop service: " + this.serviceName, this, e);
				throw e;
			}			
		}

		public override string ToString()
		{
			return serviceName.ToString();
		}

		#endregion

        #region protected properties

        protected Session Session
        {
            get { return this.session; }
        }

        protected MessageSender Sender
        {
            get { return this.sender; }
        }

        #endregion


        #region protected methods

        protected void listener_MessageReceived(object sender, 
			MessageReceivedEventArgs args)
		{
			// note: if the session does not have a threadSafeGUIControl 
			// this is invoked asynchronously
			// and is not thread safe.

			HandleReceivedMessage(sender, args);
		}

		#endregion

		#region abstract methods

		protected abstract void HandleReceivedMessage(object sender, 
			MessageReceivedEventArgs args);

		#endregion

	}

    /// <summary>
    /// An enum of valid currently supported services
    /// </summary>
	public enum ServiceName
	{
		Pricing,
        Messaging
	}
}
