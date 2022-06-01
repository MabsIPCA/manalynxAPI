using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
using ManaLynxAPI;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{
    /// <summary>
    /// Controller for the Cobertura table from the database
    /// </summary>
    [Authorize]
    [ApiController, Route("[controller]/[action]")]
    public class CoberturaController : Controller
    {

        private readonly ApplicationDbContext _db;
        private ICoberturaUtils _coberUtils;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        public CoberturaController(ApplicationDbContext db, ICoberturaUtils coberUtils, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _coberUtils = coberUtils;
            _logger = logger;
            _app = app;
        }



        /// <summary>
        /// Gets all Coberturas from DB
        /// Everyone can access this route
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {

            if (_db.Coberturas != null)
            {
                var objCoberturaList = _db.Coberturas.Select(c => new { c.Id, c.DescricaoCobertura, c.SeguroId}).ToList();

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoGetAll(_app.GetUserId(token), "Cobertura");

                return Ok(objCoberturaList);
            }
            else return BadRequest(error:"No coberturas found");


        }


        /// <summary>
        /// Gets a cobertura with a specific ID from DB
        /// Everyone can access this route
        /// If an invalid Id is given returns badrequest()
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult ViewById(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return BadRequest(error: "Please provide a valid id");
            }
            var coberturaFromDB = _db.Coberturas.Find(Id);

            if (coberturaFromDB == null)
            {
                return BadRequest(error:"No coberturas found");
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "Cobertura", Id);

            return Ok(coberturaFromDB);
        }


        /// <summary>
        /// Creates a Cobertura for the DB
        /// Only Admin can access this route
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("create"), Auth(Roles.Admin)]
        public IActionResult Create(Cobertura obj)
        {
            //Initializes response variables
            var objectUtils = new Cobertura();
            var responseUtils = string.Empty;

            if (ModelState.IsValid) 
            {
                //Call utils function
                (objectUtils, responseUtils) = _coberUtils.CreateCobertura(obj);
                if (objectUtils == null) return BadRequest(error: responseUtils);

                //Logs
                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_app.GetUserId(token), "Cobertura", json);

                return Ok(objectUtils);

            }
            return BadRequest(error: "Please provide a valid object");
        }


        /// <summary>
        /// Updates a Cobertura from the DB
        /// Only Admin can access this route
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPut("update"), Auth(Roles.Admin)]
        public IActionResult Edit(Cobertura obj)
        {
            //Initializes response variables
            var objectUtils = new Cobertura();
            var responseUtils = string.Empty;

            if (ModelState.IsValid)
            {
                //Call utils function
                (objectUtils, responseUtils) = _coberUtils.CreateCobertura(obj);
                if (objectUtils == null) return BadRequest(error: responseUtils);

                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPut(_app.GetUserId(token), "Cobertura", json);

            }
            return BadRequest(error:"please provide a valid obj");

        }

        /// <summary>
        /// Deletes a Cobertura from the DB
        /// Only admin can access this route
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete("delete"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Coberturas.Find(Id);
            if (obj == null)
            {
                return BadRequest(error:"No cobertura found with that id");
            }
            _db.Coberturas.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Cobertura", Id);

            return Ok();
        }
    }
}
