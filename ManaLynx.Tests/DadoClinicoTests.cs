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
    public class DadoClinicoTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IDadoClinicoUtils _dcUtils;

        public DadoClinicoTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _dcUtils = new DadoClinicoUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds agentes to the db
            var dadoClinicoList = new List<DadoClinico>
            {
                new DadoClinico{ Id = 1, Altura = 1.78, Peso = 81.2, Tensao = "Normal" },
                new DadoClinico{ Id = 2, Altura = 0, Peso = 0, Tensao = "Normal" }
            };


            context.DadoClinicos.AddRange(dadoClinicoList);
            context.SaveChanges();
        }

        #endregion

        #region Tests
        [Theory]
        [InlineData(1.95, 88.5, "Hipertenso")]
        public void DadoClinico_DadoClinicoCreate_ShouldCreateDadoClinico(double altura, double peso, string tensao)
        {
            //Arrange
            var dadoClinico = new DadoClinico()
            {
                Altura = altura,
                Peso = peso,
                Tensao = tensao
            };


            //Act
            var result = _dcUtils.CreateDadoClinico(dadoClinico);

            //Assert
            Assert.IsType<DadoClinico>(result.Item1);
        }


        [Theory]
        [InlineData(1, 1.95, 88.5, "Hipertenso")]
        [InlineData(2, 1.54, 51.5, "Hipotenso")]
        public void DadoClinico_DadoClinicoUpdateEverything_ShouldUpdateDadoClinico(int id, double altura, double peso, string tensao)
        {
            //Arrange
            var dadoClinico = new DadoClinico()
            {
                Id = id,
                Altura = altura,
                Peso = peso,
                Tensao = tensao
            };


            //Act
            var result = _dcUtils.UpdateDadoClinico(dadoClinico);

            //Assert
            Assert.IsType<DadoClinico>(result.Item1);
            Assert.Equal(id, result.Item1.Id);
        }


        [Theory]
        [InlineData(2, 1.95, 88.5, "Alto")]
        [InlineData(1, 1.45, 48.9, "Baixo")]
        public void DadoClinico_DadoClinicoUpdateEverything_ShouldReturnError(int id, double altura, double peso, string tensao)
        {
            //Arrange
            var dadoClinico = new DadoClinico()
            {
                Id = id,
                Altura = altura,
                Peso = peso,
                Tensao = tensao
            };


            //Act
            var result = _dcUtils.UpdateDadoClinico(dadoClinico);

            //Assert
            Assert.Equal("Incorrect field", result.Item2);
        }
        #endregion
    }
}
