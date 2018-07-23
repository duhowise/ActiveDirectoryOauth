using System.Linq;
using ActiveDirectoryAuth.Models;

namespace ActiveDirectoryAuth.Services
{
	public  class PskService
	{
		public static USerPsk GetPsk(string employeeId)
		{
			using (var context=new AdContext())
			{
				var psk = context.USerPsks.FirstOrDefault(o => o.EmployeeId == employeeId);
				if (psk != null)
					return context.USerPsks.FirstOrDefault(o => o.EmployeeId == employeeId);
				{
					context.USerPsks.Add(new USerPsk
					{
						EmployeeId = employeeId,
						Psk = TimeSensitivePassCode.GeneratePresharedKey()
					});
					context.SaveChanges();
					return context.USerPsks.FirstOrDefault(o => o.EmployeeId == employeeId);
				}

			}

		}
	}
}