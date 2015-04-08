using System;
using System.IO;
using System.Net;
using Serilog;

namespace ProductMonitor.Framework.Generic
{
    public static class GetURLStream
    {

        //taken from http://www.primaryobjects.com/CMS/Article64.aspx
        public static Stream GetUrlStream(string strURL, string userName, string password)
        {
            try
            {
                var objRequest = WebRequest.Create(strURL);
                objRequest.Timeout = 5000;
                objRequest.Credentials = new NetworkCredential(userName, password);

                var objResponse = objRequest.GetResponse();
                var objStreamReceive = objResponse.GetResponseStream();
                return objStreamReceive;
            }
            catch (Exception e)
            {
                Log.Error(e, "GetUrlStream failed");
                return null;
            }
        }
    }
}
