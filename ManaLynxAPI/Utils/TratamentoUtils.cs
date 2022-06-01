using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface ITratamentoUtils
    {
        Tuple<Tratamento?, string> CreateTratamento(Tratamento obj);
    }

    public class TratamentoUtils : ITratamentoUtils
    {
        private ApplicationDbContext _db;
        public TratamentoUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a Tratamento from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<Tratamento?, string> CreateTratamento(Tratamento obj)
        {
            var createObj = new Tratamento();

            if (obj != null)
            {

                var dadoClinico = _db.DadoClinicos.Find(obj.DadoClinicoId);

                createObj.NomeTratamento = obj.NomeTratamento;
                createObj.Frequencia = obj.Frequencia;
                createObj.UltimaToma = obj.UltimaToma;
                if (dadoClinico != null)
                    createObj.DadoClinicoId = obj.DadoClinicoId;
                else
                    return Tuple.Create<Tratamento?, string>(obj, "DadoClinico doesn't exist");


                _db.Tratamentos.Add(createObj);
                _db.SaveChanges();

                return Tuple.Create<Tratamento?, string>(createObj, "");
            }
            else return Tuple.Create<Tratamento?, string>(createObj, "Please provide a valid object");
        }
    }
}
