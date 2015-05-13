using System;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

using YJ.AppLink.Api;

namespace YJ.AppLink.Net
{
	internal delegate void MessageReceivedHandler ( object sender, MessageReceivedEventArgs args);
	
	/// <summary>
	/// For internal SDK use:
    /// Listens for messages on the socket connect,
    /// deserializes XML to API objects, 
    /// and raises an event when messages are received.
	/// </summary>
    public class MessageListener
	{
		internal event MessageReceivedHandler MessageReceived;
		
		private const string EnvelopeStart = "<Envelope";
		private const string EnvelopeEnd = "</Envelope>";

		// do not set less than 200
        private const int AsyncSleep = 2000;
		
		private int checkReceivedSleep = 100;
		
		private Socket mSocket;

		private XmlSerializer xs;
		private System.Text.Decoder decoder;

        private byte[] mDataBuffer = new byte[131072];
		private IAsyncResult mAsynResult;
		private AsyncCallback mCallback ;
		private string receivedString = "";

		private Thread checkStringThread;
		private bool listening = false;

		private Session session;

		internal MessageListener(Socket s, Session session)
		{
			mSocket = s;
			this.session = session;

			checkReceivedSleep = (int) MessageListener.AsyncSleep / 2;
			decoder = System.Text.Encoding.Unicode.GetDecoder();
			xs = new XmlSerializer(typeof(Envelope), Types.GetTypes());
		}

		#region internal methods

		internal void Start()
		{
			if ( mCallback == null )
				mCallback = new AsyncCallback (DataReceived);

			listening = true;
			Listen();

			checkStringThread = new Thread(new ThreadStart(CheckReceivedString));
			checkStringThread.IsBackground = true;
			checkStringThread.Start();
		}

		internal void Stop()
		{
			listening = false;
		}

		#endregion

		#region private methods
		
		private void Listen()
		{
			try
			{
				mAsynResult =  mSocket.BeginReceive (mDataBuffer, 0, 
					mDataBuffer.Length, SocketFlags.None, mCallback, null);
			}
			catch (Exception e)
			{
				// not thrown b/c can be called from async callback/thread
				
				session.OnSessionError("Failure to begin listening", this, e);
				if (!mSocket.Connected)
					session.ConnectionFailure(e);
			}
		}

		private delegate void MessageHandler(Envelope envelope);

        //receive message event listener
		private void OnMessageReceived (Envelope envelope)
		{
			try
			{
				if (session.ThreadSafeGUIControl != null &&
					session.ThreadSafeGUIControl.InvokeRequired)
				{
					session.ThreadSafeGUIControl.BeginInvoke(new MessageHandler(OnMessageReceived), 
						new object[] {envelope});
					return;

				}
		
				if (MessageReceived != null)
				{				
					MessageReceived(this, new MessageReceivedEventArgs(envelope));
				}
			}
			catch (Exception e)
			{
				session.OnSessionError("Error raising MessageReceived event", this, e);
			}
		}


		private void DataReceived(IAsyncResult asyn)
		{
			try
			{
				if (!listening)
					return;
				
				int i = mSocket.EndReceive (asyn);
				char[] chars = new char[i + 1];
				int charLen = decoder.GetChars(mDataBuffer, 0, i, chars, 0);
				System.String data = new System.String(chars);
				
				// note: this logging call occurs off the main thread
				session.Logger.Debug("Received: " + data, this);
				
				lock (receivedString)
				{
					receivedString += data;
				}

				// note: if this doesn't sleep long enough, data in the buffer can be missed
				System.Threading.Thread.Sleep(MessageListener.AsyncSleep);
				Listen();
			}
			catch (Exception e)
			{
				// not thrown b/c can be called from async callback/thread
				
				session.OnSessionError("Failure handling received data", this, e);
				if (!mSocket.Connected)
					session.ConnectionFailure(e);
			}
		}

		private void CheckReceivedString()
		{
			while (listening)
			{
				try
				{
					Thread.Sleep(checkReceivedSleep);

					if (receivedString.Length == 0)
						continue;
				
					string tempString = "";
                    string xml = "";
                    lock (receivedString)
					{
						tempString = receivedString.Replace("\0", ""); // removes some encoding

                        int begin = tempString.IndexOf(MessageListener.EnvelopeStart);
                        int end = tempString.IndexOf(MessageListener.EnvelopeEnd);

                        if (begin > -1 &&
                            end > -1)
                        {
                            xml = tempString.Substring(begin, end + MessageListener.EnvelopeEnd.Length - begin);

                            receivedString = tempString.Substring(end + MessageListener.EnvelopeEnd.Length);
                        }
                        else
                        {
                            continue;
                        }
					}

					StringReader streader = new StringReader(xml);
					Envelope e = (Envelope) xs.Deserialize (streader); // add specific exception handling
                    //OnMessageReceivedPricer(e);
					OnMessageReceived(e);

				}
				catch (Exception e)
				{
					session.OnSessionError("Failure processing received string: " + receivedString, this, e);
					// not thrown b/c can be called from async callback/thread
					// also do not want to exit loop

					// clear the received string to remove bad data 
					// note: this could also remove good data
					
					// couid wipe out every everything before envelope end
					lock (receivedString)
					{
						receivedString = "";
					}
				}
			}
		}

		#endregion
	}

	public class MessageReceivedEventArgs : System.EventArgs
	{
		private Envelope mEnvelope;
		
		internal MessageReceivedEventArgs(Envelope envelope)
		{
			mEnvelope = envelope;
		}

		internal Envelope Envelope
		{
			get { return mEnvelope; }
		}

	}

}
