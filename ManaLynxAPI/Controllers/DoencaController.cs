using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace ManaLynxAPI.Controllers
{
    [ApiController, Route("[controller]")]
    public class DoencaController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppUtils _appUtils;
        private readonly ILoggerUtils _logger;
        private readonly IDoencaUtils _doUtils;

        public DoencaController(ApplicationDbContext db, IAppUtils appUtils, ILoggerUtils logger, IDoencaUtils doenca)
        {
            _db = db;
            _appUtils = appUtils;
            _logger = logger;
            _doUtils = doenca;
        }


        /// <summary>
        /// Doenca index Route
        /// This route can only be accessed by authenticad users, everyone can access it.
        /// </summary>
        /// <returns>Doenca List, possibly empty</returns>
        [HttpGet, Auth]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            if (userId == null) return BadRequest();

            if (_db.Doencas != null)
            {
                var objDoencaList = _db.Doencas.Select(d => new { d.Id, d.NomeDoenca, d.Descricao }).ToList();

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoGetAll(_appUtils.GetUserId(token), "Doenca");
                return Ok(objDoencaList);
            }
            else return NotFound();
        }

        /// <summary>
        /// Doenca ViewById Route
        /// This route can only be accessed by authenticad users, everyone can access it.
        /// </summary>
        /// <param name="Id">id of client</param>
        /// <returns>Doenca List, one or zero</returns>
        [HttpGet("{Id}"), Auth]
        public IActionResult ViewById(int? Id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _appUtils.GetUserId(bearer);
            if (userId == null) return BadRequest();

            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var doencaFromDb = _db.Doencas.Find(Id);

            if (doencaFromDb == null)
            {
                return NotFound();
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_appUtils.GetUserId(token), "Doenca", Id);
            return Ok(doencaFromDb);
        }

        /// <summary>
        /// Doenca create Route
        /// This route can only be accessed by authenticad users, only Admin can access it.
        /// Admins can see post Doenca.
        /// </summary>
        /// <param name="obj">Doenca object</param>
        /// <returns>Send Doenca object.</returns>
        [HttpPost("create"), Auth(Roles.Admin)]
        public IActionResult Create(Doenca obj)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            //Initializes response variables
            var objectUtils = new Doenca();
            var responseUtils = string.Empty;

            //Calls function from utils
            (objectUtils, responseUtils) = _doUtils.CreateDoenca(obj);
            if (objectUtils == null) return BadRequest(error: responseUtils);

            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoPost(_appUtils.GetUserId(token), "Doenca", json);

            return Ok(objectUtils);
        }

        /// <summary>
        /// Doenca edit Route
        /// This route can only be accessed by authenticad users, only Admin can access it.
        /// Admins can update Doenca.
        /// </summary>
        /// <param name="obj">Doenca object</param>
        /// <returns>Updated Doenca if update is successful, if not, return the sent object.</returns>
        [HttpPut("edit"), Auth(Roles.Admin)]
        public IActionResult Edit(Doenca obj)
        {
            if (!ModelState.IsValid) return BadRequest();

            //Initializes response variables
            var objectUtils = new Doenca();
            var responseUtils = string.Empty;

            //Calls function from utils
            (objectUtils, responseUtils) = _doUtils.CreateDoenca(obj);
            if (objectUtils == null) return BadRequest(error: responseUtils);

            var json = JsonConvert.SerializeObject(objectUtils, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoPut(_appUtils.GetUserId(token), "Doenca", json);

            return Ok(objectUtils);

        }

        /// <summary>
        /// Doenca delete Route
        /// This route can only be accessed by authenticad users, only Admin can access it.
        /// Admins can delete Doenca.
        /// </summary>
        /// <param name="obj">Doenca object</param>
        /// <returns>Updated Doenca if update is successful, if not, return the sent object.</returns>
        [HttpDelete("delete"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
            var obj = _db.Doencas.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Doencas.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_appUtils.GetUserId(token), "Doenca", Id);

            return Ok(obj);
        }
    }
}
