using System;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.IO;
using System.Xml.Serialization ;
using System.Threading;

using YJ.AppLink;

namespace YJ.AppLink.Net
{
	/// <summary>
    /// For internal SDK use:
	/// Creates and manages a TCP socket connection, 
    /// and MessageListener and MessageSender instances for sending and receiving messages
	/// </summary>
    internal class Connection
	{
		private Socket mSocket;		
		private MessageListener mListener;
		private MessageSender mSender;

		private int port = Session.DefaultPort;
		private string ipAddr = Session.DefaultIPAddress;
		private string domainName = "";

		private Session mSession;
		
		internal Connection(Session session)
		{
			mSession = session;
			mSocket = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);

			mSession.Logger.Debug("Connection instantiated", this);

		}

		#region internal properties

		internal int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		internal string IpAddress
		{
			get { return this.ipAddr; }
			set { this.ipAddr = value; }
		}

		internal string DomainName
		{
			get { return this.domainName; }
			set { this.domainName = value; }
		}

		internal MessageListener MessageListener
		{
			get
			{
				if (mListener == null)
					mListener = new MessageListener(mSocket, mSession);

				return mListener;
			}
		}

		internal MessageSender MessageSender
		{
			get
			{
				if (mSender == null)
					mSender = new MessageSender(mSocket, mSession);

				return mSender;
			}
		}

		#endregion

		#region internal methods

		internal bool Connect()
		{
			
			try
			{
				System.Net.IPAddress ipAdd = GetIPAddress();
				if (ipAdd == null)
					throw new Exception("Invalid IP Address or Domain Name specified");

				mSession.Logger.Info("Connect to " + ipAdd.ToString() + ":" + port.ToString(), this);				
				
				System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint (ipAdd, port);
				mSocket.Connect (remoteEP);
				
				mSession.Logger.Info("Socket connected", this);
			}
			catch (Exception e)
			{
				mSession.OnSessionError("Socket failed to connect", this, e);
				throw e;
			}

			return true;
		}

		internal void Close()
		{
			try
			{
				mSession.Logger.Debug("Connection closing", this);
				
				if (mListener != null)
					mListener.Stop();
			
				mSocket.Close();

				mSession.Logger.Info("Connection closed", this);
			}
			catch (Exception e)
			{
				mSession.OnSessionError("Error closing connection", this, e);
				throw e;
			}
		}

		#endregion

		#region private methods
		
		private System.Net.IPAddress GetIPAddress()
		{
			System.Net.IPAddress ipAdd = null;
			try
			{
				if (domainName != null &&
					domainName != "")
				{
					IPHostEntry host = Dns.Resolve(domainName);
					if (host != null &&
						host.AddressList != null &&
						host.AddressList.Length > 0)
					{
						ipAdd = host.AddressList[0];
						mSession.Logger.Info("Connecting to domain: " + domainName, this);
					}
					else
					{
						throw new Exception("DomainName: " + domainName + " host address not found");
					}
				}
				else
				{
					ipAdd = System.Net.IPAddress.Parse(ipAddr);
					mSession.Logger.Info("Connecting to IP: " + ipAddr, this);
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			return ipAdd;
		}

		#endregion

	}
}
