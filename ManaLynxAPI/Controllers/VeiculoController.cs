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

    [Authorize]
    [ApiController, Route("[controller]")]
    public class VeiculoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggerUtils _logger;
        private readonly IAppUtils _app;
        private readonly IVeiculoUtils _vei;

        public VeiculoController(ApplicationDbContext db, ILoggerUtils logger, IAppUtils app, IVeiculoUtils vei)
        {
            _db = db;
            _logger = logger;
            _app = app;
            _vei = vei;
        }

        /// <summary>
        /// Veiculo index Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see all the data in the database.
        /// Gestores can only see Veiculos from Clientes managed by his Agentes
        /// Agentes can only see Veiculos from Clientes managed by himselves
        /// Cliente can only see his Veiculos
        /// </summary>
        /// <returns>Veiculos List, possibly empty</returns>
        [HttpGet, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Index()
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid user");

            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.Veiculos != null)
                    {
                        var objVeiculos = (from veiculo in _db.Veiculos
                                           select new
                                           {
                                               veiculo.Id,
                                               veiculo.Vin,
                                               veiculo.Matricula,
                                               veiculo.Ano,
                                               veiculo.Mes,
                                               veiculo.Marca,
                                               veiculo.Modelo,
                                               veiculo.Cilindrada,
                                               veiculo.Portas,
                                               veiculo.Lugares,
                                               veiculo.Potencia,
                                               veiculo.Peso,
                                               veiculo.CategoriaVeiculo,
                                               veiculo.ClienteId,
                                               veiculo.Cliente.Pessoa
                                           }).ToList();

                        _logger.SetLogInfoGetAll(_app.GetUserId(bearer), "Veiculo");
                        return Ok(objVeiculos);
                    }
                    else return NotFound();
                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Inalid gestor");

                    var objVeiculosGestor = (from veiculo in _db.Veiculos
                                             join cliente in _db.Clientes on veiculo.ClienteId equals cliente.Id
                                             join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId
                                             select new
                                             {
                                                 veiculo.Id,
                                                 veiculo.Vin,
                                                 veiculo.Matricula,
                                                 veiculo.Ano,
                                                 veiculo.Mes,
                                                 veiculo.Marca,
                                                 veiculo.Modelo,
                                                 veiculo.Cilindrada,
                                                 veiculo.Portas,
                                                 veiculo.Lugares,
                                                 veiculo.Potencia,
                                                 veiculo.Peso,
                                                 veiculo.CategoriaVeiculo,
                                                 veiculo.ClienteId,
                                                 veiculo.Cliente.Pessoa
                                             }).ToList();

                    _logger.SetLogInfoGetAll(_app.GetUserId(bearer), "Veiculo");
                    return Ok(objVeiculosGestor);
                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var objVeiculosAgente = (from veiculo in _db.Veiculos
                                             join cliente in _db.Clientes on veiculo.ClienteId equals cliente.Id
                                             where cliente.AgenteId == agenteId
                                             select new
                                             {
                                                 veiculo.Id,
                                                 veiculo.Vin,
                                                 veiculo.Matricula,
                                                 veiculo.Ano,
                                                 veiculo.Mes,
                                                 veiculo.Marca,
                                                 veiculo.Modelo,
                                                 veiculo.Cilindrada,
                                                 veiculo.Portas,
                                                 veiculo.Lugares,
                                                 veiculo.Potencia,
                                                 veiculo.Peso,
                                                 veiculo.CategoriaVeiculo,
                                                 veiculo.ClienteId,
                                                 veiculo.Cliente.Pessoa
                                             }).ToList();

                    _logger.SetLogInfoGetAll(_app.GetUserId(bearer), "Veiculo");
                    return Ok(objVeiculosAgente);
                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var objVeiculosCliente = (from veiculo in _db.Veiculos
                                              where veiculo.ClienteId == clienteId
                                              select new
                                              {
                                                  veiculo.Id,
                                                  veiculo.Vin,
                                                  veiculo.Matricula,
                                                  veiculo.Ano,
                                                  veiculo.Mes,
                                                  veiculo.Marca,
                                                  veiculo.Modelo,
                                                  veiculo.Cilindrada,
                                                  veiculo.Portas,
                                                  veiculo.Lugares,
                                                  veiculo.Potencia,
                                                  veiculo.Peso,
                                                  veiculo.CategoriaVeiculo,
                                                  veiculo.ClienteId,
                                                  veiculo.Cliente.Pessoa
                                              }).ToList();

                    _logger.SetLogInfoGetAll(_app.GetUserId(bearer), "Veiculo");
                    return Ok(objVeiculosCliente.ToArray());
                default: return NotFound();
            }
        }


        /// <summary>
        /// Veiculo Get by Id Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The returned content depends on the role of the user and his permissions to view.
        /// Admins can see any Veiculo.
        /// Gestores can only see the Veiculo if it Cliente is managed by his Agentes
        /// Agentes can only see the Veiculo if it Cliente is managed by himselves
        /// Cliente can only see the Veiculo if it is his own.
        /// </summary>
        /// <param name="Id">VeiculoId to get</param>
        /// <returns>Veiculos List, size one or zero</returns>
        [HttpGet("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult ViewById(int? Id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    if (_db.Veiculos != null)
                    {
                        var objVeiculos = (from veiculo in _db.Veiculos
                                           where veiculo.Id == Id
                                           select new
                                           {
                                               veiculo.Id,
                                               veiculo.Vin,
                                               veiculo.Matricula,
                                               veiculo.Ano,
                                               veiculo.Mes,
                                               veiculo.Marca,
                                               veiculo.Modelo,
                                               veiculo.Cilindrada,
                                               veiculo.Portas,
                                               veiculo.Lugares,
                                               veiculo.Potencia,
                                               veiculo.Peso,
                                               veiculo.CategoriaVeiculo,
                                               veiculo.ClienteId,
                                               veiculo.Cliente.Pessoa
                                           }).ToList();

                        _logger.SetLogInfoGet(_app.GetUserId(bearer), "Veiculo", Id);
                        return Ok(objVeiculos);
                    }
                    else return NotFound();
                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var objVeiculosGestor = (from veiculo in _db.Veiculos
                                             join cliente in _db.Clientes on veiculo.ClienteId equals cliente.Id
                                             join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                             where agente.EquipaId == equipaId && veiculo.Id == Id
                                             select new
                                             {
                                                 veiculo.Id,
                                                 veiculo.Vin,
                                                 veiculo.Matricula,
                                                 veiculo.Ano,
                                                 veiculo.Mes,
                                                 veiculo.Marca,
                                                 veiculo.Modelo,
                                                 veiculo.Cilindrada,
                                                 veiculo.Portas,
                                                 veiculo.Lugares,
                                                 veiculo.Potencia,
                                                 veiculo.Peso,
                                                 veiculo.CategoriaVeiculo,
                                                 veiculo.ClienteId,
                                                 veiculo.Cliente.Pessoa
                                             }).ToList();

                    _logger.SetLogInfoGet(_app.GetUserId(bearer), "Veiculo", Id);
                    return Ok(objVeiculosGestor);
                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var objVeiculosAgente = (from veiculo in _db.Veiculos
                                             join cliente in _db.Clientes on veiculo.ClienteId equals cliente.Id
                                             where cliente.AgenteId == agenteId && veiculo.Id == Id
                                             select new
                                             {
                                                 veiculo.Id,
                                                 veiculo.Vin,
                                                 veiculo.Matricula,
                                                 veiculo.Ano,
                                                 veiculo.Mes,
                                                 veiculo.Marca,
                                                 veiculo.Modelo,
                                                 veiculo.Cilindrada,
                                                 veiculo.Portas,
                                                 veiculo.Lugares,
                                                 veiculo.Potencia,
                                                 veiculo.Peso,
                                                 veiculo.CategoriaVeiculo,
                                                 veiculo.ClienteId,
                                                 veiculo.Cliente.Pessoa
                                             }).ToList();

                    _logger.SetLogInfoGet(_app.GetUserId(bearer), "Veiculo", Id);
                    return Ok(objVeiculosAgente);
                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var objVeiculosCliente = (from veiculo in _db.Veiculos
                                              where veiculo.ClienteId == clienteId && veiculo.Id == Id
                                              select new
                                              {
                                                  veiculo.Id,
                                                  veiculo.Vin,
                                                  veiculo.Matricula,
                                                  veiculo.Ano,
                                                  veiculo.Mes,
                                                  veiculo.Marca,
                                                  veiculo.Modelo,
                                                  veiculo.Cilindrada,
                                                  veiculo.Portas,
                                                  veiculo.Lugares,
                                                  veiculo.Potencia,
                                                  veiculo.Peso,
                                                  veiculo.CategoriaVeiculo,
                                                  veiculo.ClienteId,
                                                  veiculo.Cliente.Pessoa
                                              }).ToList();

                    _logger.SetLogInfoGet(_app.GetUserId(bearer), "Veiculo", Id);
                    return Ok(objVeiculosCliente);
                default: return NotFound();
            }
        }


        /// <summary>
        /// Veiculo Create Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The creation depends on the role of the user and his permissions to create.
        /// Admins can create any Veiculo.
        /// Gestores can only create the Veiculo if it Cliente is managed by his Agentes
        /// Agentes can only create the Veiculo if it Cliente is managed by himselves
        /// Cliente can only create the Veiculo if it is his own.
        /// </summary>
        /// <param name="obj">Veiculo Object</param>
        /// <returns>If sucessfull return created VeiculoId</returns>
        [HttpPost, Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Create(Veiculo obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            if (!ModelState.IsValid) BadRequest(error: "Invalid object");

            var createObj = new Veiculo();
            string errorString;

            switch (userRole)
            {
                case Roles.Admin:
                    (errorString, createObj) = _vei.createVeiculo(obj);
                    if (createObj == null) return BadRequest(error: errorString);

                    var json = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    _logger.SetLogInfoPost(userId, "Veiculo", json);

                    return Ok(createObj.Id);
                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    // get agenteId if the ClienteId is managed by the claimed agente and he is member of gestor Equipa
                    var agenteValidGestor = (from cliente in _db.Clientes
                                             join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                             where cliente.Id == obj.ClienteId && agente.EquipaId == equipaId
                                             select cliente.AgenteId).ToList().FirstOrDefault();

                    if (agenteValidGestor != 0)
                    {
                        (errorString, createObj) = _vei.createVeiculo(obj);
                        if (createObj == null) return BadRequest(error: errorString);


                        var jsonG = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(userId, "Veiculo", jsonG);

                        return Ok(createObj.Id);

                    }
                    return BadRequest("Permission Denied");
                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    // get agenteId if the ClienteId is managed by the claimed agente
                    var agenteValidAgente = (from cliente in _db.Clientes
                                             where cliente.Id == obj.ClienteId && cliente.AgenteId == agenteId
                                             select cliente.AgenteId).ToList().FirstOrDefault();

                    if (agenteValidAgente != 0)
                    {

                        (errorString, createObj) = _vei.createVeiculo(obj);
                        if (createObj == null) return BadRequest(error: errorString);

                        var jsonA = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(userId, "Veiculo", jsonA);

                        return Ok(createObj);
                    }
                    return BadRequest("Permission Denied");
                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    if (clienteId == obj.ClienteId)
                    {
                        (errorString, createObj) = _vei.createVeiculo(obj);
                        if (createObj == null) return BadRequest(error: errorString);

                        var jsonC = JsonConvert.SerializeObject(createObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPost(userId, "Veiculo", jsonC);
                        return Ok(createObj);
                    }
                    return BadRequest("Permission Denied");
                default: return BadRequest();
            }
        }

        /// <summary>
        /// Veiculo Update Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The update depends on the role of the user and his permissions to update.
        /// Admins can update any Veiculo.
        /// Gestores can only update the Veiculo if it Cliente is managed by his Agentes
        /// Agentes can only update the Veiculo if it Cliente is managed by himselves
        /// Cliente can only update the Veiculo if it is his own.
        /// </summary>
        /// <param name="Id">Veiculo Id to Update</param>
        /// <param name="obj">Veiculo Object</param>
        /// <returns>Updated VeiculoId if update is successful</returns>
        [HttpPut("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Edit(int Id, Veiculo obj)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            string errorString;
            var updateObj = _db.Veiculos.Find(Id);

            switch (userRole)
            {
                case Roles.Admin:
                    if (updateObj != null)
                    {
                        (errorString, updateObj) = _vei.updateVeiculo(updateObj, obj);
                        if (updateObj == null) return BadRequest(error: errorString);

                        var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        _logger.SetLogInfoPut(_app.GetUserId(bearer), "Veiculo", json);

                        return Ok(updateObj);
                    }
                    else return NotFound();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    // get agenteId if the ClienteId is managed by the claimed agente
                    var agenteValidGestor = (from cliente in _db.Clientes
                                             join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                             where cliente.Id == obj.ClienteId && agente.EquipaId == equipaId
                                             select cliente.AgenteId).ToList().FirstOrDefault();

                    if (agenteValidGestor != 0)
                    {

                        if (updateObj != null)
                        {
                            (errorString, updateObj) = _vei.updateVeiculo(updateObj, obj);
                            if (updateObj == null) return BadRequest(error: errorString);

                            var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_app.GetUserId(bearer), "Veiculo", json);

                            return Ok(updateObj);
                        }
                        else return NotFound();
                    }
                    return BadRequest("Permission Denied");

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    // get agenteId if the ClienteId is managed by the claimed agente
                    var agenteValidAgente = (from cliente in _db.Clientes
                                             where cliente.Id == obj.ClienteId && cliente.AgenteId == agenteId
                                             select cliente.AgenteId).ToList().FirstOrDefault();

                    if (agenteValidAgente != 0)
                    {
                        if (updateObj != null)
                        {
                            (errorString, updateObj) = _vei.updateVeiculo(updateObj, obj);
                            if (updateObj == null) return BadRequest(error: errorString);

                            var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_app.GetUserId(bearer), "Veiculo", json);

                            return Ok(updateObj);
                        }
                        else return NotFound();
                    }
                    return BadRequest("Permission Denied");

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    if (clienteId == obj.ClienteId)
                    {
                        if (updateObj != null)
                        {
                            (errorString, updateObj) = _vei.updateVeiculo(updateObj, obj);
                            if (updateObj == null) return BadRequest(error: errorString);

                            var json = JsonConvert.SerializeObject(updateObj, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                            _logger.SetLogInfoPut(_app.GetUserId(bearer), "Veiculo", json);

                            return Ok(updateObj);
                        }
                        else return NotFound();
                    }
                    return BadRequest("Permission Denied");
                default: return BadRequest();
            }
        }

        /// <summary>
        /// Veiculo Delete Route
        /// This route can only be accessed by authenticad users, any role can access it.
        /// The delete action permission depends on the role of the user.
        /// Admins can delete any Veiculo.
        /// Gestores can only delete the Veiculo if it Cliente is managed by his Agentes
        /// Agentes can only delete the Veiculo if it Cliente is managed by himselves
        /// Cliente can only delete the Veiculo if it is his own.
        /// </summary>
        /// <param name="Id">VeiculoId to delete</param>
        /// <returns>Ok if successful</returns>
        [HttpDelete("{Id}"), Auth(Roles.Admin, Roles.Gestor, Roles.Agente, Roles.Cliente)]
        public IActionResult Delete(int Id)
        {
            string bearer = Request.Headers.Authorization[0].Replace("Bearer ", "");
            int? userId = _app.GetUserId(bearer);
            var userRole = _app.GetUserRole(bearer);
            if (userId == null || userRole == null) return BadRequest("Invalid User");

            switch (userRole)
            {
                case Roles.Admin:
                    var obj = _db.Veiculos.Find(Id);
                    if (obj == null)
                    {
                        return NotFound();
                    }
                    _db.Veiculos.Remove(obj);
                    _db.SaveChanges();

                    _logger.SetLogInfoDelete(_app.GetUserId(bearer), "", Id);
                    return Ok();

                case Roles.Gestor:
                    int? equipaId = _app.GetEquipaId(userId);
                    if (equipaId == null) return BadRequest("Invalid Gestor");

                    var objGestor = _db.Veiculos.Find(Id);
                    if (objGestor == null)
                    {
                        return NotFound();
                    }

                    // get agenteId if the ClienteId is managed by the claimed agente
                    var agenteValidGestor = (from cliente in _db.Clientes
                                             join agente in _db.Agentes on cliente.AgenteId equals agente.Id
                                             where cliente.Id == objGestor.ClienteId && agente.EquipaId == equipaId
                                             select cliente.AgenteId).ToList().FirstOrDefault();

                    if (agenteValidGestor != 0)
                    {
                        _db.Veiculos.Remove(objGestor);
                        _db.SaveChanges();

                        _logger.SetLogInfoDelete(_app.GetUserId(bearer), "", Id);
                        return Ok();
                    }
                    return BadRequest("Permission Denied");

                case Roles.Agente:
                    int? agenteId = _app.GetAgenteId(userId);
                    if (agenteId == null) return BadRequest("Invalid Agente");

                    var objAgente = _db.Veiculos.Find(Id);
                    if (objAgente == null)
                    {
                        return NotFound();
                    }

                    // get agenteId if the ClienteId is managed by the claimed agente
                    var agenteValidAgente = (from cliente in _db.Clientes
                                             where cliente.Id == objAgente.ClienteId && cliente.AgenteId == agenteId
                                             select cliente.AgenteId).ToList().FirstOrDefault();

                    if (agenteValidAgente != 0)
                    {
                        _db.Veiculos.Remove(objAgente);
                        _db.SaveChanges();

                        _logger.SetLogInfoDelete(_app.GetUserId(bearer), "", Id);
                        return Ok();
                    }
                    return BadRequest("Permission Denied");

                case Roles.Cliente:
                    int? clienteId = _app.GetClienteId(userId);
                    if (clienteId == null) return BadRequest("Invalid Cliente");

                    var objCliente = _db.Veiculos.Find(Id);
                    if (objCliente == null)
                    {
                        return NotFound();
                    }
                    if (clienteId == objCliente.ClienteId)
                    {
                        _db.Veiculos.Remove(objCliente);
                        _db.SaveChanges();

                        _logger.SetLogInfoDelete(_app.GetUserId(bearer), "", Id);
                        return Ok();
                    }
                    return BadRequest("Permission Denied");
                default: return BadRequest();
            }
        }
    }
}
