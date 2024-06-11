using Microsoft.EntityFrameworkCore;
using TodoApi.Models;



public class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<User> Users { get; set; } // Adicionar DbSet para Users
}