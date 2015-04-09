using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;

namespace ProductMonitor.Framework.Generic
{
	public class SendSms
	{
		public SendSms(string username, string password, string message, string mobileNumber)
		{
			Username = username;
			Password = password;
			Message = message;
			MobileNumber = mobileNumber;
		}

		public const string SmsBulletAddress = "http://www.smsbullet.com/msg.php";
		private string _response;

        public string Username { get; set; }
	    public string Password { get; set; }
        public string MobileNumber { get; set; }

	    private string _message;
		public string Message
		{
			get { return _message; }
			set
			{
				if (value.Length > 120)
				{
					_message = value.Substring(0, 120);
				}
				else
				{
					_message = value;
				}
			}
		}
        
	    public bool Send()
		{
			var stringBuilder  = new StringBuilder();
			var buffer = new byte[8192];
	        var username = HttpUtility.UrlEncode(Username);
	        var password = HttpUtility.UrlEncode(Password);
	        var mobileNumber = HttpUtility.UrlEncode(MobileNumber);
	        var message = HttpUtility.UrlEncode(Message);
	        var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?u={1}&p={2}&d={3}&m={4}", SmsBulletAddress, username, password, mobileNumber, message));
            var response = (HttpWebResponse)request.GetResponse();
			var responseStream = response.GetResponseStream();
		    var count = 0;

			do
			{
			    Debug.Assert(responseStream != null, "responseStream != null");
			    count = responseStream.Read(buffer, 0, buffer.Length);

				if (count != 0)
				{
				    var tempString = Encoding.ASCII.GetString(buffer, 0, count);
					stringBuilder.Append(tempString);
				}
			}
			while (count > 0);

			_response = stringBuilder.ToString();
			return _response.StartsWith("ACK");
		}

		public string GetErrorResponse()
		{
			return _response;
		}
	}
}
