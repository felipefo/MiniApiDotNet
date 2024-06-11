namespace TodoApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Note que em produção, as senhas devem ser armazenadas de forma segura (hashing)
    }
}
