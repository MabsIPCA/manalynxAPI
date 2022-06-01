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
    public class RelatorioPeritagemTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IRelatorioPeritagemUtils _rpUtils;

        public RelatorioPeritagemTests()
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _rpUtils = new RelatorioPeritagemUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds agentes to the db
            var relatorioList = new List<RelatorioPeritagem>
            {
                new RelatorioPeritagem{ Id = 1, Conteudo = "O processo 1 do sinistro 1 foi concluido", DataRelatorio = new DateTime (2022, 5, 23), Deferido = true, SinistroId = 1 }
            };

            var sinistroList = new List<Sinistro>
            {
                new Sinistro{ Id = 1, Descricao = "Sinistro 1", Estado = "Reportado", Reembolso = 0.0, DataSinistro = new DateTime(2022, 5, 21), Valido = false, Deferido = false }
            };

            context.RelatorioPeritagems.AddRange(relatorioList);
            context.Sinistros.AddRange(sinistroList);
            context.SaveChanges();
        }

        #endregion

        #region Tests
        [Theory]
        [InlineData("Processo 1 do sinistro 1", "2022/05/24", false, 1)]
        [InlineData("Processo 2 do sinistro 1", "2022/05/24", false, 1)]
        public void Relatorio_RelatorioCreate_ShouldCreateRelatorio(string conteudo, DateTime dataRelatorio, bool deferido, int sinistroId)
        {
            //Arrange
            var relatorio = new RelatorioPeritagem()
            {
                Conteudo = conteudo,
                DataRelatorio = dataRelatorio,
                Deferido = deferido,
                SinistroId = sinistroId
            };

            //Act
            var result = _rpUtils.CreateRelatorio(relatorio);

            //Assert
            Assert.IsType<RelatorioPeritagem>(result.Item1);
            Assert.Equal("", result.Item2);
        }

        [Theory]
        [InlineData("Processo 1 do sinistro 2 - erro", "2022/05/22", false, 2)]
        public void Relatorio_RelatorioCreate_ShouldReturnError(string conteudo, DateTime dataRelatorio, bool deferido, int sinistroId)
        {
            //Arrange
            var relatorio = new RelatorioPeritagem()
            {
                Conteudo = conteudo,
                DataRelatorio = dataRelatorio,
                Deferido = deferido,
                SinistroId = sinistroId
            };

            //Act
            var result = _rpUtils.CreateRelatorio(relatorio);

            //Assert
            Assert.Equal("SinistroId not found", result.Item2);
        }

        #endregion
    }
}
