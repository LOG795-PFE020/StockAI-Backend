using Application.Commands.Interfaces;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Contexes;

public sealed class UserPrincipalContext : IdentityDbContext<UserPrincipal, IdentityRole, string>, IMigrateUserContext
{
    public UserPrincipalContext(DbContextOptions<UserPrincipalContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserPrincipal>(builder =>
        {
            builder.Property(nameof(UserPrincipal.WalletId));
        });
    }

    public void Migrate()
    {
        Database.Migrate();
    }
}