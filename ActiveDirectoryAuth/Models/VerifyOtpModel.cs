using System.ComponentModel.DataAnnotations;

namespace ActiveDirectoryAuth.Models
{
	
		public class VerifyOtpModel
	    {
		    [Required]
		    [Display(Name = "User name")]
		    public string UserName
		    {
			    get; set;
		    }

		    [Required]
		    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		    [DataType(DataType.Password)]
		    [Display(Name = "Password")]
		    public string Password
		    {
			    get; set;
		    }


		    [Required]
		    [StringLength(6, ErrorMessage = "The {0} must be {2} characters long.", MinimumLength = 6)]
		    public string Code
		    {
			    get; set;
		    }

	    }

	
}
