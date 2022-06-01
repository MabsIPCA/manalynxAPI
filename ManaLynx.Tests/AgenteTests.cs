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
    public class AgenteTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IPessoaUtils _pesUtils;
        IAgenteUtils _ageUtils;

        public AgenteTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _pesUtils = new PessoaUtils(_db);

            _ageUtils = new AgenteUtils(_db, _pesUtils);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds agentes to the db
            var agenteList = new List<Agente>
            {
                new Agente{ Id = 1, Nagente = 1, EquipaId = 1 },
                new Agente{ Id = 2, Nagente = 2, EquipaId = 2 },
                new Agente{ Id = 3, Nagente = 3, EquipaId = 3 },
                new Agente{ Id = 4, Nagente = 4, EquipaId = 4 },
                new Agente{ Id = 5, Nagente = 5, EquipaId = 5 },
                new Agente{ Id = 6, Nagente = 6, EquipaId = 6 },
                new Agente{ Id = 7, Nagente = 7, EquipaId = 7 },
                new Agente{ Id = 8, Nagente = 8, EquipaId = 8 },
                new Agente{ Id = 9, Nagente = 9, EquipaId = 9 }
            };

            //Seeds Equipas to the db
            var equipaList = new List<Equipa>
            {
                new Equipa{ Id = 1, Nome = "dummy", Regiao = "teste" },
                new Equipa{ Id = 2, Nome = "dummy", Regiao = "teste" },
                new Equipa{ Id = 3, Nome = "dummy", Regiao = "teste" }
            };


            context.Equipas.AddRange(equipaList);
            context.Agentes.AddRange(agenteList);
            context.SaveChanges();
        }

#endregion

        #region Tests
        [Theory]
        [InlineData(1,1)] // data is given for the arguments this way
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        public void Agente_AgenteCreateValidEquipa_ShouldCreateAgente(int equipaId, int nagente)
        {
            //Arrange

            //this pessoa is created for testing purposes
            //this object is not being tested, only agente
            var pessoaDummy = new Pessoa {
                Nome = "Dummy",
                DataNascimento = DateTime.Parse("01-01-1990"),
                Nacionalidade = "Portugues",
                Cc = "1234123",
                ValidadeCc = DateTime.Parse("01-01-1990"),
                Nif = "123412",
                Nss = "123413",
                Nus = "123412",
                EstadoCivil = "Casado"
            };

            var agenteTest = new Agente() //The object to test creation
            {
                EquipaId = equipaId,
                Nagente = nagente,
                Pessoa = pessoaDummy

            };

            //Act
            var result = _ageUtils.CreateAgente(agenteTest);

            //Assert
            Assert.IsType<Agente>(result.Item1);
            Assert.Equal("Dummy", result.Item1.Pessoa.Nome);
            Assert.Equal("", result.Item2);
           ;

        }

        [Theory]
        [InlineData(5, 1)] // data is given for the arguments this way
        [InlineData(6, 2)]
        [InlineData(0, 3)]
        public void Agente_AgenteCreateInvalidEquipa_ShouldReturnNull(int equipaId, int nagente)
        {
            //Arrange

            //this pessoa is created for testing purposes
            //this object is not being tested, only agente
            var pessoaDummy = new Pessoa
            {
                Nome = "Dummy",
                DataNascimento = DateTime.Parse("01-01-1990"),
                Nacionalidade = "Portugues",
                Cc = "1234123",
                ValidadeCc = DateTime.Parse("01-01-1990"),
                Nif = "123412",
                Nss = "123413",
                Nus = "123412",
                EstadoCivil = "Casado"
            };

            var agenteTest = new Agente() //The object to test creation
            {
                EquipaId = equipaId,
                Nagente = nagente,
                Pessoa = pessoaDummy

            };

            //Act
            var result = _ageUtils.CreateAgente(agenteTest);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("equipa inválida", result.Item2);

        }



        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(2, 3, 3)]
        [InlineData(3, 1, 1)]
        public void Agente_AgenteUpdateValidEquipa_ShouldUpdateAgente(int agenteId, int equipaId, int expected)
        {
            //Arrange
            var agente = new Agente()
            {
                Id = agenteId,
                EquipaId = equipaId,
            };


            //Act
            var result = _ageUtils.UpdateAgente(agente);


            //Assert
            Assert.Equal(expected, result.Item1.EquipaId);
            Assert.Equal("", result.Item2);
        }


        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 4)]
        [InlineData(2, 5)]
        [InlineData(3, 6)]
        public void Agente_AgenteUpdateInvalidEquipa_ShouldReturnError(int agenteId, int equipaId)
        {
            //Arrange
            var agente = new Agente()
            {
                Id = agenteId,
                EquipaId = equipaId,
            };


            //Act
            var result = _ageUtils.UpdateAgente(agente);


            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Equipa Inválida", result.Item2);
        }


        [Theory]
        [InlineData(0, 1)]
        [InlineData(11, 1)]
        [InlineData(12, 4)]
        [InlineData(18, 4)]
        public void Agente_AgenteUpdateInvalidAgente_ShouldReturnError(int agenteId, int equipaId)
        {
            //Arrange
            var agente = new Agente()
            {
                Id = agenteId,
                EquipaId = equipaId,
            };


            //Act
            var result = _ageUtils.UpdateAgente(agente);


            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please provide a valid object", result.Item2);
        }




        #endregion



    }
}
