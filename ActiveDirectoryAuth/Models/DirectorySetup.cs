using System.Data.Entity;

namespace ActiveDirectoryAuth.Models
{
    public class DirectorySetup
    {
        public string Domain { get; set; }
        public string IpAddress { get; set; }
        public int PortNumber { get; set; }
        public string ServiceUserName { get; set; }
        public string ServicePassword { get; set; }

    }

    public class AdContext:DbContext    
}