using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace ActiveDirectoryAuth
{
    public class AdAuthorizationServerProvider:OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin",new []{"*"});

          var principalContext = new ActiveDirectoryManager(ConfigurationManager.AppSettings["IpAddress"], ConfigurationManager.AppSettings["DomainName"], ConfigurationManager.AppSettings["ServiceUSerName"], ConfigurationManager.AppSettings["ServicePassword"]).GetPrincipalContext();

            using (principalContext)
            {
                var isValid = principalContext.ValidateCredentials(context.UserName, context.Password);
                if (!isValid)
                {
                    context.SetError("invalid_grant","The username or password is incorrect");
                    return Task.FromResult<object>(null);
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role,"user"));
            context.Validated(identity);
            var props = new AuthenticationProperties(new Dictionary<string, string>
            {
                {
                    "userName", context.UserName
                }
            });
            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);

            return Task.FromResult<object>(null);

        }
        public override async Task TokenEndpoint(OAuthTokenEndpointContext context)
        {

            foreach (var property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            await Task.FromResult<object>(null);
        }
    }
}