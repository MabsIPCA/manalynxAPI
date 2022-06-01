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
    public class CoberturaTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        ICoberturaUtils _cobUtils;

        public CoberturaTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _cobUtils = new CoberturaUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds coberturas to the db
            var coberturaList = new List<Cobertura>
            {
                new Cobertura{ Id = 1, SeguroId = 1, DescricaoCobertura = "Foo1" },
                new Cobertura{ Id = 2, SeguroId = 1, DescricaoCobertura = "Foo2" },
                new Cobertura{ Id = 3, SeguroId = 2, DescricaoCobertura = "Foo3" },
                new Cobertura{ Id = 4, SeguroId = 2, DescricaoCobertura = "Foo4" },
                new Cobertura{ Id = 5, SeguroId = 3, DescricaoCobertura = "Foo5" },
                new Cobertura{ Id = 6, SeguroId = 4, DescricaoCobertura = "Foo6" },
                new Cobertura{ Id = 7, SeguroId = 5, DescricaoCobertura = "Foo7" },
                new Cobertura{ Id = 8, SeguroId = 5, DescricaoCobertura = "Foo8" },
                new Cobertura{ Id = 9, SeguroId = 6, DescricaoCobertura = "Foo9" },
                new Cobertura{ Id = 10, SeguroId = 6, DescricaoCobertura = "Foo10" }

            };

            //Seeds seguros to the db
            var seguroList = new List<Seguro>
            {
                new Seguro{ Id = 1, Nome = "dummy", Tipo = "Pessoal", Ativo = true },
                new Seguro{ Id = 2, Nome = "dummy", Tipo = "Veiculo", Ativo = true },
                new Seguro{ Id = 3, Nome = "dummy", Tipo = "Saúde", Ativo = true },
                new Seguro{ Id = 4, Nome = "dummy", Tipo = "Pessoal", Ativo = true },
                new Seguro{ Id = 5, Nome = "dummy", Tipo = "Pessoal", Ativo = true },
                new Seguro{ Id = 6, Nome = "dummy", Tipo = "Pessoal", Ativo = true },

            };


            context.Seguros.AddRange(seguroList);
            context.Coberturas.AddRange(coberturaList);
            context.SaveChanges();
        }

        #endregion


        #region Tests
        [Theory]
        [InlineData("Quebra de Vidros", 1)]
        [InlineData("Que", 2)]
        [InlineData("Quebra de Vidros e Tempestades Naturas", 3)]
        [InlineData("Furos de Pneus", 4)]
        [InlineData("Consultas", 5)]
        [InlineData("2", 6)]
        public void Cobertura_CoberturaCreateValidDescricao_ShouldCreateCobertura(string descricao, int seguroId)
        {
            
            //Arrange
            var cobertura = new Cobertura
            {
                DescricaoCobertura = descricao,
                SeguroId = seguroId
            };

            //Act
            var result = _cobUtils.CreateCobertura(cobertura);

            //Assert
            Assert.Equal(descricao, result.Item1.DescricaoCobertura);
            Assert.Equal(seguroId, result.Item1.SeguroId);
            Assert.Equal("",result.Item2);

        }


        [Theory]
        [InlineData("", 1)]
        [InlineData("Quebra de Vidros e Tempestades Naturais e Trovoada", 3)]
        public void Cobertura_CoberturaCreateInvalidDescricao_ShouldntCreateCobertura(string descricao, int seguroId)
        {

            //Arrange
            var cobertura = new Cobertura
            {
                DescricaoCobertura = descricao,
                SeguroId = seguroId
            };

            //Act
            var result = _cobUtils.CreateCobertura(cobertura);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Invalid Descricao length", result.Item2);

        }

        [Theory]
        [InlineData("Quebra de Vidros", 0)]
        [InlineData("Quebra de Vidros", 7)]
        public void Cobertura_CoberturaCreateInvalidSeguro_ShouldntCreateCobertura(string descricao, int seguroId)
        {
            //Arrange
            var cobertura = new Cobertura
            {
                DescricaoCobertura = descricao,
                SeguroId = seguroId
            };

            //Act
            var result = _cobUtils.CreateCobertura(cobertura);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Invalid Seguro", result.Item2);
        }



        #endregion


    }
}
