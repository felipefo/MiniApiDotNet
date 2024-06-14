# MiniApiDotNet
 Mini api usando o sqlite em dot net para todo 

# Notas
A senha nao esta sendo sala cripritografada. E não se deve deixar a senha do jwt token no repositorio.

# Usando 
- Faça o checkout do repositorio 
- Abra o projeto no visual studio 
- Precione F5 para rodar o projeto
- Abra o link https://localhost:{porta}/swagger/

# Para modificar o api 
 - Altere a classe Todo.cs com campos que queira adicionar 
 - dotnet ef migrations add MigrationName
 - dotnet ef database update
