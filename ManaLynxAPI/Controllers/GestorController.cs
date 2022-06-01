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
    public class GestorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IAgenteUtils _ageUtils;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;
        private readonly IGestorUtils _gesUtils;

        public GestorController(ApplicationDbContext db, IAgenteUtils ageUtils, ILoggerUtils logger, IAppUtils app, IGestorUtils gesUtils)
        {
            _db = db;
            _ageUtils = ageUtils;
            _logger = logger;
            _app = app;
            _gesUtils = gesUtils;
        }


        #region Rotas
        //GETALL
        [HttpGet, Auth(Roles.Admin)]
        public IActionResult Index()
        {
            if (_db.Gestors != null)
            {
                var objGestorList = _db.Gestors.Select(c => new { c.Id, c.AgenteId }).ToList();

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoGetAll(_app.GetUserId(token), "Gestor");

                return Ok(objGestorList);

            }
            else return BadRequest(error:"no gestor found");
        }




        //GETBYID
        [HttpGet("{Id}"), Auth(Roles.Admin)]
        public IActionResult ViewById(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return BadRequest(error:"Please provide a valid id");
            }
            var gestorFromDb = _db.Gestors.Find(Id);

            if (gestorFromDb == null)
            {
                return BadRequest(error:"No gestor found");
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "Gestor", Id);

            return Ok(gestorFromDb);
        }


        //CREATE
        [HttpPost("create"), Auth(Roles.Admin)]
        public IActionResult Create(CreateGestor obj)
        {
            //Initializes variables
            var agenteId = obj.agenteId;
            var equipaId = obj.equipaId;
            //Initializes response variables
            var objectUtils = new Gestor();
            var responseUtils = string.Empty;

            if (ModelState.IsValid)
            {
                //Call Utils
                (objectUtils, responseUtils) = _gesUtils.createGestor(agenteId, equipaId);

                if (objectUtils == null) return BadRequest(error: responseUtils);

                //Logs
                var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_app.GetUserId(token), "Gestor", json);


                return Ok(objectUtils);

            }
            return BadRequest(error: "Please provide a valid object");
        }



        //DELETE
        [HttpDelete("delete"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Gestors.Find(Id);
            if (obj == null)
            {
                return BadRequest(error: "No gestor found with that id");
            }
            _db.Gestors.Remove(obj);
            _db.SaveChanges();


            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "Gestor", Id);

            return Ok();
        }


        #endregion


    }

    public class CreateGestor{
        public int agenteId { get; set; }
        public int equipaId { get; set; }
    }
}
