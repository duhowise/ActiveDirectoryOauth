using System;
using System.Web.Http;
using ActiveDirectoryAuth;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
[assembly: OwinStartup(typeof(Startup))]

namespace ActiveDirectoryAuth
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration=new HttpConfiguration();
            ConfigureOAuth(app);
            WebApiConfig.Register(configuration);
            app.UseWebApi(configuration);
        }

        private void ConfigureOAuth(IAppBuilder app)
        {
            var oAuthAuthorizationServerOptions=new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new AdAuthorizationServerProvider(),

            };
            app.UseOAuthAuthorizationServer(oAuthAuthorizationServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }
    }
}
