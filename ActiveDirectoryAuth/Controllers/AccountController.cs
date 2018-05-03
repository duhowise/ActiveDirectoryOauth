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
            _userManager=new ActiveDirectoryManager("192.168.183.129", "DC=Devhost,DC=com", "Administrator", "D3vMachin3");
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

            return Ok(result.Select(s=>new UserEntry{EmailAddress = s.EmailAddress,EmployeeId = s.EmployeeId,FirstName = s.GivenName,LastName = s.Surname,MiddleName = s.MiddleName,Telephone = s.VoiceTelephoneNumber,UserName = s.SamAccountName}));
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
                
                return Ok(new UserEntry
                    {
                        EmailAddress = user.EmailAddress,
                        EmployeeId = user.EmployeeId,
                        FirstName = user.GivenName,
                        MiddleName = user.MiddleName,
                        LastName = user.Surname,
                        Telephone = user.VoiceTelephoneNumber,
                        UserName = user.SamAccountName
                    }
                );
            }
            return Ok(new{state=false,message="user not found"});
        }

    }
}
