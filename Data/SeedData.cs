using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace TodoApi.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new TodoDb(
                serviceProvider.GetRequiredService<DbContextOptions<TodoDb>>()))
            {
                // Verifica se já existem usuários no banco de dados
                if (context.Users.Any())
                {
                    return;   // O banco de dados já foi semeado
                }

                context.Users.AddRange(
                    new User
                    {
                        Username = "test",
                        Password = "password" // Em produção, nunca armazene senhas em texto puro
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
