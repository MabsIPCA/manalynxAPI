using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IDoencaUtils
    {
        Tuple<Doenca?, string> CreateDoenca(Doenca obj);
        Tuple<Doenca?, string> UpdateDoenca(Doenca obj);
    }

    public class DoencaUtils : IDoencaUtils
    {
        private ApplicationDbContext _db;
        public DoencaUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a Doenca from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<Doenca?, string> CreateDoenca(Doenca obj)
        {
            var createObj = new Doenca();

            if (obj != null)
            {
                //Assigns variables to the createObj
                if((obj.NomeDoenca == null) || (obj.NomeDoenca == ""))
                {
                    return Tuple.Create<Doenca?, string>(null, "Insert Nome");
                } else {
                    createObj.NomeDoenca = obj.NomeDoenca;
                }
                createObj.Descricao = obj.Descricao;

                //Updates DadoClinico with the data given
                _db.Doencas.Add(createObj);
                _db.SaveChanges();

                return Tuple.Create<Doenca?, string>(createObj, "");
            }
            else return Tuple.Create<Doenca?, string>(createObj, "Please provide a valid object");
        }

        /// <summary>
        /// Updates a Doenca from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<Doenca?, string> UpdateDoenca(Doenca obj)
        {
            var updateObj = _db.Doencas.Find(obj.Id);

            if (updateObj != null)
            {
                if (obj.NomeDoenca != "") updateObj.NomeDoenca = obj.NomeDoenca;
                if (obj.Descricao != "") updateObj.Descricao = obj.Descricao;

                _db.Doencas.Update(updateObj);
                _db.SaveChanges();

                return Tuple.Create<Doenca?, string>(updateObj, "");
            }
            else return Tuple.Create<Doenca?, string>(updateObj, "Please provide a valid object");
        }
    }
}
