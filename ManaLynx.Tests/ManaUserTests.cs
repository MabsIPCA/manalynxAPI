using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;

namespace ManaLynx.Tests
{
    public class ManaUserTests
    {
        private readonly ApplicationDbContext _db;
        private readonly IManaUserUtils _user;
        private readonly ILoginCredentialUtils _login;

        public ManaUserTests()
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _login = new LoginCredentialUtils(_db, new ConfigurationManager());
            _user = new ManaUserUtils(_db, _login, new ClienteUtils(_db, new PessoaUtils(_db), new DadoClinicoUtils(_db)));

            Seed(_db);
        }

        private void Seed(ApplicationDbContext context)
        {
            var users = new List<ManaUser>
            {
                new ManaUser
                {
                    Id = 1,
                    Email = "email",
                    Username = "username",
                    UserRole = Roles.Cliente.ToString(),

                }
            };

            _db.ManaUsers.AddRange(users);
            _db.SaveChanges();
        }

        //[Theory]
        //[InlineData("","","")]
        //[InlineData("test1","test1","test1")]
        //[InlineData("test2","test2","test2")]
        //public void ManaUser_ManaUserCreateValid_ShouldCreateManaUser(string email, string username, string password)
        //{
        //    var user = new RegisterRequest()
        //    {
        //        Email = email,
        //        Username = username,
        //        Password = password
        //    };

        //    var success = _user.AddCliente(user);

        //    var result = _user.User;

        //    Assert.True(success);
        //    Assert.NotNull(result);
        //    Assert.Null(result!.Pessoa);
        //    Assert.Null(result.PessoaId);
        //    Assert.NotNull(result.LoginCredential);
        //    Assert.NotNull(result.LoginCredentialNavigation);
        //}
    }
}
