using Microsoft.AspNetCore.Authorization;
using Auth = ManaLynxAPI.Authentication.Auth;
using Roles = ManaLynxAPI.Models.Roles;
using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using ManaLynxAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace ManaLynxAPI.Controllers
{

    [Authorize]
    [ApiController, Route("[controller]")]
    public class CategoriaVeiculoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;

        public CategoriaVeiculoController(ApplicationDbContext db, ILoggerUtils logger, IAppUtils app)
        {
            _db = db;
            _logger = logger;
            _app = app;
        }

        /// <summary>
        /// CategoriaVeiculo index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content is equal to every role
        /// </summary>
        /// <returns>CategoriaVeiculo List</returns>
        [HttpGet, Auth(Roles.Admin,Roles.Agente,Roles.Gestor,Roles.Cliente)]
        public IActionResult Index()
        {

            if (_db.CategoriaVeiculos != null)
            {
                var objCategoriaVeiculo = (from cVeiculo in _db.CategoriaVeiculos
                                           select new { cVeiculo.Id, cVeiculo.Categoria }).ToList();

                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoGetAll(_app.GetUserId(token), "CategoriaVeiculo");
                return Ok(objCategoriaVeiculo);
            }
            else return NotFound();
        }


        /// <summary>
        /// CategoriaVeiculo Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// </summary>
        /// <param name="Id">CategoriaVeiculoId to get</param>
        /// <returns>CategoriaVeiculo List, size one or zero</returns>
        [HttpGet("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult ViewById(int? Id)
        {

            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var categoriaVeiculo = (from cVeiculo in _db.CategoriaVeiculos
                                    where cVeiculo.Id == Id
                                    select new { cVeiculo.Id, cVeiculo.Categoria }).ToList().FirstOrDefault();

            if (categoriaVeiculo == null)
            {
                return NotFound();
            }

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoGet(_app.GetUserId(token), "CategoriaVeiculo", Id);
            return Ok(categoriaVeiculo);
        }


        /// <summary>
        /// CategoriaVeiculo Create Route
        /// This route can only be accessed by Admin users.
        /// </summary>
        /// <param name="obj">CategoriaVeiculo Object</param>
        /// <returns>If sucessfull return created CategoriaVeiculoId</returns>
        [HttpPost, Auth(Roles.Admin)]
        public IActionResult Create(CategoriaVeiculo obj)
        {

            if (ModelState.IsValid)
            {
                var createObj = new CategoriaVeiculo();

                //Assigns variables to the createObj
                createObj.Categoria = obj.Categoria;

                _db.CategoriaVeiculos.Add(createObj);
                _db.SaveChanges();

                var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });


                var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                _logger.SetLogInfoPost(_app.GetUserId(token), "CategoriaVeiculo", json);

                return Ok(createObj);
            }
            return BadRequest(obj);
        }

        /// <summary>
        /// CategoriaVeiculo Update Route
        /// This route can only be accessed by Admin users.
        /// </summary>
        /// <param name="Id">CategoriaVeiculo Id to Update</param>
        /// <param name="obj">CategoriaVeiculo Object</param>
        /// <returns>Updated CategoriaVeiculo if update successful</returns>
        [HttpPut("{Id}"), Auth(Roles.Admin)]
        public IActionResult Edit(int Id, CategoriaVeiculo obj)
        {

            if (ModelState.IsValid)
            {
                var updateObj = _db.CategoriaVeiculos.Find(Id);

                if (updateObj != null)
                {
                    //Assigns variables to the updateObj
                    if (obj.Categoria != null) updateObj.Categoria = obj.Categoria;

                    //Updates Seguros with the data given
                    _db.CategoriaVeiculos.Update(updateObj);
                    _db.SaveChanges();

                    var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

                    var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
                    _logger.SetLogInfoPut(_app.GetUserId(token), "CategoriaVeiculo", json);

                    return Ok(updateObj);
                }
                else return NotFound(obj);

            }
            return Ok(obj);
        }

        /// <summary>
        /// CategoriaVeiculo Delete Route
        /// This route can only be accessed by Admin users.
        /// </summary>
        /// <param name="Id">CategoriaVeiculoId to delete</param>
        /// <returns>Ok if successful</returns>
        [HttpDelete("{Id}"), Auth(Roles.Admin)]
        public IActionResult Delete(int Id)
        {
                var obj = _db.CategoriaVeiculos.Find(Id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.CategoriaVeiculos.Remove(obj);
            _db.SaveChanges();

            var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
            _logger.SetLogInfoDelete(_app.GetUserId(token), "CategoriaVeiculo", Id);
            return Ok();
        }
    }
}
