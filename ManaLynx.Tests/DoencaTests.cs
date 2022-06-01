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
    public class DoencaTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IDoencaUtils _doUtils;

        public DoencaTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _doUtils = new DoencaUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds agentes to the db
            var doencaList = new List<Doenca>
            {
                new Doenca{ Id = 1, NomeDoenca = "Esclerose", Descricao = "Descricao sobre Esclerose" },
                new Doenca{ Id = 2, NomeDoenca = "Anemia", Descricao = "Descricao sobre Anemia" },
                new Doenca{ Id = 3, NomeDoenca = "Miopia", Descricao = "Descricao sobre Miopia" }
            };


            context.Doencas.AddRange(doencaList);
            context.SaveChanges();
        }

        #endregion

        #region Tests
        [Theory]
        [InlineData("Diostrofia", "Descricao sobre Diostrofia")]
        [InlineData("Pneumonia", "Descricao sobre Pneumonia")]
        public void Doenca_DoencaCreate_ShouldCreateDoenca(string nome, string descricao)
        {
            //Arrange
            var doenca = new Doenca()
            {
                NomeDoenca = nome,
                Descricao = descricao
            };


            //Act
            var result = _doUtils.CreateDoenca(doenca);

            //Assert
            Assert.IsType<Doenca>(result.Item1);
        }

        [Theory]
        [InlineData(6, "", "Descricao sobre Diostrofia")]
        [InlineData(7, null, "Descricao sobre Pneumonia")]
        public void Doenca_DoencaCreate_ShouldReturnError(int id, string nome, string descricao)
        {
            //Arrange
            var doenca = new Doenca()
            {
                Id = id,
                NomeDoenca = nome,
                Descricao = descricao
            };


            //Act
            var result = _doUtils.CreateDoenca(doenca);

            //Assert
            Assert.Equal("Insert Nome", result.Item2);
        }
       

        [Theory]
        [InlineData(1, "Novo", "Descricao sobre Novo")]
        public void Doenca_DoencaUpdate_ShouldUpdateDoenca(int id, string nome, string descricao)
        {
            //Arrange
            var doenca = new Doenca()
            {
                Id = id,
                NomeDoenca = nome,
                Descricao = descricao
            };


            //Act
            var result = _doUtils.UpdateDoenca(doenca);

            //Assert
            Assert.IsType<Doenca>(result.Item1);
            Assert.Equal(id, result.Item1.Id);
        }

        #endregion
    }
}
