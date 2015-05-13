using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using XCSUtils.Smtp;

namespace MarketQADataProcessor
{
	
	internal class Emailer
	{
	
		private string _fromEmail;
		private string _fromName;
		private string _toEmail;
		private string _toName;
		private string _subject;
		private string _emailBody;
		private bool _isBodyHtml;
		private List<Attachment> _attachments;

		private string _host;
		private int _port;

		private bool _isAuthenticationRequired;

		private string _LoginName;
		private string _LoginPassword;

		public Emailer(string emailServer, int port, string fromEmail, string fromName, string toEmail, string toName, string subject, string emailBody, bool isBodyHtml)
		{
			_attachments = new List<Attachment>();
			
			_fromEmail = fromEmail;
			_fromName = fromName;
			_toEmail = toEmail;
			_toName = toName;
			_subject = subject;
			_emailBody = emailBody;
			_isBodyHtml = isBodyHtml;
			_host = emailServer;
			_port = port;
		}

		internal void SetAuthenticationParameters(bool isAuthenticationRequired, string loginName, string loginPassword)
		{
			_isAuthenticationRequired = isAuthenticationRequired;
			_LoginName = loginName;
			_LoginPassword = loginPassword;
		}

		internal void AddAttachment(string fileName)
		{
			if (File.Exists(fileName))
			{
				_attachments.Add(new Attachment(fileName));	
			}
		}

		internal void SendEmail()
		{
			using (MailMessage _MailMessage = new MailMessage())
			{
				_MailMessage.From = new MailAddress(_fromEmail, _fromName);
				_MailMessage.To.Add(_toEmail);

				_MailMessage.Subject = _subject;

				_MailMessage.Body = _emailBody;
				_MailMessage.IsBodyHtml = _isBodyHtml;

				if (_attachments.Count > 0)
				{
					foreach (var attachment in _attachments)
					{
						_MailMessage.Attachments.Add(attachment);
					}
				}

				using (SmtpClient _SmtpClient = new SmtpClient(_host, _port))
				{
					_SmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

					if (_isAuthenticationRequired)
					{
						System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential(_LoginName, _LoginPassword);

						_SmtpClient.UseDefaultCredentials = false;
						_SmtpClient.Credentials = networkCredential;
					}


					_SmtpClient.Send(_MailMessage);
				}
			}
		}
	}
}
