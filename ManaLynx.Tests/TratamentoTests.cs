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
    public class TratamentoTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        ITratamentoUtils _trUtils;

        public TratamentoTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _trUtils = new TratamentoUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {
            //Seeds tratamento to the db
            var tratamentoList = new List<Tratamento>
            {
                new Tratamento{ Id = 1, NomeTratamento = "Hemodiálise", Frequencia = "Diario", UltimaToma = new DateTime(2022, 3, 21), DadoClinicoId = 1 },
                new Tratamento{ Id = 2, NomeTratamento = "Quimioterapia", Frequencia = "Mensal", UltimaToma = new DateTime(2022, 5, 21), DadoClinicoId = 2 },
                new Tratamento{ Id = 3, NomeTratamento = "Fisioteratia", Frequencia = "Semanal", UltimaToma = new DateTime(2022, 1, 21), DadoClinicoId = 2 }
            };

            var dadoClinicoList = new List<DadoClinico>
            {
                new DadoClinico{ Id = 1, Altura = 1.78, Peso = 81.2, Tensao = "Normal" },
                new DadoClinico{ Id = 2, Altura = 0, Peso = 0, Tensao = "Normal" }
            };

            context.Tratamentos.AddRange(tratamentoList);
            context.DadoClinicos.AddRange(dadoClinicoList);
            context.SaveChanges();
        }

#endregion

        #region Tests
        [Theory]
        [InlineData("Oxigenoterapia", "Semanal", "2022-05-22", 1)]
        public void Tratamento_Create_ShouldCreateTratamento(string nome, string frequencia, DateTime data, int dadoClinicoId)
        {
            //Arrange
            var tratamento = new Tratamento()
            {
                NomeTratamento = nome,
                Frequencia = frequencia,
                UltimaToma = data,
                DadoClinicoId = dadoClinicoId
            };


            //Act
            var result = _trUtils.CreateTratamento(tratamento);

            //Assert
            Assert.IsType<Tratamento>(result.Item1);
        }


        [Theory]
        [InlineData("Oxigenoterapia", "Semanal", "2022-05-22", 10000)]
        public void Tratamento_Create_ShouldReturnError(string nome, string frequencia, DateTime data, int dadoClinicoId)
        {
            //Arrange
            var tratamento = new Tratamento()
            {
                NomeTratamento = nome,
                Frequencia = frequencia,
                UltimaToma = data,
                DadoClinicoId = dadoClinicoId
            };


            //Act
            var result = _trUtils.CreateTratamento(tratamento);

            //Assert
            Assert.Equal("DadoClinico doesn't exist", result.Item2);
        }
        #endregion
    }
}
