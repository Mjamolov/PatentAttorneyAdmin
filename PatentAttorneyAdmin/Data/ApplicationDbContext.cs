using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ClientRequest> ClientRequests => Set<ClientRequest>();
    public DbSet<RequestMessage> RequestMessages => Set<RequestMessage>();
    public DbSet<ServiceApplication> ServiceApplications => Set<ServiceApplication>();
    public DbSet<ServiceApplicationDocument> ServiceApplicationDocuments => Set<ServiceApplicationDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ClientRequest>()
            .HasOne(r => r.User)
            .WithMany(u => u.ClientRequests)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RequestMessage>()
            .HasOne(m => m.ClientRequest)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.ClientRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ServiceApplication>()
            .HasOne(a => a.User)
            .WithMany(u => u.ServiceApplications)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceApplication>()
            .HasOne(a => a.ServiceCategory)
            .WithMany()
            .HasForeignKey(a => a.ServiceCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceApplicationDocument>()
            .HasOne(d => d.ServiceApplication)
            .WithMany(a => a.Documents)
            .HasForeignKey(d => d.ServiceApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
