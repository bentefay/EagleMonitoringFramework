using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace ProductMonitor.Generic
{
	/// <summary>
	/// The SendSMS class can be used to Send an SMS message to a collection of
	/// phone numbers (supplied as an array list). Currently this class is
	/// specialised to only work with the SMSBullet provider.
	/// </summary>
	public class SendSms
	{
		#region Constructors and Destructors

		/// <summary>
		/// Constructor for the SendSMS class.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="message">The message to send, cannot be greater than 120 characters.</param>
		/// <param name="numbers"></param>
		public SendSms(string username, string password, string message, string numbers)
		{
			this.sUsername = username;
			this.sPassword = password;
			this.sMessage = message;
			this.mobNumber = numbers;
		}

		#endregion

		#region Constants

		/// <summary>
		/// The address of the SMSBullet service.
		/// </summary>
		public const string SMSBULLET_ADDRESS = "http://www.smsbullet.com/msg.php";

		#endregion

		#region Fields and Properties

		/// <summary>
		/// The response from the webserver.
		/// </summary>
		private string _sResponse;

		private string _sUsername;

		/// <summary>
		/// The username for this SendSMS attempt.
		/// </summary>
		public string sUsername
		{
			get
			{
				return _sUsername;
			}
			set
			{
				_sUsername = value;
			}
		}


		private string _sPassword;

		/// <summary>
		/// The password for this SendSMS attempt.
		/// </summary>
		public string sPassword
		{
			get
			{
				return _sPassword;
			}
			set
			{
				_sPassword = value;
			}
		}


		private string _sMessage;

		/// <summary>
		/// The message for this SendSMS attempt.
		/// </summary>
		public string sMessage
		{
			get
			{
				return _sMessage;
			}
			set
			{
				if (value.Length > 120)
				{
					_sMessage = value.Substring(0, 120);
				}
				else
				{
					_sMessage = value;
				}
			}
		}


		private string mobNumber;

		/// <summary>
		/// The number that this SMS will be sent to.
		/// </summary>
		public string MobNumber
		{
			get
			{
				return mobNumber;
			}
			set
			{
				mobNumber = value;
			}
		}


		#endregion

		#region Methods and Functions



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Send()
		{
			StringBuilder sb  = new StringBuilder();

			// Used on each read operation.
			byte[] oBuf = new byte[8192];

			// Prepare the web request.
			HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?u={1}&p={2}&d={3}&m={4}", SMSBULLET_ADDRESS, HttpUtility.UrlEncode(this.sUsername), HttpUtility.UrlEncode(this._sPassword), HttpUtility.UrlEncode(this.mobNumber), HttpUtility.UrlEncode(this.sMessage)));

			// Execute the request
            HttpWebResponse oResponse = (HttpWebResponse)oRequest.GetResponse();

			// We will read data via the response stream.
			Stream oResStream = oResponse.GetResponseStream();

			string sTempString = null;
			int iCount = 0;

			do
			{
				// Fill the buffer with data.
				iCount = oResStream.Read(oBuf, 0, oBuf.Length);

				// Make sure we read some data
				if (iCount != 0)
				{
					// Translate from bytes to ASCII text.
					sTempString = Encoding.ASCII.GetString(oBuf, 0, iCount);

					// Continue building the string.
					sb.Append(sTempString);
				}
			}
			while (iCount > 0);

			this._sResponse = sb.ToString();

			if (this._sResponse.StartsWith("ACK"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}


		/// <summary>
		/// Returns the error response of the request.
		/// </summary>
		/// <returns>A string representing the error response.</returns>
		public string GetErrorResponse()
		{
			return this._sResponse;
		}


		#endregion
	}
}
