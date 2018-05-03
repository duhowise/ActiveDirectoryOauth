using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ActiveDirectoryAuth.Models;

namespace ActiveDirectoryAuth.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly ActiveDirectoryManager _userManager = null;

        public AccountController()
        {
           

            using (var context = new AdContext())
            {
                var config = context.DirectorySetups.AsNoTracking().FirstOrDefault();
                var domain = config?.DomainName.Split('.');

                if (domain!=null)
                {
            _userManager=new ActiveDirectoryManager(config.IpAddress, $"DC={domain[0]},DC={domain[1]}", config.ServiceUserName, config.ServicePassword);

                }

            }
        }


        [Authorize]
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAllUsers()
        {
            var result = new List<UserPrincipal>();
            var searcher = new PrincipalSearcher { QueryFilter = new UserPrincipal(_userManager.GetPrincipalContext()) };
            var data = await Task.FromResult(searcher.FindAll());
            foreach (var principal1 in data)
            {
                var principal = (UserPrincipal) principal1;
                result.Add(principal);
            }
            return Ok(new { state = true, data = result.Select(s => new UserEntry { EmailAddress = s.EmailAddress, EmployeeId = s.EmployeeId, FirstName = s.GivenName, LastName = s.Surname, MiddleName = s.MiddleName, Telephone = s.VoiceTelephoneNumber, UserName = s.SamAccountName }) });

        }


        [Authorize]
        [Route("{term}")]
        [HttpGet]
        public async Task<IHttpActionResult> Search(string term)
        {
            var result = new List<UserPrincipal>();
            var searchPrinciples = new List<UserPrincipal>
            {
                new UserPrincipal(_userManager.GetPrincipalContext()) {DisplayName = $"*{term}*"},
                new UserPrincipal(_userManager.GetPrincipalContext()) {SamAccountName = $"*{term}*"},
                new UserPrincipal(_userManager.GetPrincipalContext()) {MiddleName = $"*{term}*"},
                new UserPrincipal(_userManager.GetPrincipalContext()) {GivenName =$"*{term}*"}
            };

            foreach (var item in searchPrinciples)
            {
               var searcher = new PrincipalSearcher(item);
                var data = await Task.FromResult(searcher.FindAll());
                foreach (var principal1 in data)
                {
                    var principal = (UserPrincipal)principal1;
                    result.Add(principal);
                }

            }
            var cleaned = result.Distinct().ToList();

            return Ok(new{state=true,data= cleaned.Select(s => new UserEntry { EmailAddress = s.EmailAddress, EmployeeId = s.EmployeeId, FirstName = s.GivenName, LastName = s.Surname, MiddleName = s.MiddleName, Telephone = s.VoiceTelephoneNumber, UserName = s.SamAccountName }).Distinct() });

        }


        [Authorize]
        [Route("UserInfo")]
        public async Task<IHttpActionResult> UserInfo(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validateCredentials = _userManager.ValidateCredentials(model.UserName, model.Password);

            if (validateCredentials)
            {
                var user = await  Task.FromResult(_userManager.GetUser(model.UserName));
                
                return Ok(new  {state=true,data= new UserEntry
                        {
                            EmailAddress = user.EmailAddress,
                            EmployeeId = user.EmployeeId,
                            FirstName = user.GivenName,
                            MiddleName = user.MiddleName,
                            LastName = user.Surname,
                            Telephone = user.VoiceTelephoneNumber,
                            UserName = user.SamAccountName
                        }
                    }
                );
            }
            return Ok(new{state=false,message="user not found"});
        }

    }
}
