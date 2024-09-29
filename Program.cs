using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.dominio.services;
using minimal_api.Dominio.ModelViews;
using minimal_api.infraestrutura.Interfaces;
using MinimalApi.DTOs;
using MinimalAPI.Infraestrutura.DB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<iAdministradorService, AdministradorService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBContexto>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("sqlServer")
    );
});

var app = builder.Build();

// Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");

// Administrador
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorService administradorService) =>
{
    if (administradorService.Login(loginDTO) != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

// Veiculos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoService veiculoService) =>
{
    var veiculo = new Veiculo{
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoService.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", (int? pagina, iVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute]int? id, iVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.BuscaPorId(Id);
    if(veiculo == null) return Results.NotFound();

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute]int? id, VeiculoDTO veiculoDTO, iVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.BuscaPorId(Id);
    if(veiculo == null) return Results.NotFound();

    veiculo.Nome = VeiculoDTO.Nome;
    veiculo.Marca = VeiculoDTO.Marca;
    veiculo.Ano = VeiculoDTO.Ano;

    veiculoService.Atualizar(veiculo);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute]int? id, iVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.BuscaPorId(Id);
    if(veiculo == null) return Results.NotFound();

    veiculoService.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veiculos");

// App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();