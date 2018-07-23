using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveDirectoryAuth.Models;
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

            using (var dbcontext = new AdContext())
            {
                var config = dbcontext.DirectorySetups.FirstOrDefault();
                var domain = config?.DomainName.Split('.');

                if (domain != null)
                {
                    var principalContext = new ActiveDirectoryManager(config.IpAddress, $"DC={domain[0]},DC={domain[1]}", config.ServiceUserName, config.ServicePassword).GetPrincipalContext();
                    using (principalContext)
                    {
                        var isValid = principalContext.ValidateCredentials(context.UserName, context.Password);
                        if (!isValid)
                        {
                            context.SetError("invalid_grant", "The username or password is incorrect");
                            return Task.FromResult<object>(null);
                        }
                    }

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