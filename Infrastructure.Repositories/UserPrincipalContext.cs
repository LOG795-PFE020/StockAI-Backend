using Application.Commands.Interfaces;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserPrincipalContext : IdentityDbContext<UserPrincipal, IdentityRole, string>, IMigrateUserContext
{
    public UserPrincipalContext(DbContextOptions<UserPrincipalContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserPrincipal>();
    }

    public void Migrate()
    {
        Database.Migrate();
    }
}