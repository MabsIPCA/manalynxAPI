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
    public class GestorTests
    {
        #region Context Creation

        private readonly ApplicationDbContext _db;
        private readonly IGestorUtils _gestorUtils;

        public GestorTests()
        {

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _gestorUtils = new GestorUtils(_db);

            //Populate db
            Seed(_db);

        }

        private void Seed(ApplicationDbContext context)
        {

            //Seeds pessoas to the db
            var pessoaList = new List<Pessoa>
            {
                new Pessoa { Id = 1, Nome= "Jaoao"},
                new Pessoa { Id = 2, Nome= "Geremias"},
                new Pessoa { Id = 3, Nome= "Antonio"},
                new Pessoa { Id = 4, Nome= "Filomena"},
                new Pessoa { Id = 5, Nome= "Pedro"},
                new Pessoa { Id = 6, Nome= "Renato"},
                new Pessoa { Id = 7, Nome= "Joana"},
                new Pessoa { Id = 8, Nome= "Maria"},
                new Pessoa { Id = 9, Nome= "Fernando"},
                new Pessoa { Id = 10, Nome= "Pedro"},
                new Pessoa { Id = 11, Nome= "Rebinde"},
            };


            //Seeds users to the db
            var userList = new List<ManaUser>
            {
                new ManaUser { Id = 1, Email = "dummy", UserRole = "Gestor", PessoaId = 1, Username = "dums"},
                new ManaUser { Id = 2, Email = "dummy", UserRole = "Gestor", PessoaId = 2, Username = "dums"},
                new ManaUser { Id = 3, Email = "dummy", UserRole = "Gestor", PessoaId = 3, Username = "dums"},
                new ManaUser { Id = 4, Email = "dummy", UserRole = "Gestor", PessoaId = 4, Username = "dums"},
                new ManaUser { Id = 5, Email = "dummy", UserRole = "Gestor", PessoaId = 5, Username = "dums"},
                new ManaUser { Id = 6, Email = "dummy", UserRole = "Gestor", PessoaId = 6, Username = "dums"},
                new ManaUser { Id = 7, Email = "dummy", UserRole = "Gestor", PessoaId = 7, Username = "dums"},
                new ManaUser { Id = 8, Email = "dummy", UserRole = "Gestor", PessoaId = 8, Username = "dums"},
                new ManaUser { Id = 9, Email = "dummy", UserRole = "Gestor", PessoaId = 9, Username = "dums"},
                new ManaUser { Id = 10, Email = "dummy", UserRole = "Gestor", PessoaId = 10, Username = "dums"},
            };

            //Seeds agentes to the db
            var agenteList = new List<Agente>
            {
                new Agente { Id = 1, Nagente = 123123, EquipaId = 1, PessoaId = 1},  //Gestor
                new Agente { Id = 2, Nagente = 123123, EquipaId = 2, PessoaId = 2},  //Gestor
                new Agente { Id = 3, Nagente = 123123, EquipaId = 3, PessoaId = 3},  //Gestor
                new Agente { Id = 4, Nagente = 123123, EquipaId = 4, PessoaId = 4},  //Gestor
                new Agente { Id = 5, Nagente = 123123, EquipaId = 5, PessoaId = 5},  //Gestor
                new Agente { Id = 6, Nagente = 123123, EquipaId = 1, PessoaId = 6},  //Agente
                new Agente { Id = 7, Nagente = 123123, EquipaId = 2, PessoaId = 7},  //Agente
                new Agente { Id = 8, Nagente = 123123, EquipaId = 3, PessoaId = 8},  //Agente
                new Agente { Id = 9, Nagente = 123123, EquipaId = 4, PessoaId = 9},  //Agente
                new Agente { Id = 10, Nagente = 123123, EquipaId = 5, PessoaId = 10}, //Agente
                new Agente { Id = 11, Nagente = 123123, EquipaId = 5, PessoaId = null}, //Agente with no pessoa
                new Agente { Id = 12, Nagente = 123123, EquipaId = 5, PessoaId = 11} //Agente with pessoa but no user
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


            //Seeds Equipas to the db
            var equipaList = new List<Equipa>
            {
                new Equipa{ Id = 1, Nome = "Bikingues", Regiao = "Norte", GestorId = 1},
                new Equipa{ Id = 2, Nome = "Espartanhos", Regiao = "Norte", GestorId = 2},
                new Equipa{ Id = 3, Nome = "Romanus", Regiao = "Norte", GestorId = 3},
                new Equipa{ Id = 4, Nome = "Caubois", Regiao = "Norte", GestorId = 4},
                new Equipa{ Id = 5, Nome = "Antonios", Regiao = "Norte", GestorId = 5},
                new Equipa{ Id = 6, Nome = "Pedros", Regiao = "Norte", GestorId = 6},
            };

            context.Pessoas.AddRange(pessoaList);
            context.ManaUsers.AddRange(userList);
            context.Equipas.AddRange(equipaList);
            context.Agentes.AddRange(agenteList);
            context.Gestors.AddRange(gestorList);
            context.SaveChanges();
        }
        #endregion

        #region Tests

        [Theory]
        [InlineData(6, 6, "Renato")]
        [InlineData(7, 6, "Joana")]
        [InlineData(8, 6, "Maria")]
        public void Gestor_CreateGestorValid_ShouldCreateGestor(int agenteId, int equipaId, string nome)
        {
            //Arrange

            //Act
            var result = _gestorUtils.createGestor(agenteId, equipaId);

            //Assert
            Assert.Equal(equipaId, result.Item1.Id);
            Assert.Equal(nome, result.Item1.Agente.Pessoa.Nome);
            Assert.Equal("Gestor", result.Item1.Agente.Pessoa.ManaUsers.FirstOrDefault().UserRole);
            Assert.Equal(6, _db.Gestors.Count());

        }

        [Theory]
        [InlineData(0, 6)]
        [InlineData(13, 6)]
        [InlineData(-12, 6)]
        public void Gestor_CreateGestorInvalidAgente_ShouldntCreateGestor(int agenteId, int equipaId)
        {
            //Arrange

            //Act
            var result = _gestorUtils.createGestor(agenteId, equipaId);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please Provide a valid Agente", result.Item2);

        }

        [Theory]
        [InlineData(6, 0)]
        [InlineData(6, 7)]
        [InlineData(6, -12)]
        public void Gestor_CreateGestorInvalidEquipa_ShouldntCreateGestor(int agenteId, int equipaId)
        {
            //Arrange

            //Act
            var result = _gestorUtils.createGestor(agenteId, equipaId);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("Please Provide a valid Equipa", result.Item2);

        }


        [Theory]
        [InlineData(11, 6)]
        public void Gestor_CreateGestorAgenteHasNoPessoa_ShouldntCreateGestor(int agenteId, int equipaId)
        {
            //Arrange

            //Act
            var result = _gestorUtils.createGestor(agenteId, equipaId);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("That agente has no pessoa associated, please fix", result.Item2);

        }

        [Theory]
        [InlineData(12, 6)]
        public void Gestor_CreateGestorPessoaHasNoUser_ShouldntCreateGestor(int agenteId, int equipaId)
        {
            //Arrange

            //Act
            var result = _gestorUtils.createGestor(agenteId, equipaId);

            //Assert
            Assert.Null(result.Item1);
            Assert.Equal("That agente has no user associated please fix", result.Item2);

        }


        #endregion


    }
}
