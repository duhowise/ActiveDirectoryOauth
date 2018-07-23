using System.Data.Entity;

namespace ActiveDirectoryAuth.Models
{
    public class AdContext : DbContext
    {
        public virtual DbSet<DirectorySetup> DirectorySetups { get; set; }
        public virtual DbSet<USerPsk> USerPsks { get; set; }

        public AdContext():base("ADContext")
        {
            
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("ADContext".ToUpperInvariant());
            modelBuilder.Properties<string>().Configure(s=>s.IsUnicode(false).HasMaxLength(256));
            modelBuilder.Types().Configure(t => t.ToTable(t.ClrType.Name.ToUpperInvariant()));
            modelBuilder.Properties().Configure(t => t.HasColumnName(t.ClrPropertyInfo.Name.ToUpperInvariant()));
        }
    }
}