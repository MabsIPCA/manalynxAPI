using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface ICoberturaUtils
    {
        bool CoberturaExists(Cobertura cobertura);
        Cobertura? AddCobertura(Cobertura cobertura);
        Tuple<Cobertura?, string> CreateCobertura(Cobertura obj);
    }


    public class CoberturaUtils : ICoberturaUtils
    {

        private ApplicationDbContext _db;

        public CoberturaUtils(ApplicationDbContext db)
        {
            _db = db;
        }


        public bool CoberturaExists(Cobertura cobertura)
        {
            return false;
        }
        
        /// <summary>
        /// Creates a cobertura when called by the route
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<Cobertura?, string> CreateCobertura(Cobertura obj)
        {
            var createObj = new Cobertura();

            if (obj != null)
            {
                //Verifies if seguro exists
                if (_db.Seguros.Find(obj.SeguroId) == null) return Tuple.Create<Cobertura?, string>(null, "Invalid Seguro");

                //Verifies string length
                if (obj.DescricaoCobertura.Length > 40 || obj.DescricaoCobertura.Length == 0) return Tuple.Create<Cobertura?, string>(null, "Invalid Descricao length");

                //Assigns variables to the updateObj
                createObj.SeguroId = obj.SeguroId;
                createObj.DescricaoCobertura = obj.DescricaoCobertura;

                //Updates Agente with the data given
                _db.Coberturas.Add(createObj);
                _db.SaveChanges();

                return Tuple.Create<Cobertura?, string>(createObj, "");
            }
            return Tuple.Create<Cobertura?, string>(null, "Please provide a valid object");
        }

        public Tuple<Cobertura?, string> UpdateCobertura(Cobertura obj)
        {
            var updateObj = _db.Coberturas.Find(obj.Id);

            if (updateObj != null)
            {
                //Verifies if seguro exists
                if (_db.Coberturas.Find(obj.SeguroId) == null) return Tuple.Create<Cobertura?, string>(null,"Invalid Seguro");

                //Assigns variables to the updateObj
                if (obj.SeguroId != 0) updateObj.SeguroId = obj.SeguroId;
                if (obj.DescricaoCobertura.Length == 0 || obj.DescricaoCobertura.Length > 40) return Tuple.Create<Cobertura?, string>(null, "Invalid Descricao length");
                updateObj.DescricaoCobertura = obj.DescricaoCobertura;

                //Updates Agente with the data given
                _db.Coberturas.Update(updateObj);
                _db.SaveChanges();

                return Tuple.Create<Cobertura?, string>(updateObj, "");
            }
            else return Tuple.Create<Cobertura?, string>(null, "please provide a valid update obj");
        }



        public Cobertura? AddCobertura(Cobertura cobertura)
        {
            var createObj = new Cobertura();

            if (cobertura != null)
            {

                //Assigns variables to the updateObj
                createObj.SeguroId = cobertura.SeguroId;
                createObj.DescricaoCobertura = cobertura.DescricaoCobertura;

                //Updates Agente with the data given
                _db.Coberturas.Add(createObj);
                _db.SaveChanges();
                return createObj;
            }
            return null;
        }


    }
}
