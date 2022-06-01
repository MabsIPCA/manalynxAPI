using Microsoft.EntityFrameworkCore;
using Xunit;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
namespace ManaLynx.Tests
{
    public class PessoaTests
    {
        #region Context Creation
        private readonly ApplicationDbContext _db;
        private readonly IPessoaUtils _pessoa;
        private static List<Pessoa> _pessoaList = new();

        public PessoaTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _pessoa = new PessoaUtils(_db);

            //Populate db
            Seed(_db);
        }

        private static void Seed(ApplicationDbContext context)
        {
            //Seeds Pessoas to the db
            _pessoaList = new List<Pessoa>
            {
                new Pessoa
                {
                    Id = 1,
                    Nome = "Nome1",
                    Cc = "cc-number1",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Solteiro.ToString(),
                    Nacionalidade = "nacionalidade1",
                    Nif = "nif-number1",
                    Nss = "nss-number1",
                    Nus = "nus-number1",
                    ValidadeCc = DateTime.MinValue
                },
                new Pessoa
                {
                    Id = 2,
                    Nome = "Nome2",
                    Cc = "cc-number2",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.UniaoDeFacto.ToString(),
                    Nacionalidade = "nacionalidade2",
                    Nif = "nif-number2",
                    Nss = "nss-number2",
                    Nus = "nus-number2",
                    ValidadeCc = DateTime.MinValue
                },
                new Pessoa
                {
                    Id = 3,
                    Nome = "Nome3",
                    Cc = "cc-number3",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Casado.ToString(),
                    Nacionalidade = "nacionalidade3",
                    Nif = "nif-number3",
                    Nss = "nss-number3",
                    Nus = "nus-number3",
                    ValidadeCc = DateTime.MinValue
                },
                // Nus
                new Pessoa
                {
                    Id = 4,
                    Nome = "Nome4",
                    Cc = "cc-number4",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Solteiro.ToString(),
                    Nacionalidade = "nacionalidade4",
                    Nif = "nif-number4",
                    Nss = "nss-number4",
                    Nus = "nus-number4,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()",
                    ValidadeCc = DateTime.MinValue
                },
                // Nss
                new Pessoa
                {
                    Id = 5,
                    Nome = "Nome5",
                    Cc = "cc-number5",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.UniaoDeFacto.ToString(),
                    Nacionalidade = "nacionalidade5",
                    Nif = "nif-number5",
                    Nss = "nss-number5,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()",
                    Nus = "nus-number5",
                    ValidadeCc = DateTime.MinValue
                },
                // Nif
                new Pessoa
                {
                    Id = 6,
                    Nome = "Nome6",
                    Cc = "cc-number6",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Casado.ToString(),
                    Nacionalidade = "nacionalidade6",
                    Nif = "nif-number6,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()",
                    Nss = "nss-number6",
                    Nus = "nus-number6",
                    ValidadeCc = DateTime.MinValue
                },
                // Cc
                new Pessoa
                {
                    Id = 7,
                    Nome = "Nome7",
                    Cc = "cc-number7,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Casado.ToString(),
                    Nacionalidade = "nacionalidade7",
                    Nif = "nif-number7",
                    Nss = "nss-number7",
                    Nus = "nus-number7",
                    ValidadeCc = DateTime.MinValue
                },
                // Nome
                new Pessoa
                {
                    Id = 8,
                    Nome = "Nome8,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()",
                    Cc = "cc-number8",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Casado.ToString(),
                    Nacionalidade = "nacionalidade8",
                    Nif = "nif-number8",
                    Nss = "nss-number8",
                    Nus = "nus-number8",
                    ValidadeCc = DateTime.MinValue
                },
                // Nacionalidade
                new Pessoa
                {
                    Id = 9,
                    Nome = "Nome9",
                    Cc = "cc-number9",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = EstadoCivil.Casado.ToString(),
                    Nacionalidade = "nacionalidade9,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()",
                    Nif = "nif-number9",
                    Nss = "nss-number9",
                    Nus = "nus-number9",
                    ValidadeCc = DateTime.MinValue
                },
                // EstadoCivil
                new Pessoa
                {
                    Id = 10,
                    Nome = "Nome11",
                    Cc = "cc-number11",
                    DataNascimento = DateTime.MinValue,
                    EstadoCivil = "fail",
                    Nacionalidade = "nacionalidade11",
                    Nif = "nif-number11",
                    Nss = "nss-number11",
                    Nus = "nus-number11",
                    ValidadeCc = DateTime.MinValue
                },
            };

            //Seeds Clientes to the db
            var clienteList = new List<Cliente>
            {
                new Cliente{ Id = 1, PessoaId = 1 },
                new Cliente{ Id = 2, PessoaId = 2 },
                new Cliente{ Id = 3, PessoaId = 3 },
            };


            context.Pessoas.AddRange(new[] { _pessoaList[0], _pessoaList[1], _pessoaList[2] });
            context.Clientes.AddRange(clienteList);
            context.SaveChanges();
        }
        #endregion
        #region Tests
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Pessoa_PessoaCreateValidField_ShouldCreatePessoa(int index)
        {
            bool success = _pessoa.AddPessoa(_pessoaList[index]);

            var result = _pessoa.Model;

            Assert.True(success);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(_pessoa.Error));
            Assert.Equal(0, result!.Agentes.Count);
            Assert.Equal(1, result.Clientes.Count);
            Assert.Equal(0, result.Contactos.Count);
            Assert.Equal(0, result.ManaUsers.Count);
        }

        [Theory]
        [InlineData(3, "Nus")]
        [InlineData(4, "Nss")]
        [InlineData(5, "Nif")]
        [InlineData(6, "Cc")]
        [InlineData(7, "Nome")]
        [InlineData(8, "Nacionalidade")]
        [InlineData(9, "EstadoCivil")]
        public void Pessoa_PessoaCreateInvalidField_ShouldReturnError(int index, string field)
        {
            bool success = _pessoa.AddPessoa(_pessoaList[index]);

            var result = _pessoa.Model;

            Assert.False(success);
            Assert.Null(result);
            Assert.NotNull(_pessoa.Error);
            Assert.Equal(field + " is too Long.", _pessoa.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Pessoa_PessoaUpdateValidField_ShouldUpdatePessoa(int index)
        {
            var success = _pessoa.UpdatePessoa(_pessoaList[index]);

            var result = _pessoa.Model;

            Assert.True(success);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(_pessoa.Error));
        }

        [Theory]
        [InlineData(3, "Nus")]
        [InlineData(4, "Nss")]
        [InlineData(5, "Nif")]
        [InlineData(6, "Cc")]
        [InlineData(7, "Nome")]
        [InlineData(8, "Nacionalidade")]
        [InlineData(9, "EstadoCivil")]
        public void Pessoa_PessoaUpdateInvalidField_ShouldReturnError(int index, string field)
        {
            bool success = _pessoa.UpdatePessoa(_pessoaList[index]);

            var result = _pessoa.Model;

            Assert.False(success);
            Assert.Null(result);
            Assert.NotNull(_pessoa.Error);
            Assert.Equal(field + " is too Long.", _pessoa.Error);
        }
        #endregion
    }
}
