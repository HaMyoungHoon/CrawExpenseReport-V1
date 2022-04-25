using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CrawExpenseReport.Base.Rest.Common
{
    public class RestApiService : RestParameter
    {
        public enum Method_Type
        {
            NULL = -1,
            GET = 0,
            POST = 1,
            PUT = 2,
            DELETE = 3,
        }

        int _timeout;
        string _url;
        Method_Type _method;

        public RestApiService()
        {
            _url = "";
            _method = Method_Type.NULL;
            _timeout = 10 * 1000;
        }
        public RestApiService(string url, Method_Type method, int timeout)
        {
            _url = url;
            _method = method;
            _timeout = timeout;
        }
        public void SetUrl(string url)
        {
            _url = url;
        }
        public void SetMethod(Method_Type method)
        {
            _method = method;
        }
        public void SetTimeout(int timeout)
        {
            _timeout = timeout;
        }

        public bool Send(out string err, out RestResult ret)
        {
            bool desRet = false;
            ret = new RestResult();
            if (_url.Length <= 0)
            {
                err = "Url is Empty";
                return desRet;
            }
            else if (!Enum.IsDefined<Method_Type>(_method) || _method == Method_Type.NULL)
            {
                err = "Request Method is NULL";
            }
            else
            {
                err = "";
            }

            try
            {
                StringBuilder stb = new StringBuilder();
                if (GetPaths().Length > 0)
                {
                    stb.AppendFormat("{0}{1}", _url, GetPaths());
                }
                else
                {
                    stb.AppendFormat("{0}", _url);
                }
                if (GetParams().Length > 0)
                {
                    stb.AppendFormat("?{0}", GetParams());
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(stb.ToString());
                WebHeaderCollection webHeader = req.Headers;
                foreach (var header in GetHeaders())
                {
                    webHeader.Add(header);
                }

                switch (_method)
                {
                    case Method_Type.NULL: return false;
                    case Method_Type.GET:
                        {
                            req.Method = "GET";
                        }
                        break;
                    case Method_Type.POST:
                        {
                            req.Method = "POST";
                        }
                        break;
                    case Method_Type.PUT:
                        {
                            req.Method = "PUT";
                        }
                        break;
                    case Method_Type.DELETE:
                        {
                            req.Method = "POST";
                        }
                        break;
                    default:
                        {
                            return false;
                        }
                }

                req.ContentType = "application/json";
                req.Timeout = _timeout;

                using (WebResponse res = req.GetResponse())
                {
                    Stream resStream = res.GetResponseStream();
                    desRet = RestResult.Deserialize(resStream, out ret, out err);
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return desRet;
            }

            return desRet;
        }

        public bool Send(string fileName, out string err, out FileStream ret)
        {
            bool desRet = false;
            ret = null;
            if (_url.Length <= 0)
            {
                err = "Url is Empty";
                return desRet;
            }
            else if (!Enum.IsDefined<Method_Type>(_method) || _method == Method_Type.NULL)
            {
                err = "Request Method is NULL";
            }
            else
            {
                err = "";
            }

            try
            {
                StringBuilder stb = new StringBuilder();
                if (GetPaths().Length > 0)
                {
                    stb.AppendFormat("{0}{1}", _url, GetPaths());
                }
                else
                {
                    stb.AppendFormat("{0}", _url);
                }
                if (GetParams().Length > 0)
                {
                    stb.AppendFormat("?{0}", GetParams());
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(stb.ToString());
                WebHeaderCollection webHeader = req.Headers;
                foreach (var header in GetHeaders())
                {
                    webHeader.Add(header);
                }

                switch (_method)
                {
                    case Method_Type.NULL: return false;
                    case Method_Type.GET:
                        {
                            req.Method = "GET";
                        }
                        break;
                    case Method_Type.POST:
                        {
                            req.Method = "POST";
                        }
                        break;
                    case Method_Type.PUT:
                        {
                            req.Method = "PUT";
                        }
                        break;
                    case Method_Type.DELETE:
                        {
                            req.Method = "POST";
                        }
                        break;
                    default:
                        {
                            return false;
                        }
                }

                req.ContentType = "application/json";
                req.Timeout = _timeout;

                using (WebResponse res = req.GetResponse())
                {
                    Stream resStream = res.GetResponseStream();
                    desRet = RestResult.Deserialize(fileName, resStream, out ret, out err);
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return desRet;
            }

            return desRet;
        }

        public bool Send(string fileName, out string err, out RestResult ret)
        {
            bool desRet = false;
            ret = new RestResult();
            if (_url.Length <= 0)
            {
                err = "Url is Empty";
                return desRet;
            }
            else if (!Enum.IsDefined<Method_Type>(_method) || _method == Method_Type.NULL)
            {
                err = "Request Method is NULL";
            }
            else
            {
                err = "";
            }

            try
            {
                StringBuilder stb = new StringBuilder();
                if (GetPaths().Length > 0)
                {
                    stb.AppendFormat("{0}{1}", _url, GetPaths());
                }
                else
                {
                    stb.AppendFormat("{0}", _url);
                }
                if (GetParams().Length > 0)
                {
                    stb.AppendFormat("?{0}", GetParams());
                }

                using (HttpClient httpClient = new HttpClient())
                {
                    using (MultipartFormDataContent form = new MultipartFormDataContent())
                    {
                        using (FileStream file = new FileStream(fileName, FileMode.Open))
                        {
                            using (StreamContent streamContent = new StreamContent(file))
                            {
                                using (ByteArrayContent fileContent = new ByteArrayContent(Task.Run(async () => await streamContent.ReadAsByteArrayAsync()).Result))
                                {
                                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                                    form.Add(fileContent, "file", Path.GetFileName(fileName));
                                    HttpResponseMessage res = Task.Run(async () => await httpClient.PostAsync(stb.ToString(), form)).Result;
                                    desRet = RestResult.Deserialize(res.Content.ReadAsStream(), out ret, out err);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return desRet;
            }

            return desRet;
        }
    }
}
