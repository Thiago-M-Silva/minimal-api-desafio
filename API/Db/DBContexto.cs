using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.entidades;
using minimal_api.Dominio.Entidades;

namespace MinimalAPI.Infraestrutura.DB;

public class DBContexto : DbContext
{

    private readonly IConfiguration _configuracaoAppSettings;
    public DBContexto(IConfiguration _configuracaoAppSettings)
    {
        _configuracaoAppSettings = _configuracaoAppSettings;
    }
    public DbSet<Administrador> administradores { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador
            {
                Id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        var stringConexao = _configuracaoAppSettings.GetConnectionString("sqlServer")?.ToString();
        if (!string.IsNullOrEmpty(stringConexao))
        {
            optionsBuilder.UseSqlServer(stringConexao);
        }
    }
}