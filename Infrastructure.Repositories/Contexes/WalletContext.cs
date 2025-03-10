using Application.Commands.Interfaces;
using Domain.Portfolio;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Contexes;

public sealed class WalletContext : DbContext, IMigrateWalletContext
{
    public WalletContext(DbContextOptions<WalletContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wallet>(builder =>
        {
            builder.HasKey(nameof(Wallet.Id));
            builder.Ignore(wallet => wallet.DomainEvents);
            builder.OwnsOne(w => w.Balance);
            builder.OwnsMany(w => w.Shares);
        });
    }

    public void Migrate()
    {
        Database.Migrate();
    }
}