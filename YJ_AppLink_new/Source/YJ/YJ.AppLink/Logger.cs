using System;
using System.Collections;

namespace YJ.AppLink
{
	public delegate void MessageLoggedHandler (object sender, MessageLoggedEventArgs args);

	/// <summary>
	/// A class that provides logging functionality for the AppLink
	/// </summary>
    public class Logger
	{		
		/// <summary>
		/// Occurs when a message is logged
		/// </summary>
        public event MessageLoggedHandler MessageLogged;
		
		private Session session;
		private LogLevel level = LogLevel.Warn;
		private ArrayList log = new ArrayList();
		private int maxLogMessageCount = 1000;

		internal Logger(Session session) 
		{
			this.session = session;
		}

		#region public properties

		/// <summary>
		/// Gets or sets the level of logging.  The default level is Warn
		/// </summary>
        public  LogLevel LogLevel
		{
			get { return this.level; }
			set { this.level = value; }
		}

        /// <summary>
        /// Gets or sets the maximum number of log messages stored in memory.  The default value is 1000.
        /// </summary>
		public  int MaxLogMessageCount
		{
			get { return this.maxLogMessageCount; }
			set { this.maxLogMessageCount = value; }
		}

		#endregion

		#region public methods

		/// <summary>
        /// Logs a debug level message, if the LogLevel is Debug
		/// </summary>
        public void Debug(string message, object sender)
		{
			Debug(message, sender, null);
		}

        /// <summary>
        /// Logs a debug level message and exception, if the LogLevel is Debug
        /// </summary>
		public void Debug(string message, object sender, Exception e)
		{
			if (level < LogLevel.Debug)
				return;

			OnMessageLogged(LogLevel.Debug, message, sender, e);
		}

        /// <summary>
        /// Logs an info level message, if the LogLevel is Info or less
        /// </summary>
		public void Info(string message, object sender)
		{
			Info(message, sender, null);
		}

        /// <summary>
        /// Logs an info level message and exception, if the LogLevel is Info or less
        /// </summary>
		public void Info(string message, object sender, Exception e)
		{
			if (level < LogLevel.Info)
				return;

			OnMessageLogged(LogLevel.Info, message, sender, e);
		}

        /// <summary>
        /// Logs an Warn level message, if the LogLevel is Warn or less
        /// </summary>
		public void Warn(string message, object sender)
		{
			Warn(message, sender, null);
		}

        /// <summary>
        /// Logs an Warn level message and exception, if the LogLevel is Warn or less
        /// </summary>
		public void Warn(string message, object sender, Exception e)
		{
			if (level < LogLevel.Warn)
				return;

			OnMessageLogged(LogLevel.Warn, message, sender, e);

		}

        /// <summary>
        /// Logs an Error level message, if the LogLevel is Error or less
        /// </summary>
		public void Error(string message, object sender)
		{
			Error(message, sender, null);
		}

        /// <summary>
        /// Logs an Error level message and exception, if the LogLevel is Error or less
        /// </summary>
		public void Error(string message, object sender, Exception e)
		{
			if (level < LogLevel.Error)
				return;

			OnMessageLogged(LogLevel.Error, message, sender, e);

		}

        /// <summary>
        /// Logs an Fatal level message, if the LogLevel is Fatal or less
        /// </summary>
		public void Fatal(string message, object sender)
		{
			Fatal(message, sender, null);
		}

        /// <summary>
        /// Logs an Fatal level message and exception, if the LogLevel is Fatal or less
        /// </summary>
		public void Fatal(string message, object sender, Exception e)
		{
			if (level < LogLevel.Fatal)
				return;

			OnMessageLogged(LogLevel.Fatal, message, sender, e);

		}

        /// <summary>
        /// Gets an array dump of all stored log messages
        /// </summary>
		public string[] GetLogDump()
		{
			lock (log)
			{
				string[] ret = new string[log.Count];
				log.CopyTo(ret);
				return ret;
			}	
		}

        /// <summary>
        /// Gets an string dump of all stored log messages
        /// </summary>
		public string GetLogDumpToString()
		{
			string[] logs = GetLogDump();
			if (logs.Length > 0)
			{
				return String.Join("\n", logs);
			}

			return "";
		}

		#endregion

		#region private Methods
		
		private void StoreLogMessage(string message)
		{
			lock (log)
			{
				if (log.Count >= maxLogMessageCount)
				{
					log.RemoveRange(0, (maxLogMessageCount - log.Count) + 1);
				}
				
				log.Add(message);
			}
		}

		private delegate void OnMessageLoggedCallback(LogLevel level, string message, object sender, Exception e);
		private void OnMessageLogged(LogLevel level, string message, object sender, Exception e)
		{
			// note: if sessionThreadSafeGUIControl is null
			// this is not guarnateed to be thread safe
			
			if (session.ThreadSafeGUIControl != null &&
				session.ThreadSafeGUIControl.InvokeRequired)
			{
				session.ThreadSafeGUIControl.BeginInvoke(new OnMessageLoggedCallback(OnMessageLogged), 
					new object[] {level, message, sender, e});
				return;

			}
			
			MessageLoggedEventArgs args = new MessageLoggedEventArgs(level, message);
			if (sender != null)
				args.SenderType = sender.GetType().ToString();
			
			args.Exception = e;
			
			if (MessageLogged != null)
			{
				MessageLogged(this, args);
			}

			StoreLogMessage(args.ToString());
		}

		#endregion
	}

    /// <summary>
    /// An enum of logging levels
    /// </summary>
	public enum LogLevel : int
	{	
		Debug = 5,       // All messages are logged
		Info = 4,        // Info and below are logged
		Warn = 3,        // Warn and below are logged
		Error = 2,       // Error and below are logged
		Fatal = 0,       // Only Fatal messages are logged
		None = -1        // No messages are logged
	}

	public class MessageLoggedEventArgs : System.EventArgs
	{
		private Exception ex = null;
		private string message = "";
		private string sender = ""; 
		private DateTime created = DateTime.Now;
		private LogLevel level;

		private string messageLogString = null;

		internal MessageLoggedEventArgs (LogLevel level, string message)
		{
			this.level = level;
			this.message = message;
		}

		public LogLevel LogLevel
		{
			get { return this.level; }
		}

		public Exception Exception
		{
			get { return this.ex; }
			set { this.ex = value; }
		}
		
		public string SenderType
		{
			get { return this.sender; }
			set { this.sender = value; }
		}

		public DateTime Created
		{
			get { return this.created; }
		}

		private void WriteMessageLogString()
		{
			if (messageLogString != null)
				return;
			
			messageLogString = level.ToString() + "\t" + created.Hour + ":" + created.Minute + ":" 
				+ created.Second + ":" + created.Millisecond;

			if (sender != "")
			{
				messageLogString += " (" + sender + ")"; 
			}

			messageLogString += " " + message;

			if (ex != null)
			{
				messageLogString += ", Exception: " + ex.Message;
			}
		}

		public override string ToString()
		{
			WriteMessageLogString();
			return messageLogString;
		}

		
	}


}
