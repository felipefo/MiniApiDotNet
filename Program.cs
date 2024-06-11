using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using TodoApi.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext
builder.Services.AddDbContext<TodoDb>(opt => opt.UseSqlite("Data Source=TodoDatabase.db"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuração de Autenticação JWT
var key = Encoding.ASCII.GetBytes("minha_chave_secreta_nem_um_tanto_secreta");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "your_issuer",
            ValidAudience = "your_audience",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

builder.Services.AddCors();

var app = builder.Build();



// Configure o middleware CORS para permitir todas as origens, métodos e cabeçalhos
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// Middleware de Autenticação e Autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync()).RequireAuthorization();

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync()).RequireAuthorization();

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound()).RequireAuthorization();

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});


  app.MapPost("/login", async (User userLogin, TodoDb db) =>
{
    // Verifique o usuário no banco de dados
    var user = await db.Users.SingleOrDefaultAsync(u => u.Username == userLogin.Username && u.Password == userLogin.Password);

    if (user != null)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("minha_chave_secreta_nem_um_tanto_secreta");//nao compartilhar publicamente!
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = "your_issuer",
            Audience = "your_audience"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Results.Ok(new { Token = tokenString });
    }

    return Results.Unauthorized();
});


app.MapPost("/register", async (User newUser, TodoDb db) =>
{
    // Verifica se o username já existe no banco de dados
    if (await db.Users.AnyAsync(u => u.Username == newUser.Username))
    {
        return Results.Conflict("Username already exists.");
    }

    // Hash da senha usando BCrypt (ou outra biblioteca de hashing)
    string passwordHash = newUser.Password;

    // Cria o novo usuário
    var user = new User
    {
        Username = newUser.Username,
        Password = passwordHash
    };

    // Adiciona o usuário ao contexto do banco de dados e salva as mudanças
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created();
});


app.Run();
