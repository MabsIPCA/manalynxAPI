using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IVeiculoUtils
    {
        public Tuple<string, Veiculo?> createVeiculo(Veiculo obj);
        public Tuple<string, Veiculo?> updateVeiculo(Veiculo updateObj, Veiculo obj);
    }

    public class VeiculoUtils : IVeiculoUtils
    {
        private readonly ApplicationDbContext _db;
        public VeiculoUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Create Veiculo from Route
        /// </summary>
        /// <param name="obj">Veiculo OBJ</param>
        /// <returns></returns>
        public Tuple<string, Veiculo?> createVeiculo(Veiculo obj)
        {
            var createObj = new Veiculo();

            //Assigns variables to the createObj
            //Verifies if Clientes exists
            if (_db.Clientes.Find(obj.ClienteId) == null) return Tuple.Create<string, Veiculo?>("Invalid Cliente", null);
            //Verifies if Coberturas exists
            if (_db.CategoriaVeiculos.Find(obj.CategoriaVeiculoId) == null) return Tuple.Create<string, Veiculo?>("Invalid Categoria", null);

            //Verifies string length
            if (obj.Vin.Length > 17 || obj.Vin.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Marca.Length > 40 || obj.Marca.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Matricula.Length > 8 || obj.Matricula.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Modelo.Length > 40 || obj.Modelo.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Marca.Length > 40 || obj.Vin.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);

            createObj.Vin = obj.Vin;
            createObj.Matricula = obj.Matricula;
            createObj.Ano = obj.Ano;
            createObj.Mes = obj.Mes;
            createObj.Marca = obj.Marca;
            createObj.Modelo = obj.Modelo;
            createObj.Cilindrada = obj.Cilindrada;
            createObj.Portas = obj.Portas;
            createObj.Lugares = obj.Lugares;
            createObj.Potencia = obj.Potencia;
            createObj.Peso = obj.Peso;
            createObj.ClienteId = obj.ClienteId;
            createObj.CategoriaVeiculoId = obj.CategoriaVeiculoId;
            _db.Veiculos.Add(createObj);
            _db.SaveChanges();

            return Tuple.Create<string, Veiculo?>("", createObj);
        }
        /// <summary>
        /// Update Veiculo From Route
        /// </summary>
        /// <param name="updateObj">Veiculo from DB</param>
        /// <param name="obj">Veiculo Data to Update</param>
        /// <returns></returns>
        public Tuple<string, Veiculo?> updateVeiculo(Veiculo updateObj, Veiculo obj)
        {

            //Assigns variables to the createObj
            //Verifies if Clientes exists
            if (_db.Clientes.Find(obj.ClienteId) == null) return Tuple.Create<string, Veiculo?>("Invalid Cliente", null);
            //Verifies if Coberturas exists
            if (_db.CategoriaVeiculos.Find(obj.CategoriaVeiculoId) == null) return Tuple.Create<string, Veiculo?>("Invalid Categoria", null);

            //Verifies string length
            if (obj.Vin.Length > 17 || obj.Vin.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Marca.Length > 40 || obj.Marca.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Matricula.Length > 8 || obj.Matricula.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Modelo.Length > 40 || obj.Modelo.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);
            if (obj.Marca.Length > 40 || obj.Vin.Length == 0) return Tuple.Create<string, Veiculo?>("Invalid Field", null);

            updateObj.Vin = obj.Vin;
            updateObj.Matricula = obj.Matricula;
            updateObj.Ano = obj.Ano;
            updateObj.Mes = obj.Mes;
            updateObj.Marca = obj.Marca;
            updateObj.Modelo = obj.Modelo;
            updateObj.Cilindrada = obj.Cilindrada;
            updateObj.Portas = obj.Portas;
            updateObj.Lugares = obj.Lugares;
            updateObj.Potencia = obj.Potencia;
            updateObj.Peso = obj.Peso;
            updateObj.CategoriaVeiculoId = obj.CategoriaVeiculoId;
            _db.Veiculos.Update(updateObj);
            _db.SaveChanges();

            return Tuple.Create<string, Veiculo?>("", updateObj);
        }


    }
}
