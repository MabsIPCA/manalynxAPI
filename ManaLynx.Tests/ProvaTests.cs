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
    public class ProvaTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IProvaUtils _prUtils;

        public ProvaTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _prUtils = new ProvaUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds agentes to the db
            var provaList = new List<Prova>
            {
                new Prova{ Id = 1, Conteudo = "Conteudo de prova para o id 1", DataSubmissao = new DateTime(2022, 5, 21), SinistroId = 1 },
                new Prova{ Id = 2, Conteudo = "Conteudo de prova para o id 2", DataSubmissao = new DateTime(2022, 5, 21), SinistroId = 1 },
            };

            var sinistroList = new List<Sinistro>
            {
                new Sinistro{ Id = 1, Descricao = "Sinistro 1", Estado = "Reportado", Reembolso = 0.0, DataSinistro = new DateTime(2022, 5, 21), Valido = false, Deferido = false }
            };

            context.Provas.AddRange(provaList);
            context.Sinistros.AddRange(sinistroList);
            context.SaveChanges();
        }

#endregion

        #region Tests
        [Theory]
        [InlineData("Conteudo de prova para o id 3", "2022/05/21", 1)]
        [InlineData("Conteudo de prova para o id 4", "2022/05/22", 1)]
        public void Prova_ProvaCreate_ShouldCreateProva(string conteudo, DateTime data, int sinistroId)
        {
            //Arrange
            var prova = new Prova()
            {
                Conteudo = conteudo,
                DataSubmissao = data,
                SinistroId = sinistroId
            };


            //Act
            var result = _prUtils.CreateProva(prova);

            //Assert
            Assert.IsType<Prova>(result.Item1);
            Assert.Equal("", result.Item2);
        }

        [Theory]
        [InlineData("Conteudo de prova erro", "2022/05/22", 2)]
        public void Prova_ProvaCreate_ShouldReturnError(string conteudo, DateTime data, int sinistroId)
        {
            //Arrange
            var prova = new Prova()
            {
                Conteudo = conteudo,
                DataSubmissao = data,
                SinistroId = sinistroId
            };


            //Act
            var result = _prUtils.CreateProva(prova);

            //Assert
            Assert.Equal("SinistroId not found", result.Item2);
        }

        #endregion
    }
}
