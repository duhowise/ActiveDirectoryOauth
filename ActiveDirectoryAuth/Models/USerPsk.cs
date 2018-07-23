using System.ComponentModel.DataAnnotations;

namespace ActiveDirectoryAuth.Models
{
	public class USerPsk
	{
		[Key]public string EmployeeId { get; set; }
		public string Psk { get; set; }
	}
}