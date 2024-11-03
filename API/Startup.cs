using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.dominio.entidades;
using minimal_api.dominio.services;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enuns;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.infraestrutura.Interfaces;
using MinimalApi.DTOs;
using MinimalAPI.Infraestrutura.DB;

namespace minimal_api
{
    public class Startup

    {
        public Startup(IConfiguration configuration)
        {
            if (configuration != null)
            {
                Configuration = configuration;
                key = Configuration?.GetSection("Jwt").ToString() ?? "";
            }
        }

        private string key = "";

        public IConfiguration? Configuration { get; set; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<iAdministradorService, AdministradorService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT desta maneira: Bearer {seu token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });


            var key = Configuration.GetSection("Jwt").ToString();
            if (string.IsNullOrEmpty(key)) key = "123456";


            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });


            services.AddDbContext<DBContexto>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("sqlServer")
                );
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseSwagger();
            app.UseSwaggerUi();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Administrador
                string GerarTokenJwt(Administrador administrador)
                {
                    if (string.IsNullOrEmpty(key)) return string.Empty;

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>(){
                        new Claim("Email", administrador.Email),
                        new Claim("Perfil", administrador.Perfil),
                        new Claim(ClaimTypes.Role, administrador.Perfil)
                    };

                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }

                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorService administradorService) =>
                {
                    var adm = administradorService.Login(loginDTO);
                    if (adm != null)
                    {
                        string token = GerarTokenJwt(adm);
                        return Results.Ok(new AdministradorLogado
                        {
                            Email = adm.Email,
                            Perfil = adm.Perfil,
                            Token = token
                        });

                    }
                    else
                        return Results.Unauthorized();
                })
                .AllowAnonymous()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administradores");

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, iAdministradorService administradorService) =>
                {
                    var validacao = new ErrosDeValidacao
                    {
                        Mensagens = new List<string>()
                    };

                    if (string.IsNullOrEmpty(administradorDTO.Email))
                        validacao.Mensagens.Add("Email não pode ser vazio");

                    if (string.IsNullOrEmpty(administradorDTO.Senha))
                        validacao.Mensagens.Add("Senha não pode ser vazia");

                    if (administradorDTO.Perfil == null)
                        validacao.Mensagens.Add("Perfil não pode ser vazio");

                    if (validacao.Mensagens.Count > 0)
                        return Results.BadRequest(validacao);

                    var administrador = new Administrador
                    {
                        Email = administradorDTO.Email,
                        Senha = administradorDTO.Senha,
                        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.editor.ToString()
                    };

                    administradorService.Incluir(administrador);

                    return Results.Created();
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administradores");

                endpoints.MapGet("/administradores", ([FromBody] int? pagina, iAdministradorService administradorService) =>
                {
                    var adms = new List<AdministradorModelView>();
                    var administradores = administradorService.Todos(pagina);

                    foreach (var adm in administradores)
                    {
                        adms.Add(new AdministradorModelView
                        {
                            Email = adm.Email,
                            Perfil = (Perfil)Enum.Parse(typeof(Perfil), adm.Perfil)
                        });
                    }
                    return Results.Ok(administradorService.Todos(pagina));
                })
                .RequireAuthorization()
                .WithTags("Administradores");

                endpoints.MapPost("/administradores/{id}", ([FromBody] int id, iAdministradorService administradorService) =>
                {
                    var adm = administradorService.BuscaPorId(id);
                    if (adm == null) return Results.NotFound();

                    return Results.Ok(adm);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administradores");
                #endregion Administrador

                #region Veiculos
                ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
                {
                    var validacao = new ErrosDeValidacao
                    {
                        Mensagens = new List<string>()
                    };

                    if (string.IsNullOrEmpty(veiculoDTO.Nome))
                    {
                        validacao.Mensagens.Add("O nome não pode ser vazio");
                    }

                    if (string.IsNullOrEmpty(veiculoDTO.Marca))
                    {
                        validacao.Mensagens.Add("A marca não pode ficar em branco");
                    }

                    if (veiculoDTO.Ano < 1950)
                    {
                        validacao.Mensagens.Add("Veiculo muito antigo, aceito somente anos superiores a 1950");
                    }

                    return validacao;
                }

                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, iVeiculosService veiculoService) =>
                {
                    var validacao = validaDTO(veiculoDTO);
                    if (validacao.Mensagens.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    var veiculo = new Veiculo
                    {
                        Nome = veiculoDTO.Nome,
                        Marca = veiculoDTO.Marca,
                        Ano = veiculoDTO.Ano
                    };

                    veiculoService.Incluir(veiculo);

                    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                .WithTags("Veiculos");

                endpoints.MapGet("/veiculos", (int? pagina, iVeiculosService veiculoService) =>
                {
                    var veiculos = veiculoService.Todos(pagina);
                    return Results.Ok(veiculos);
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                .WithTags("Veiculos");

                endpoints.MapGet("/veiculos/{id}", ([FromRoute] int? id, iVeiculosService veiculoService) =>
                {
                    var veiculo = veiculoService.BuscaPorId((int)id);
                    if (veiculo == null) return Results.NotFound();

                    return Results.Ok(veiculo);
                }).RequireAuthorization().WithTags("Veiculos");

                endpoints.MapPut("/veiculos/{id}", ([FromRoute] int? id, VeiculoDTO veiculoDTO, iVeiculosService veiculoService) =>
                {
                    var validacao = validaDTO(veiculoDTO);
                    if (validacao.Mensagens.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    var veiculo = veiculoService.BuscaPorId((int)id);
                    if (veiculo == null) return Results.NotFound();

                    veiculo.Nome = veiculoDTO.Nome;
                    veiculo.Marca = veiculoDTO.Marca;
                    veiculo.Ano = veiculoDTO.Ano;

                    veiculoService.Atualizar(veiculo);

                    return Results.Ok(veiculo);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");

                endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int? id, iVeiculosService veiculoService) =>
                {
                    var veiculo = veiculoService.BuscaPorId((int)id);
                    if (veiculo == null) return Results.NotFound();

                    veiculoService.Apagar(veiculo);

                    return Results.NoContent();
                })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Veiculos");
                #endregion
            });
        }
    }
}