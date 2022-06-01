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
    /// Controller for the Seguro table from the database
    /// </summary>

    [Authorize]
    [ApiController, Route("[controller]")]
    public class SeguroController : Controller
    {
        
        private readonly ApplicationDbContext _db;
        private ICoberturaUtils _coberturaUtils;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        public SeguroController(ApplicationDbContext db, ICoberturaUtils coberturaUtils, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _coberturaUtils = coberturaUtils;
            _logger = logger;
            _app = app;
        }

        /// <summary>
        /// Shows all seguros from DB
        /// Everyone can access this route
        /// </summary>
        /// <returns></returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {

            if (_db.Seguros != null)
            {
                var objSeguroList = _db.Seguros.Select(c => new { c.Id, c.Nome, c.Tipo,c.Ativo, c.Coberturas }).ToList();

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoGetAll(_app.GetUserId(token), "Seguro");

                return Ok(objSeguroList);
            }
            else return NotFound();

            
        }


        /// <summary>
        /// Gets a specific seguro form DB by ID
        /// Only Admin and Gestor can access this route
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}"), Auth(Roles.Admin, Roles.Gestor)]
        public IActionResult ViewById(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var seguroFromDb = _db.Seguros.Find(Id);

            if (seguroFromDb == null)
            {
                return NotFound();
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "Seguro", Id);

            return Ok(seguroFromDb);
        }


        /// <summary>
        /// Creates a seguro into the DB
        /// Only Admin can access this route
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost("create"), Auth(Roles.Admin)]
        public IActionResult Create(Seguro obj)
        {
            if (ModelState.IsValid)
            {
                var createObj = new Seguro();

                //Verifications
                if (obj.Nome.Length == 0 || obj.Nome.Length > 40) return BadRequest(error: "Insert a valid seguro Nome");
                if (Enum.IsDefined(typeof(Tipo), obj.Tipo) == false) return BadRequest(error: "Insert a valid seguro Tipo");
                
                //Assigns variables to the createObj
                createObj.Nome = obj.Nome;
                createObj.Ativo = obj.Ativo;
                createObj.Tipo = obj.Tipo;


                //Updates Seguros with the data given
                _db.Seguros.Add(createObj);
                _db.SaveChanges();

                //Creates Coberturas for the seguro
                if (obj.Coberturas != null)
                {
                    foreach (var cobertura in obj.Coberturas)
                    {
                        cobertura.SeguroId = createObj.Id;
                        _coberturaUtils.AddCobertura(cobertura);
                    }
                }

                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_app.GetUserId(token), "Seguro", json);

                return Ok(createObj);
            }
            return BadRequest(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPut("edit"), Auth(Roles.Admin)]
        public IActionResult Edit(Seguro obj)
        {
            if (ModelState.IsValid)
            {
                var updateObj = _db.Seguros.Find(obj.Id);

                if (updateObj != null)
                {
                    //Assigns variables to the updateObj
                    updateObj.Ativo = obj.Ativo;

                    //Updates Seguros with the data given
                    _db.Seguros.Update(updateObj);
                    _db.SaveChanges();

                    var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPut(_app.GetUserId(token), "Seguro", json);

                    return Ok(updateObj);
                }
                else return NotFound(obj);

            }
            return View(obj);
        }

        //DELETE
        [HttpDelete("delete"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Seguros.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Seguros.Remove(obj);
            _db.SaveChanges();
            
            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Seguro", Id);

            return Ok();
        }


    }
}
