using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IDadoClinicoUtils
    {
        Tuple<DadoClinico?, string> CreateDadoClinico(DadoClinico obj);
        Tuple<DadoClinico?, string> UpdateDadoClinico(DadoClinico obj);
    }

    public class DadoClinicoUtils : IDadoClinicoUtils
    {
        private ApplicationDbContext _db;
        public DadoClinicoUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a dadoClinico from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<DadoClinico?, string> CreateDadoClinico(DadoClinico obj)
        {
            var createObj = new DadoClinico();

            if (obj != null)
            {
                //Assigns variables to the updateObj
                createObj.Altura = obj.Altura;
                createObj.Peso = obj.Peso;
                if ((obj.Tensao == "Hipotenso") || (obj.Tensao == "Normal") || (obj.Tensao == "Hipertenso"))
                    createObj.Tensao = obj.Tensao;
                else
                    return Tuple.Create<DadoClinico?, string>(null, "Incorrect field");

                _db.DadoClinicos.Add(createObj);
                _db.SaveChanges();

                return Tuple.Create<DadoClinico?, string>(createObj, "");
            }
            else return Tuple.Create<DadoClinico?, string>(createObj, "Please provide a valid object");
        }

        /// <summary>
        /// Updates a dadoClinico from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<DadoClinico?, string> UpdateDadoClinico(DadoClinico obj)
        {
            var updateObj = _db.DadoClinicos.Find(obj.Id);

            if (updateObj != null)
            {
                //Assigns variables to the updateObj
                if (obj.Altura != null) updateObj.Altura = obj.Altura;
                if (obj.Peso != null) updateObj.Peso = obj.Peso;
                if ((obj.Tensao == "Hipotenso") || (obj.Tensao == "Normal") || (obj.Tensao == "Hipertenso"))
                    updateObj.Tensao = obj.Tensao;
                else
                    return Tuple.Create<DadoClinico?, string>(null, "Incorrect field");

                //Updates DadoClinico with the data given
                _db.DadoClinicos.Update(updateObj);
                _db.SaveChanges();

                return Tuple.Create<DadoClinico?, string>(updateObj, "");
            }
            else return Tuple.Create<DadoClinico?, string>(updateObj, "Please provide a valid object");
        }
    }
}
