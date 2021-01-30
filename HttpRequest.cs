using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hWeb
{
    public sealed class HttpRequest : IDisposable
    {
        #region Params
        public string Address { get; set; }
        private readonly RequestMethod RequestMethod;

        private RequestHeader RequestHeader;

        private Dictionary<string, string> ParamsValue = new Dictionary<string, string>();
        #endregion

        #region Initialization
        public HttpRequest(string Address, RequestMethod RequestMethod = RequestMethod.GET, RequestHeader RequestHeader = null)
        {
            this.Address = Address;

            this.RequestMethod = RequestMethod;
            this.RequestHeader = RequestHeader;
        }
        #endregion

        #region Indexer
        public object this[string Param]
        {
            set => ParamsValue.Add(Param, value.ToString());
        }
        #endregion

        #region Operators
        public static implicit operator string(HttpRequest Value) => Value.GetResponse();
        #endregion

        #region Params
        public string ContentType
        {
            private get;
            set;
        }

        public string Accept
        {
            private get;
            set;
        }

        public string UserAgent
        {
            private get;
            set;
        }
        #endregion

        #region Methods
        private HttpWebRequest GetWebRequest()
        {
            HttpWebRequest HttpWebRequest;

            HttpWebRequest = RequestMethod == RequestMethod.GET ? (HttpWebRequest)WebRequest.Create(Address + Encoding.UTF8.GetString(GetParam())) : (HttpWebRequest)WebRequest.Create(Address);
            HttpWebRequest.Method = RequestMethod == RequestMethod.GET ? "GET" : "POST";

            if (RequestHeader != null)
            {
                foreach (var Headers in RequestHeader?.RequestValue)
                {
                    HttpWebRequest.Headers.Add(Headers.Key, Headers.Value);
                }
            }

            HttpWebRequest.ContentType = ContentType ?? "application/x-www-form-urlencoded";
            HttpWebRequest.Accept = Accept ?? "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            HttpWebRequest.UserAgent = UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 YaBrowser/19.3.1.887 Yowser/2.5 Safari/537.36";

            return HttpWebRequest;
        }

        private HttpWebResponse Response()
        {
            HttpWebRequest HttpWebRequest = GetWebRequest();

            if (RequestMethod == RequestMethod.POST)
            {
                byte[] Buffer = GetParam();

                HttpWebRequest.ContentLength = Buffer.Length;

                using (Stream StreamRequest = HttpWebRequest.GetRequestStream())
                {
                    StreamRequest.Write(Buffer, 0, Buffer.Length);
                }
            }

            return (HttpWebResponse)HttpWebRequest.GetResponse();
        }

        public async Task<WebResponse> ResponseAsync()
        {
            HttpWebRequest HttpWebRequest = GetWebRequest();

            if (RequestMethod == RequestMethod.POST)
            {
                byte[] Buffer = GetParam();

                HttpWebRequest.ContentLength = Buffer.Length;

                using (Stream StreamRequest = await HttpWebRequest.GetRequestStreamAsync())
                {
                    StreamRequest.Write(Buffer, 0, Buffer.Length);
                }
            }

            return await HttpWebRequest.GetResponseAsync();
        }

        private string GetResponse()
        {
            StringBuilder ReadResponse = new StringBuilder(15);

            using (StreamReader StreamReader = new StreamReader(Response().GetResponseStream()))
            {
                ReadResponse.Append(StreamReader.ReadToEnd());
            }

            return ReadResponse.ToString();
        }

        private async Task<string> GetResponseAsync()
        {
            StringBuilder ReadResponse = new StringBuilder(15);

            WebResponse HttpWebResponse = await ResponseAsync();

            using (StreamReader StreamReader = new StreamReader(HttpWebResponse.GetResponseStream()))
            {
                ReadResponse.Append(await StreamReader.ReadToEndAsync());
            }

            return ReadResponse.ToString();
        }
        #endregion

        #region Helper Methods
        private byte[] GetParam()
        {
            try
            {
                StringBuilder Params = new StringBuilder(15);

                foreach (var Param in ParamsValue)
                {
                    Params.Append(Param.Key + "=" + Param.Value + "&");
                }

                return RequestMethod == RequestMethod.GET ? Encoding.UTF8.GetBytes(Params.Insert(0, "?").ToString().TrimEnd('&')) : Encoding.UTF8.GetBytes(Params.ToString().TrimEnd('&'));
            }
            catch
            {
                return new byte[1];
            }
        }

        public void ClearParams() => ParamsValue.Clear();
        #endregion

        #region Overide
        public override string ToString() => GetResponse();
        #endregion

        public void Dispose()
        {
            ParamsValue.Clear();
        }
    }
}