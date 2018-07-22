using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ActiveDirectoryAuth.Models;
using ActiveDirectoryAuth.Services;
using Microsoft.AspNet.Identity;

namespace ActiveDirectoryAuth.Controllers
{
	[RoutePrefix("api/Account")]
    public partial class AccountController : ApiController
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


	    [Authorize]
	    [Route("OTP")]
	    [HttpPost]
	    public async Task<IHttpActionResult> GetOtp(LoginModel login)
	    {
		    if (!ModelState.IsValid)
		    {
			    return BadRequest(ModelState);
		    }

		    var valid =  _userManager.ValidateCredentials(login.UserName, login.Password);



		    if (!valid) return NotFound();
		    var user = _userManager.GetUser(login.UserName);
		    var code = TimeSensitivePassCode.GetListOfOtPs(user.Psk)[1];
		    if (!string.IsNullOrEmpty(user.VoiceTelephoneNumber))
		    {
			   await   new SmsService().SendAsync(new IdentityMessage
			 {
				 Body = $"Your Pin Is:\n {code}",
				 Destination = user.VoiceTelephoneNumber
			 });
		    }
		    else
		    {
			    ModelState.AddModelError("PhoneNumber", "user's Phone number is not available");
			    return BadRequest(ModelState);
		    }
		    return Ok(code);
	    }
		[Authorize]
	    [Route("Verify")]
	    [HttpPost]
	    public async Task<IHttpActionResult> VerifyOtp(VerifyOtpModel login)
	    {
		    if (!ModelState.IsValid)
		    {
			    return BadRequest(ModelState);
		    }

		    var user = await Task.FromResult(_userManager.ValidateCredentials(login.UserName, login.Password));

		    if (user == null) return NotFound();
		    var state = TimeSensitivePassCode.GetListOfOtPs(user.Psk).Any(c => c.Equals(login.Code));
		    return Ok(new { state = state });
	    }

	}
}
