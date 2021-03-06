using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace ApplicationSecurity
{
    public class SecurityModule<T> : IHttpModule
    {
        private readonly PermissionConfig<T> _config;

        public SecurityModule(PermissionConfig<T> config)
        {
            _config = config;
        }

        public void Dispose()
        {}

        public void Init(HttpApplication context)
        {
            context.AuthorizeRequest += ContextAuthorizeRequest;
        }

        private void ContextAuthorizeRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app == null)
                return;

            var user = app.User;
            if (user == null)
                return;

            if (!user.Identity.IsAuthenticated)
                return;

            var authCookie = app.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
                return;

            var ticket = FormsAuthentication.Decrypt(authCookie.Value);
            var permissionsRaw = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(ticket.UserData);
            
            var permissions = permissionsRaw.Select(p => _config.Permissions[p].ToString());
            
            var identity = new GenericIdentity(ticket.Name);
            var principal = new GenericPrincipal(identity, permissions.ToArray());

            app.Context.User = principal;
        }
    }
}