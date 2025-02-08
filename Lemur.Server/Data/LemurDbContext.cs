using Lemur.Server.Models.ModelsTelegram;
using Microsoft.EntityFrameworkCore;
using System;

namespace Lemur.Server.Data;

public class LemurDbContext : DbContext
{

    public DbSet<UserTelegram> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Animali> Animali { get; set; }
    public DbSet<GameResult> GameResults { get; set; }


    public LemurDbContext()
    {
    }

    public LemurDbContext(DbContextOptions<LemurDbContext> options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GroupPermission>()
            .HasKey(gp => new { gp.GroupId, gp.PermissionId }); // Chiave composta

        modelBuilder.Entity<GroupPermission>()
            .HasOne(gp => gp.Group)
            .WithMany(g => g.GroupPermissions)
            .HasForeignKey(gp => gp.GroupId);

        modelBuilder.Entity<GroupPermission>()
            .HasOne(gp => gp.Permission)
            .WithMany(p => p.GroupPermissions)
            .HasForeignKey(gp => gp.PermissionId);

        // Configurazione opzionale per la tabella Animali
        modelBuilder.Entity<Animali>(entity =>
        {
            entity.HasKey(a => a.Id); // Imposta Id come chiave primaria
            entity.Property(a => a.Id)
                  .ValueGeneratedOnAdd(); // Imposta Id come autoincremento
            entity.Property(a => a.Nome)
                  .IsRequired() // Imposta Nome come campo obbligatorio
                  .HasMaxLength(100); // Limita la lunghezza del campo
            entity.Property(a => a.Descrizione)
                  .HasMaxLength(500); // Limita la lunghezza del campo
        });

        modelBuilder.Entity<GameResult>()
                .Property(g => g.GameDate)
                .HasDefaultValueSql("GETDATE()");
    }

}
