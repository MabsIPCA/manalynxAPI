using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using ManaLynxAPI.Utils;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{
    [Authorize]
    [ApiController, Route("[controller]")]
    public class EquipaController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IEquipaUtils _equUtils;

        public EquipaController(ApplicationDbContext db, IEquipaUtils equUtils)
        {
            _db = db;
            _equUtils = equUtils;
        }


        /// <summary>
        /// Shows all equipas present in the db
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Admin)]
        public IActionResult Index()
        {
            var objEquipaList = _db.Equipas.Select(c => new { c.Id, c.Nome, c.Regiao, c.GestorId, elementos = c.Agentes.Count() }).ToList();

            return Ok(objEquipaList);
        }


        /// <summary>
        /// Creates a new Equipa with Gestor Id = null
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost, Auth(Roles.Admin)]
        public IActionResult Create(Equipa obj)
        {
            //Initializes response variables
            var objectUtils = new Equipa();
            var responseUtils = string.Empty;


            if (ModelState.IsValid && obj != null)
            {
                //Calls function from utils
                (objectUtils, responseUtils) = _equUtils.CreateEquipa(obj);

                if (objectUtils == null) return BadRequest(error: responseUtils);

                return Ok(objectUtils);
                
            }
            return BadRequest(error: "Please provide a valid object");
        }


        /// <summary>
        /// Updates the gestor id from a equipa
        /// </summary>
        /// <param name="equipaId"></param>
        /// <param name="gestorId"></param>
        /// <returns></returns>
        [HttpPut, Auth(Roles.Admin)]
        public IActionResult Edit(Equipa obj)
        {
            //Initializes response variables
            var objectUtils = new Equipa();
            var responseUtils = string.Empty;


            if (ModelState.IsValid && obj.Id != null && obj.GestorId != null)
            {
                //Calls function from utils
                (objectUtils, responseUtils) = _equUtils.UpdateEquipa(obj);

                if (objectUtils == null) return BadRequest(error: responseUtils);

                return Ok(objectUtils);

            }
            return BadRequest(error: "Please Provide a valid object");
        }


    }
}
