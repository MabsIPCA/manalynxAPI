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
    public class EquipaTests
    {

        #region Context Creation

        private readonly ApplicationDbContext _db;
        private readonly IEquipaUtils _equipaUtils;

        public EquipaTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _equipaUtils = new EquipaUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds equipas to the db
            var equipaList = new List<Equipa>
            {
                new Equipa{ Id = 1, Nome = "Dummy", Regiao = "Norte", GestorId = 1 },
                new Equipa{ Id = 2, Nome = "Dummy", Regiao = "Norte", GestorId = 1 },
                new Equipa{ Id = 3, Nome = "Dummy", Regiao = "Norte", GestorId = 2 },
                new Equipa{ Id = 4, Nome = "Dummy", Regiao = "Norte", GestorId = 2 },
                new Equipa{ Id = 5, Nome = "Dummy", Regiao = "Norte", GestorId = 3 },
                new Equipa{ Id = 6, Nome = "Dummy", Regiao = "Norte", GestorId = 3 },
                new Equipa{ Id = 7, Nome = "Dummy", Regiao = "Norte", GestorId = 4 },
                new Equipa{ Id = 8, Nome = "Dummy", Regiao = "Norte", GestorId = 4 },
                new Equipa{ Id = 9, Nome = "Dummy", Regiao = "Norte", GestorId = 5 },
                new Equipa{ Id = 10, Nome = "Dummy", Regiao = "Norte", GestorId = 5 }
            };

            //Seeds Gestores to the db
            var gestorList = new List<Gestor>
            {
                new Gestor{ Id = 1, AgenteId = 1 },
                new Gestor{ Id = 2, AgenteId = 2 },
                new Gestor{ Id = 3, AgenteId = 3 },
                new Gestor{ Id = 4, AgenteId = 4 },
                new Gestor{ Id = 5, AgenteId = 5 }
            };


            context.Equipas.AddRange(equipaList);
            context.Gestors.AddRange(gestorList);
            context.SaveChanges();
        }
        #endregion

        #region Tests

        [Theory]
        [InlineData("Bikingues", "Ne", 1)]
        [InlineData("Bi", "Nasdorte", 2)]
        [InlineData("Bikinguesasdasdasdasdasdsda", "Nte", 3)]
        [InlineData("B", "Norteasdasdasd", 4)]
        [InlineData("Bikinsssgues", "N", 5)]
        public void Equipa_CreateEquipaValidAttributes_ShouldCreateEquipa(string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.CreateEquipa(equipa);

            //Assert
            Assert.Equal(nome, result.Item1.Nome);
            Assert.Equal(regiao, result.Item1.Regiao);
            Assert.Equal(null, result.Item1.GestorId);
            Assert.Equal("", result.Item2);
        }


        [Theory]
        [InlineData("", "Ne", 1)]
        [InlineData("Bikinguesasdasdasdasdasdsdaasdasdasdasdasdasdasdasd", "Nte", 3)]
        public void Equipa_CreateEquipaInvalidNome_ShouldntCreateEquipa(string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.CreateEquipa(equipa);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please provide a valid nome for the equipa", result.Item2);
        }

        [Theory]
        [InlineData("Bikingues", "", 1)]
        [InlineData("Bikingues", "estapalavraemuitograndeparaestarnonomedeumaregiao", 3)]
        public void Equipa_CreateEquipaInvalidRegiao_ShouldntCreateEquipa(string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.CreateEquipa(equipa);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please provide a valid regiao for the equipa", result.Item2);
        }


        [Theory]
        [InlineData("Bikingues", "Norte", 0)]
        [InlineData("Bikingues", "Norte", 6)]
        [InlineData("Bikingues", "Norte", 12312312)]
        public void Equipa_CreateEquipaInvalidGestor_ShouldCreateEquipa(string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.CreateEquipa(equipa);

            //Assert
            Assert.Equal(nome, result.Item1.Nome);
            Assert.Equal(regiao, result.Item1.Regiao);
            Assert.Equal(null, result.Item1.GestorId);
            Assert.Equal("", result.Item2);
        }


        [Theory]
        [InlineData(1, "Dummy", "Norte", 1)]
        [InlineData(2, "Dummy", "Norte", 2)]
        [InlineData(3, "Dummy", "Norte", 3)]
        public void Equipa_UpdateEquipaValidObject_ShouldUpdateEquipa(int equipaId, string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Id = equipaId,
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.UpdateEquipa(equipa);

            //Assert
            Assert.Equal(nome, result.Item1.Nome);
            Assert.Equal(regiao, result.Item1.Regiao);
            Assert.Equal(gestorId, result.Item1.GestorId);
            Assert.Equal("", result.Item2);
        }



        [Theory]
        [InlineData(0, "Bikingues", "Norte", 1)]
        [InlineData(11, "Bikingues", "Norte", 2)]
        [InlineData(2123, "Bikingues", "Norte", 3)]
        public void Equipa_UpdateEquipaInvalidEquipa_ShouldntUpdateEquipa(int equipaId, string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Id = equipaId,
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.UpdateEquipa(equipa);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please Provide a Valid Equipa", result.Item2);
        }


        [Theory]
        [InlineData(1,"Bikingues", "Norte", 0)]
        [InlineData(2,"Bikingues", "Norte", 6)]
        [InlineData(3, "Bikingues", "Norte", 12312312)]
        public void Equipa_UpdateEquipaInvalidGestor_ShouldntUpdateEquipa(int equipaId, string nome, string regiao, int gestorId)
        {
            //Arrange
            var equipa = new Equipa()
            {
                Id = equipaId,
                Nome = nome,
                Regiao = regiao,
                GestorId = gestorId
            };

            //Act
            var result = _equipaUtils.UpdateEquipa(equipa);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please Provide a valid Gestor", result.Item2);
        }


        #endregion
    }
}
