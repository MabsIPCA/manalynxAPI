using Microsoft.EntityFrameworkCore;
using Xunit;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;

namespace ManaLynx.Tests
{
    public class ClienteTests
    {
        #region Context Creation
        private readonly ApplicationDbContext _db;
        private readonly IClienteUtils _cliente;
        private static List<Cliente> _clienteList = new();

        public ClienteTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _cliente = new ClienteUtils(_db, new PessoaUtils(_db), new DadoClinicoUtils(_db));

            Seed();
        }

        private static void Seed()
        {
            //Seeds Clientes
            _clienteList = new()
            {
                new()
                {
                    Id = 0,
                    IsLead = 0,
                    Profissao = "Profissao",
                    ProfissaoRisco = false,
                    Agente = null,
                    AgenteId = null,
                    ApolicePessoals = new List<ApolicePessoal> { new() },
                    ApoliceSaudes = new List<ApoliceSaude> { new() },
                    DadoClinico = null,
                    DadoClinicoId = null,
                    Pessoa = null,
                    PessoaId = null,
                    Veiculos = new List<Veiculo> { new() },
                },
                new()
                {
                    Id = 0,
                    IsLead = 1,
                    Profissao = "Profissao",
                    ProfissaoRisco = false,
                    Agente = null,
                    AgenteId = null,
                    ApolicePessoals = new List<ApolicePessoal> { new() },
                    ApoliceSaudes = new List<ApoliceSaude> { new() },
                    DadoClinico = null,
                    DadoClinicoId = null,
                    Pessoa = null,
                    PessoaId = null,
                    Veiculos = new List<Veiculo> { new() },
                },
                new()
                {
                    Id = 0,
                    IsLead = null,
                    Profissao = "Profissao",
                    ProfissaoRisco = false,
                    Agente = null,
                    AgenteId = null,
                    ApolicePessoals = new List<ApolicePessoal> { new() },
                    ApoliceSaudes = new List<ApoliceSaude> { new() },
                    DadoClinico = null,
                    DadoClinicoId = null,
                    Pessoa = null,
                    PessoaId = null,
                    Veiculos = new List<Veiculo> { new() },
                },
                // Profissao
                new()
                {
                    Id = 0,
                    IsLead = null,
                    Profissao = "Profissao,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()asdasdasd",
                    ProfissaoRisco = false,
                    Agente = null,
                    AgenteId = null,
                    ApolicePessoals = new List<ApolicePessoal> { new() },
                    ApoliceSaudes = new List<ApoliceSaude> { new() },
                    DadoClinico = null,
                    DadoClinicoId = null,
                    Pessoa = null,
                    PessoaId = null,
                    Veiculos = new List<Veiculo> { new() },
                },
                // Pessoa Error
                new()
                {
                    Id = 0,
                    IsLead = null,
                    Profissao = "Profissao",
                    ProfissaoRisco = false,
                    Agente = null,
                    AgenteId = null,
                    ApolicePessoals = new List<ApolicePessoal> { new() },
                    ApoliceSaudes = new List<ApoliceSaude> { new() },
                    DadoClinico = null,
                    DadoClinicoId = null,
                    Pessoa = new() { Cc = "cc-4,abcdefghijklmnopqrstuvwxyz,0123456789,!?;:][{}()asdasd" },
                    PessoaId = null,
                    Veiculos = new List<Veiculo> { new() },
                },
                // DadoClinico Error
                new()
                {
                    Id = 0,
                    IsLead = null,
                    Profissao = "Profissao",
                    ProfissaoRisco = false,
                    Agente = null,
                    AgenteId = null,
                    ApolicePessoals = new List<ApolicePessoal> { new() },
                    ApoliceSaudes = new List<ApoliceSaude> { new() },
                    DadoClinico = new() { Tensao = "fail" },
                    DadoClinicoId = null,
                    Pessoa = null,
                    PessoaId = null,
                    Veiculos = new List<Veiculo> { new() },
                },
            };
        }
        #endregion
        #region Tests
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Cliente_ClienteCreateValidField_ShouldCreateCliente(int index)
        {
            bool success = _cliente.AddCliente(_clienteList[index]);

            var result = _cliente.Model;

            Assert.True(success);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(_cliente.Error));
            Assert.Null(result!.Agente);
            Assert.Equal(0, result.ApolicePessoals.Count);
            Assert.Equal(0, result.ApoliceSaudes.Count);
            Assert.NotNull(result.DadoClinico);
            Assert.NotNull(result.Pessoa);
            Assert.Equal(0, result.Veiculos.Count);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Cliente_ClienteCreateInvalidField_ShouldReturnError(int index)
        {
            bool success = _cliente.AddCliente(_clienteList[index]);

            var result = _cliente.Model;

            Assert.False(success);
            Assert.Null(result);
            Assert.False(string.IsNullOrEmpty(_cliente.Error));
        }
        #endregion
    }
}
