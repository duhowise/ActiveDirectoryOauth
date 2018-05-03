namespace ActiveDirectoryAuth.Models
{
    public class DirectorySetup
    {
        public int Id { get; set; } 
        public string DomainName { get; set; }
        public string IpAddress { get; set; }   
        public string ServiceUserName { get; set; }
        public string ServicePassword { get; set; }

    }
}