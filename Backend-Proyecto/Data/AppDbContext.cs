using Backend_Proyecto.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Averia> Averias => Set<Averia>();
    public DbSet<AveriaHistorial> AveriasHistorial => Set<AveriaHistorial>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<ActividadPlomeria> ActividadesPlomeria => Set<ActividadPlomeria>();
    public DbSet<ActividadFontanero> ActividadesFontanero => Set<ActividadFontanero>();
    public DbSet<LecturaMedidor> LecturasMedidor => Set<LecturaMedidor>();
    public DbSet<Comunicado> Comunicados => Set<Comunicado>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<SecuenciaContador> SecuenciasContador => Set<SecuenciaContador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Averia>(entity =>
        {
            entity.HasIndex(a => a.NumeroSeguimiento).IsUnique();
            entity.HasOne(a => a.FontaneroAsignado)
                .WithMany()
                .HasForeignKey(a => a.FontaneroAsignadoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AveriaHistorial>(entity =>
        {
            entity.HasOne(h => h.Averia)
                .WithMany()
                .HasForeignKey(h => h.AveriaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasIndex(s => s.NumeroSeguimiento).IsUnique();
        });

        modelBuilder.Entity<ActividadPlomeria>(entity =>
        {
            entity.HasKey(a => a.Id);
        });

        modelBuilder.Entity<ActividadFontanero>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Fontanero)
                .WithMany()
                .HasForeignKey(a => a.FontaneroId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.LecturaMedidor)
                .WithMany()
                .HasForeignKey(a => a.LecturaMedidorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LecturaMedidor>(entity =>
        {
            entity.HasOne(l => l.Fontanero)
                .WithMany()
                .HasForeignKey(l => l.FontaneroId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.NombreUsuario).IsUnique();
        });

        modelBuilder.Entity<SecuenciaContador>(entity =>
        {
            entity.HasKey(s => s.Prefijo);
        });
    }
}
