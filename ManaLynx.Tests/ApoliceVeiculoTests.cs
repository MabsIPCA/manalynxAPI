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
    public class ApoliceVeiculoTests
    {
        #region Context Creation
        ApplicationDbContext _db;
        IApoliceUtils _apoliceUtils;

        public ApoliceVeiculoTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _apoliceUtils = new ApoliceUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds seguros to the db
            var segurosList = new List<Seguro>
            {
                new Seguro{ Id = 1, Nome = "Foo1", Ativo= true, Tipo= "Pessoal"  },
                new Seguro{ Id = 2, Nome = "Foo2", Ativo= true, Tipo= "Veiculo"  },
                new Seguro{ Id = 3, Nome = "Foo3", Ativo= true, Tipo= "Saude"  },
                new Seguro{ Id = 4, Nome = "Foo4", Ativo= false, Tipo= "Pessoal"  },

            };

            //Seeds Coberturas to the db
            var cobesturasList = new List<Cobertura>
            {
                new Cobertura{ Id = 1, DescricaoCobertura= "FakeCob1", SeguroId= 1},
                new Cobertura{ Id = 2, DescricaoCobertura= "FakeCob2", SeguroId= 2},
                new Cobertura{ Id = 3, DescricaoCobertura= "FakeCob3", SeguroId= 3},
                new Cobertura{ Id = 4, DescricaoCobertura= "FakeCob4", SeguroId= 4}
            };

            //Seeds Agentes to the db
            var agentesList = new List<Agente>
            {
                new Agente{ Id = 1, Nagente= 100000000, PessoaId= 1, EquipaId=1},
                new Agente{ Id = 2, Nagente= 200000000, PessoaId= 2, EquipaId=1},
                new Agente{ Id = 3, Nagente= 300000000, PessoaId= 3, EquipaId=1},
                new Agente{ Id = 4, Nagente= 400000000, PessoaId= 4, EquipaId=1}
            };

            //Seeds Clientes to the db
            var clientesList = new List<Cliente>
            {
                new Cliente{ Id = 1, Profissao = "Prof1", PessoaId = 5, DadoClinicoId=1, AgenteId= null,IsLead = 0 },
                new Cliente{ Id = 2, Profissao = "Prof2", PessoaId = 6, DadoClinicoId=2, AgenteId= null,IsLead = 0 },
                new Cliente{ Id = 3, Profissao = "Prof3", PessoaId = 7, DadoClinicoId=3, AgenteId= null,IsLead = 0 },
                new Cliente{ Id = 4, Profissao = "Prof4", PessoaId = 8, DadoClinicoId=4, AgenteId= null,IsLead = 0 },
            };

            var veiculoList = new List<Veiculo>
            {
                new Veiculo{ Id = 1, Vin= "01234567891234567", Matricula= "AA-00-AA",ClienteId= 1, Ano=2000, Mes = 12, Marca= "Renault", Modelo= "Clio", Cilindrada= 1150, Portas=5,Lugares=5, Potencia = 75, Peso=950, CategoriaVeiculoId=2 },
                new Veiculo{ Id = 2, Vin= "11234567891234568", Matricula= "BA-01-AA",ClienteId= 2, Ano=2001, Mes = 11, Marca= "Renault", Modelo= "Clio", Cilindrada= 1150, Portas=5,Lugares=5, Potencia = 75, Peso=950, CategoriaVeiculoId=2 },
                new Veiculo{ Id = 3, Vin= "21234567891234569", Matricula= "CA-02-AA",ClienteId= 3, Ano=2002, Mes = 10, Marca= "Renault", Modelo= "Clio", Cilindrada= 1150, Portas=5,Lugares=5, Potencia = 75, Peso=950, CategoriaVeiculoId=2 },
                new Veiculo{ Id = 4, Vin= "31234567891234560", Matricula= "DA-03-AA",ClienteId= 4, Ano=2003, Mes = 9 , Marca= "Renault", Modelo= "Clio", Cilindrada= 1150, Portas=5,Lugares=5, Potencia = 75, Peso=950, CategoriaVeiculoId=2 },
            };

            var apoliceList = new List<Apolice>
            {
                new Apolice{ Id = 1, Ativa = true, Simulacao = "Pagamento Emitido", AgenteId = 1, Premio = 100, Fracionamento= "Mensal", SeguroId = 2 },
                new Apolice{ Id = 2, Ativa = false, Simulacao = "Aprovada", AgenteId = 1, Premio = 100, Fracionamento= "Mensal", SeguroId = 2 }
            };

            var ApoliceVeiculoList = new List<ApoliceVeiculo>
            {
                new ApoliceVeiculo{ Id = 1, AcidentesRecentes = 0, DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0), ApoliceId= 1, VeiculoId = 1},
                new ApoliceVeiculo{ Id = 2, AcidentesRecentes = 0, DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0), ApoliceId= 2, VeiculoId = 2}
            };

            context.Seguros.AddRange(segurosList);
            context.Coberturas.AddRange(cobesturasList);
            context.Agentes.AddRange(agentesList);
            context.Clientes.AddRange(clientesList);
            context.Veiculos.AddRange(veiculoList);
            context.Apolices.AddRange(apoliceList);
            context.ApoliceVeiculos.AddRange(ApoliceVeiculoList);
            context.SaveChanges();
        }

        #endregion


        #region Tests
        [Theory]
        [InlineData(null)] // data is given for the arguments this way
        public void ApoliceVeiculo_ApoliceVeiculoCreateValidValidade_ShouldCreateApoliceVeiculo(string? dataValidade)
        {
            //Arrange
            DateTime? expectedDate = null;
            if (dataValidade != null) expectedDate = DateTime.Parse(dataValidade);

            var createAp = new ApoliceVeiculo();

            var obj = new ApoliceVeiculo();
            var objApolice = new Apolice();
            objApolice.Fracionamento = "Mensal";
            objApolice.Simulacao = "Não Validada";
            objApolice.SeguroId = 2;
            objApolice.Premio = 132.10;
            objApolice.Validade = expectedDate;
            obj.Apolice = objApolice;
            obj.AcidentesRecentes = 0;
            obj.DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0);
            obj.VeiculoId = 1;

            //Act
            var result = _apoliceUtils.CreateApoliceVeiculo(createAp, obj);

            //Assert
            Assert.IsType<ApoliceVeiculo>(result.Item2);
            Assert.Equal("", result.Item1);
            Assert.Equal(expectedDate, result.Item2.Apolice.Validade);
        }

        [Theory]
        [InlineData("2012-05-05")] // data is given for the arguments this way
        [InlineData("2016-06-08")]
        public void ApoliceVeiculo_ApoliceVeiculoCreateValidValidade_ShouldReturnNull(string? dataValidade)
        {
            //Arrange
            DateTime? expectedDate = null;
            if (dataValidade != null) expectedDate = DateTime.Parse(dataValidade);

            var createAp = new ApoliceVeiculo();

            var obj = new ApoliceVeiculo();
            var objApolice = new Apolice();
            objApolice.Fracionamento = "Mensal";
            objApolice.Simulacao = "Não Validada";
            objApolice.SeguroId = 2;
            objApolice.Premio = 132.10;
            objApolice.Validade = expectedDate;
            obj.Apolice = objApolice;
            obj.AcidentesRecentes = 0;
            obj.DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0);
            obj.VeiculoId = 1;

            //Act
            var result = _apoliceUtils.CreateApoliceVeiculo(createAp, obj);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Invalid Field", result.Item1);
        }

        [Theory]
        [InlineData(2)] // data is given for the arguments this way
        public void ApoliceVeiculo_ApoliceVeiculoCreateValidSeguro_ShouldCreateApoliceVeiculo(int seguroId)
        {
            //Arrange

            var createAp = new ApoliceVeiculo();

            var obj = new ApoliceVeiculo();
            var objApolice = new Apolice();
            objApolice.Fracionamento = "Mensal";
            objApolice.Simulacao = "Não Validada";
            objApolice.SeguroId = seguroId;
            objApolice.Premio = 132.10;
            objApolice.Validade = null;
            obj.Apolice = objApolice;
            obj.AcidentesRecentes = 0;
            obj.DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0);
            obj.VeiculoId = 1;

            //Act
            var result = _apoliceUtils.CreateApoliceVeiculo(createAp, obj);

            //Assert
            Assert.IsType<ApoliceVeiculo>(result.Item2);
            Assert.Equal("", result.Item1);
            Assert.Equal(seguroId, result.Item2.Apolice.SeguroId);
        }

        [Theory]
        [InlineData(1)] // data is given for the arguments this way
        [InlineData(3)] // data is given for the arguments this way
        [InlineData(4)] // data is given for the arguments this way
        public void ApoliceVeiculo_ApoliceVeiculoCreateValidSeguro_ShouldReturnNull(int? seguroId)
        {
            //Arrange

            var createAp = new ApoliceVeiculo();

            var obj = new ApoliceVeiculo();
            var objApolice = new Apolice();
            objApolice.Fracionamento = "Mensal";
            objApolice.Simulacao = "Não Validada";
            objApolice.SeguroId = seguroId;
            objApolice.Premio = 132.10;
            objApolice.Validade = null;
            obj.Apolice = objApolice;
            obj.AcidentesRecentes = 0;
            obj.DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0);
            obj.VeiculoId = 1;

            //Act
            var result = _apoliceUtils.CreateApoliceVeiculo(createAp, obj);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Invalid Seguro", result.Item1);
        }

        [Theory]
        [InlineData(new object[] { new int[] {} })] // data is given for the arguments this way
        [InlineData(new object[] { new int[] {2} })]
        public void ApoliceVeiculo_ApoliceVeiculoCreateValidCobertura_ShouldCreateApoliceVeiculo(int[] coberturasId)
        {
            //Arrange
            var createAp = new ApoliceVeiculo();

            var obj = new ApoliceVeiculo();
            var objApolice = new Apolice();
            objApolice.Fracionamento = "Mensal";
            objApolice.Simulacao = "Não Validada";
            objApolice.SeguroId = 2;
            objApolice.Premio = 132.10;
            objApolice.Validade = null;
            foreach (int coberturaId in coberturasId)
            {
                CoberturaHasApolice cob = new CoberturaHasApolice();
                cob.CoberturaId = coberturaId;
                objApolice.CoberturaHasApolices.Add(cob);
            }            
            obj.Apolice = objApolice;
            obj.AcidentesRecentes = 0;
            obj.DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0);
            obj.VeiculoId = 1;

            //Act
            var result = _apoliceUtils.CreateApoliceVeiculo(createAp, obj);

            //Assert
            Assert.IsType<ApoliceVeiculo>(result.Item2);
            Assert.Equal("", result.Item1);
        }

        [Theory]
        [InlineData(new object[] { new int[] { 1 } })] // data is given for the arguments this way
        [InlineData(new object[] { new int[] { 3 } })]
        [InlineData(new object[] { new int[] { 4, 1 } })]
        public void ApoliceVeiculo_ApoliceVeiculoCreateValidCobertura_ShouldReturnNull(int[] coberturasId)
        {
            //Arrange
            var createAp = new ApoliceVeiculo();

            var obj = new ApoliceVeiculo();
            var objApolice = new Apolice();
            objApolice.Fracionamento = "Mensal";
            objApolice.Simulacao = "Não Validada";
            objApolice.SeguroId = 2;
            objApolice.Premio = 132.10;
            objApolice.Validade = null;
            foreach (int coberturaId in coberturasId)
            {
                CoberturaHasApolice cob = new CoberturaHasApolice();
                cob.CoberturaId = coberturaId;
                objApolice.CoberturaHasApolices.Add(cob);
            }
            obj.Apolice = objApolice;
            obj.AcidentesRecentes = 0;
            obj.DataCartaConducao = new DateTime(2008, 3, 1, 7, 0, 0);
            obj.VeiculoId = 1;

            //Act
            var result = _apoliceUtils.CreateApoliceVeiculo(createAp, obj);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Invalid Cobertura", result.Item1);
        }

        [Theory]
        [InlineData(2,2)] // data is given for the arguments this way
        public void ApoliceVeiculo_ApoliceVeiculoUpdateValidAcidentesRecentes_ShouldCreateApoliceVeiculo(int ApoliceVeiculoId, int acidentesR)
        {
            //Arrange
            var updateObj = _db.ApoliceVeiculos.Find(ApoliceVeiculoId);
            var obj = _db.ApoliceVeiculos.Find(ApoliceVeiculoId);
            obj.AcidentesRecentes = acidentesR;


            //Act
            var result = _apoliceUtils.UpdateApoliceVeiculo(updateObj, obj);

            //Assert
            Assert.IsType<ApoliceVeiculo>(result.Item2);
            Assert.Equal("", result.Item1);
            Assert.Equal(acidentesR, result.Item2.AcidentesRecentes);
        }

        [Theory]
        [InlineData(1, 2)] // data is given for the arguments this way
        public void ApoliceVeiculo_ApoliceVeiculoUpdateValidValor_ShouldReturnNull(int ApoliceVeiculoId, int acidentesR)
        {
            //Arrange
            var updateObj = _db.ApoliceVeiculos.Find(ApoliceVeiculoId);
            var obj = _db.ApoliceVeiculos.Find(ApoliceVeiculoId);
            obj.AcidentesRecentes = acidentesR;


            //Act
            var result = _apoliceUtils.UpdateApoliceVeiculo(updateObj, obj);

            //Assert
            Assert.Null(result.Item2);
            Assert.Equal("Permission Denied", result.Item1);
        }
        #endregion


    }
}
