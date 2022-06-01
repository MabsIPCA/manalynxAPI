using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IProvaUtils
    {
        Tuple<Prova?, string> CreateProva(Prova obj);
    }

    public class ProvaUtils : IProvaUtils
    {
        private ApplicationDbContext _db;
        private ISinistroUtils _sinistroUtils;
        public ProvaUtils(ApplicationDbContext db)
        {
            _db = db;
            _sinistroUtils = new SinistroUtils(db);
        }

        /// <summary>
        /// Creates a Prova from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<Prova?, string> CreateProva(Prova obj)
        {
            var sinistro = _db.Sinistros.Find(obj.SinistroId);
            var createObj = new Prova();

            if (obj != null)
            {
                //Assigns variables to the updateObj
                createObj.Conteudo = obj.Conteudo;
                createObj.DataSubmissao = obj.DataSubmissao;

                if (sinistro != null)
                    createObj.SinistroId = obj.SinistroId;
                else
                    return Tuple.Create<Prova?, string>(null, "SinistroId not found");

                _db.Provas.Add(createObj);
                _db.SaveChanges();
                _sinistroUtils.SinistroToAguardarValidacao(obj.SinistroId);

                return Tuple.Create<Prova?, string>(createObj, "");
            }
            else return Tuple.Create<Prova?, string>(createObj, "Please provide a valid object");
        }
    }
}
