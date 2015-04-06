using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace Product_Monitor.Generic
{
    static class GetURLStream
    {

        //taken from http://www.primaryobjects.com/CMS/Article64.aspx


        public static Stream getURLStream(string strURL, string userName, string password)
        {
            System.Net.WebRequest objRequest;
            System.Net.WebResponse objResponse = null;
            Stream objStreamReceive;

            try
            {
                objRequest = System.Net.WebRequest.Create(strURL);
                objRequest.Timeout = 5000;
                objRequest.Credentials = new NetworkCredential(userName, password);

                objResponse = objRequest.GetResponse();
                objStreamReceive = objResponse.GetResponseStream();
                return objStreamReceive;
            }
            catch (Exception excep)
            {
                Generic.Logger.getInstance().Log(excep);
                return null;
            }
        }
    }
}
