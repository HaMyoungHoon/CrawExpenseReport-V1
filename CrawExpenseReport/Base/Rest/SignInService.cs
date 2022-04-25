using CrawExpenseReport.Base.Rest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawExpenseReport.Base.Rest
{
    public class SignInService
    {
        private string _url = @"api/v1/";
        public SignInService()
        {

        }

        public bool SignIn(string id, string pw, out string err, out RestResult ret)
        {
            string signInUrl = _url.Replace("api", FBaseFunc.Ins.Cfg.API_URL) + "signIn";
            RestApiService service = new RestApiService(signInUrl, RestApiService.Method_Type.POST, 10 * 1000);
            service.AppendParameter("id", id);
            service.AppendParameter("pw", pw);
            return service.Send(out err, out ret);
        }

        public bool GetUser(out string err, out RestResult ret)
        {
            string userUrl = _url.Replace("api", FBaseFunc.Ins.Cfg.API_URL) + "user";
            RestApiService service = new RestApiService(userUrl, RestApiService.Method_Type.GET, 10 * 1000);
            service.AppendHeaders("auth_token", FBaseFunc.Ins.Token);
            return service.Send(out err, out ret);
        }
    }
}
