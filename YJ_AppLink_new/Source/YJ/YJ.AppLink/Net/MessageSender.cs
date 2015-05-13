using System;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

using YJ.AppLink.Api;
using YJ.AppLink;

namespace YJ.AppLink.Net
{
	/// <summary>
	/// For internal SDK use:
    /// Serializes API objects to XML to be sent over the TCP socket connection.
	/// </summary>
    public class MessageSender
	{
		private Socket mSocket;
		private Envelope sendingEnvelope;
		private XmlSerializer xs;
		private Session session;
		private bool running = true;
		
		internal MessageSender(Socket s, Session session)
		{
			mSocket = s;
			this.session = session;
			sendingEnvelope = new Envelope();
			xs = new XmlSerializer(typeof(Envelope), Types.GetTypes());
		}

		#region internal methods

		internal void Start()
		{
			running = true;
		}

		internal void Stop()
		{
			running = false;
		}

		/// <summary>
		/// Serializes and sends and XML message.
        /// The content object param must be of an API type, such as StartServiceRequest or PriceResponse.
		/// </summary>
        internal void Send(object content)
		{
			try
			{
				if (!running)
				{
					session.Logger.Warn("MessageSender not running", this);
                    return;
				}
				
				sendingEnvelope.Content = content;
				StringWriter writer = new StringWriter();
				xs.Serialize(writer, sendingEnvelope);		
				string xml = writer.ToString();
                

				byte[] byteData = System.Text.Encoding.Unicode.GetBytes(xml);		
				mSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
					new AsyncCallback(SendCallback), null);

			}
			catch (Exception e)
			{
				session.OnSessionError("Failure sending message", this, e);
				throw e;
			}
		}

		#endregion

		#region private methods

		private void SendCallback(IAsyncResult asyn)
		{
			try
			{
				if (!running)
					return;
				
				mSocket.EndSend(asyn);
			}
			catch (Exception e)
			{
				session.OnSessionError("Failure in send callback", this, e);
			}
		}

		#endregion
	}
}
