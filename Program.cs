var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.email != null && loginDTO.senha != null)
        return Results.Ok("Login com sucesso");
    else
        return Results.Unauthorized();
});


app.Run();

public class LoginDTO
{
    public string email { get; set; }
    public string senha { get; set; }
}