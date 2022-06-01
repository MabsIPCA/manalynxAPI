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
    public class SinistroTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        ISinistroUtils _siUtils;

        public SinistroTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _siUtils = new SinistroUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {
            var sinistroList = new List<Sinistro>
            {
                new Sinistro{ Id = 1, Descricao = "Sinistro 1", Estado = "Reportado", Reembolso = 0.0, DataSinistro = new DateTime(2022, 4, 15), Valido = false, Deferido = false },
                new Sinistro{ Id = 2, Descricao = "Sinistro 2", Estado = "Reportado", Reembolso = 0.0, DataSinistro = new DateTime(2022, 5, 21), Valido = false, Deferido = false }
            };

            var sinistroPessoalList = new List<SinistroPessoal>
            {
                new SinistroPessoal { Id = 1, SinistroId = 1, ApolicePessoalId = 1}
            };

            context.Sinistros.AddRange(sinistroList);
            context.SinistroPessoals.AddRange(sinistroPessoalList);
            context.SaveChanges();
        }

        #endregion

        #region Tests

        [Theory]
        [InlineData("Exemplo de SinistroPessoal", "2022/05/21")]
        [InlineData("", "2022/05/21")]
        public void SinistroPessoal_SinistroCreate_ShouldCreateSinistro(string descricao, DateTime data)
        {
            //Arrange
            var sinistro = new Sinistro()
            {
                Descricao = descricao,
                DataSinistro = data
            };

            var sinistroPessoal = new SinistroPessoal()
            {
                Sinistro = sinistro
            };


            //Act
            var result = _siUtils.CreateSinistroPessoal(sinistroPessoal);

            //Assert
            Assert.IsType<SinistroPessoal>(result.Item1);
            Assert.Equal("", result.Item2);
        }

        [Theory]
        [InlineData("Exemplo de SinistroPessoal", "2022/05/28")]
        [InlineData("", "2022/08/21")]
        public void SinistroPessoal_SinistroCreate_ShouldReturnDateError(string descricao, DateTime data)
        {
            //Arrange
            var sinistro = new Sinistro()
            {
                Descricao = descricao,
                DataSinistro = data
            };

            var sinistroPessoal = new SinistroPessoal()
            {
                Sinistro = sinistro
            };


            //Act
            var result = _siUtils.CreateSinistroPessoal(sinistroPessoal);

            //Assert
            Assert.Equal("Data not accepted", result.Item2);
        }

        [Theory]
        [InlineData(1, "Sinistro 1 alterado", 0, false, false)]
        [InlineData(2, "Sinistro 2 alterado", 0, false, false)]
        public void Sinistro_SinistroUpdate_ShouldCreateSinistro(int id, string descricao, double reembolso, bool valido, bool deferido)
        {
            //Arrange
            var sinistro = new Sinistro()
            {
                Id = id,
                Descricao = descricao,
                Reembolso = reembolso,
                Valido = valido,
                Deferido = deferido
            };


            //Act
            var result = _siUtils.UpdateSinistro(sinistro);

            //Assert
            Assert.IsType<Sinistro>(result.Item1);
            Assert.Equal("", result.Item2);
        }

        [Theory]
        [InlineData(1, "Sinistro 1 alterado - ERRO 1", 155.24, false, false)]
        [InlineData(1, "Sinistro 1 alterado - ERRO 2", 155.24, true, false)]
        [InlineData(1, "Sinistro 1 alterado - ERRO 3", 155.24, false, true)]
        public void Sinistro_SinistroUpdate_ShouldReturnErrorReembolso(int id, string descricao, double reembolso, bool valido, bool deferido)
        {
            //Arrange
            var sinistro = new Sinistro()
            {
                Id = id,
                Descricao = descricao,
                Reembolso = reembolso,
                Valido = valido,
                Deferido = deferido
            };


            //Act
            var result = _siUtils.UpdateSinistro(sinistro);

            //Assert
            Assert.Equal("Reembolso not appliable", result.Item2);
        }

        #endregion
    }
}
