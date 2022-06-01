using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IRelatorioPeritagemUtils
    {
        Tuple<RelatorioPeritagem?, string> CreateRelatorio(RelatorioPeritagem obj);
    }

    public class RelatorioPeritagemUtils : IRelatorioPeritagemUtils
    {
        private ApplicationDbContext _db;
        private ISinistroUtils _sinistroUtils;
        public RelatorioPeritagemUtils(ApplicationDbContext db)
        {
            _db = db;
            _sinistroUtils = new SinistroUtils(db);
        }

        /// <summary>
        /// Creates a relatorio from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<RelatorioPeritagem?, string> CreateRelatorio(RelatorioPeritagem obj)
        {
            var sinistro = _db.Sinistros.Find(obj.SinistroId);
            var createObj = new RelatorioPeritagem();

            if (obj != null)
            { 

                //Assigns variables to the createObj
                createObj.Conteudo = obj.Conteudo;
                createObj.DataRelatorio = obj.DataRelatorio;
                createObj.Deferido = obj.Deferido;
                if (sinistro != null)
                    createObj.SinistroId = obj.SinistroId;
                else
                    return Tuple.Create<RelatorioPeritagem?, string>(null, "SinistroId not found");

                _db.RelatorioPeritagems.Add(createObj);
                _db.SaveChanges();
                _sinistroUtils.SinistroSubmitRelatorio(obj.SinistroId, obj.Deferido);

                return Tuple.Create<RelatorioPeritagem?, string>(createObj, "");
            }
            else return Tuple.Create<RelatorioPeritagem?, string>(createObj, "Please provide a valid object");
        }
    }
}
