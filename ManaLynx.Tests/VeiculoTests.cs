using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManaLynxAPI;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Autofac.Extras.Moq;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Controllers;
using ManaLynxAPI.Utils;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.AspNetCore.Mvc;

namespace ManaLynx.Tests
{
    public class VeiculoTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IVeiculoUtils _veiUtils;

        public VeiculoTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _veiUtils = new VeiculoUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds categoriaVeiculos to the db
            var categoriaVeiculosList = new List<CategoriaVeiculo>
            {
                new CategoriaVeiculo{ Id = 1, Categoria = "Foo1" },
                new CategoriaVeiculo{ Id = 2, Categoria = "Foo2" },
                new CategoriaVeiculo{ Id = 3, Categoria = "Foo3" },
                new CategoriaVeiculo{ Id = 4, Categoria = "Foo4" }

            };

            //Seeds Equipas to the db
            var clienteList = new List<Cliente>
            {
                new Cliente{ Id = 1, Profissao="dummy", ProfissaoRisco = false, PessoaId = 1, DadoClinicoId=1, AgenteId=null, IsLead = 0 },
                new Cliente{ Id = 2, Profissao="dummy", ProfissaoRisco = false, PessoaId = 2, DadoClinicoId=2, AgenteId=null, IsLead = 0 },
                new Cliente{ Id = 3, Profissao="dummy", ProfissaoRisco = false, PessoaId = 3, DadoClinicoId=3, AgenteId=null, IsLead = 0 },
                new Cliente{ Id = 4, Profissao="dummy", ProfissaoRisco = false, PessoaId = 4, DadoClinicoId=4, AgenteId=null, IsLead = 0 }

            };


            context.CategoriaVeiculos.AddRange(categoriaVeiculosList);
            context.Clientes.AddRange(clienteList);
            context.SaveChanges();
        }

        #endregion


        #region Tests
        [Theory]
        [InlineData(1, 1)] // data is given for the arguments this way
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        public void Veiculo_VeiculoCreateValidCliente_ShouldCreateVeiculo(int clienteId, int categoriaId)
        {
            //Arrange
            var veiculo = new Veiculo() //The object to test creation
            {
                Vin = "01234567891234567",
                Matricula = "00-aa-00",
                Ano = 2000,
                Mes = 1,
                Marca = "dummy",
                Modelo =  "dummy",
                Cilindrada = 1000,
                Portas = 3,
                Lugares = 2,
                Potencia = 100,
                Peso = 1000,
                ClienteId = clienteId,
                CategoriaVeiculoId  = categoriaId
            };

            //Act
            var result = _veiUtils.createVeiculo(veiculo);

            //Assert
            Assert.IsType<Veiculo>(result.Item2);
            Assert.Equal("", result.Item1);
            Assert.Equal(clienteId, result.Item2.ClienteId);
            Assert.Equal(categoriaId, result.Item2.CategoriaVeiculoId);
        }

        [Theory]
        [InlineData(5, 1)] // data is given for the arguments this way
        [InlineData(6, 2)]
        [InlineData(7, 3)]
        public void Veiculo_VeiculoCreateValidCliente_ShouldReturnNull(int clienteId, int categoriaId)
        {
            //Arrange
            var veiculo = new Veiculo() //The object to test creation
            {
                Vin = "01234567891234567",
                Matricula = "00-aa-00",
                Ano = 2000,
                Mes = 1,
                Marca = "dummy",
                Modelo = "dummy",
                Cilindrada = 1000,
                Portas = 3,
                Lugares = 2,
                Potencia = 100,
                Peso = 1000,
                ClienteId = clienteId,
                CategoriaVeiculoId = categoriaId
            };

            //Act
            var result = _veiUtils.createVeiculo(veiculo);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Invalid Cliente", result.Item1);
        }

        [Theory]
        [InlineData(1, 1)] // data is given for the arguments this way
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        public void Veiculo_VeiculoCreateValidCategoria_ShouldCreateVeiculo(int clienteId, int categoriaId)
        {
            //Arrange
            var veiculo = new Veiculo() //The object to test creation
            {
                Vin = "01234567891234567",
                Matricula = "00-aa-00",
                Ano = 2000,
                Mes = 1,
                Marca = "dummy",
                Modelo = "dummy",
                Cilindrada = 1000,
                Portas = 3,
                Lugares = 2,
                Potencia = 100,
                Peso = 1000,
                ClienteId = clienteId,
                CategoriaVeiculoId = categoriaId
            };

            //Act
            var result = _veiUtils.createVeiculo(veiculo);

            //Assert
            Assert.IsType<Veiculo>(result.Item2);
            Assert.Equal("", result.Item1);
            Assert.Equal(clienteId, result.Item2.ClienteId);
            Assert.Equal(categoriaId, result.Item2.CategoriaVeiculoId);
        }

        [Theory]
        [InlineData(1, 6)] // data is given for the arguments this way
        [InlineData(2, 7)]
        [InlineData(3, 8)]
        public void Veiculo_VeiculoCreateValidCategoria_ShouldReturnNull(int clienteId, int categoriaId)
        {
            //Arrange
            var veiculo = new Veiculo() //The object to test creation
            {
                Vin = "01234567891234567",
                Matricula = "00-aa-00",
                Ano = 2000,
                Mes = 1,
                Marca = "dummy",
                Modelo = "dummy",
                Cilindrada = 1000,
                Portas = 3,
                Lugares = 2,
                Potencia = 100,
                Peso = 1000,
                ClienteId = clienteId,
                CategoriaVeiculoId = categoriaId
            };

            //Act
            var result = _veiUtils.createVeiculo(veiculo);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Invalid Categoria", result.Item1);
        }

        [Theory]
        [InlineData("98798798797897877")] // data is given for the arguments this way
        [InlineData("11111111111111111")]
        [InlineData("01234567891234567")]
        public void Veiculo_VeiculoCreateValidVIN_ShouldCreateVeiculo(string vin)
        {
            //Arrange
            var veiculo = new Veiculo() //The object to test creation
            {
                Vin = vin,
                Matricula = "00-aa-00",
                Ano = 2000,
                Mes = 1,
                Marca = "dummy",
                Modelo = "dummy",
                Cilindrada = 1000,
                Portas = 3,
                Lugares = 2,
                Potencia = 100,
                Peso = 1000,
                ClienteId = 1,
                CategoriaVeiculoId = 1
            };

            //Act
            var result = _veiUtils.createVeiculo(veiculo);

            //Assert
            Assert.IsType<Veiculo>(result.Item2);
            Assert.Equal("", result.Item1);
            Assert.Equal(vin, result.Item2.Vin);
        }

        [Theory]
        [InlineData("")] // data is given for the arguments this way
        [InlineData("11111111111111111111")]
        [InlineData("012345678912345671231")]
        public void Veiculo_VeiculoCreateValidVIN_ShouldReturnNull(string vin)
        {
            //Arrange
            var veiculo = new Veiculo() //The object to test creation
            {
                Vin = vin,
                Matricula = "00-aa-00",
                Ano = 2000,
                Mes = 1,
                Marca = "dummy",
                Modelo = "dummy",
                Cilindrada = 1000,
                Portas = 3,
                Lugares = 2,
                Potencia = 100,
                Peso = 1000,
                ClienteId = 1,
                CategoriaVeiculoId = 1
            };

            //Act
            var result = _veiUtils.createVeiculo(veiculo);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Invalid Field", result.Item1);
        }

        #endregion


    }
}
