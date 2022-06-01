using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using Microsoft.AspNetCore.Mvc;


namespace ManaLynxAPI.Utils
{
    public interface IEquipaUtils
    {
        Tuple<Equipa?, string> CreateEquipa(Equipa obj);
        Tuple<Equipa?, string> UpdateEquipa(Equipa obj);
    }

    public class EquipaUtils : IEquipaUtils
    {
        private ApplicationDbContext _db;

        public EquipaUtils(ApplicationDbContext db)
        {
            _db = db;
        }


        public Tuple<Equipa?, string> CreateEquipa(Equipa obj)
        {
            var createObj = new Equipa();

            //Assigns Values to the Aux Object for creation
            if (obj.Nome.Length > 0 && obj.Nome.Length < 40) createObj.Nome = obj.Nome; else return Tuple.Create<Equipa?, string>(null, "Please provide a valid nome for the equipa");
            if (obj.Regiao.Length > 0 && obj.Regiao.Length < 40) createObj.Regiao = obj.Regiao; else return Tuple.Create<Equipa?, string>(null, "Please provide a valid regiao for the equipa");
            createObj.GestorId = null;

            //Updates Agente with the data given
            _db.Equipas.Add(createObj);
            _db.SaveChanges();
            return Tuple.Create<Equipa?, string>(createObj, "");
        }



        public Tuple<Equipa?, string> UpdateEquipa(Equipa obj)
        {
            var updateObj = _db.Equipas.Find(obj.Id);

            if (updateObj != null)
            {
                //Assigns variables to the updateObj
                if (_db.Gestors.Find(obj.GestorId) != null) updateObj.GestorId = obj.GestorId; else return Tuple.Create<Equipa?, string>(null, "Please Provide a valid Gestor");

                //Updates Agente with the data given
                _db.Equipas.Update(updateObj);
                _db.SaveChanges();

                return Tuple.Create<Equipa?, string>(updateObj, "");
            }
            else return Tuple.Create<Equipa?, string>(null, "Please Provide a Valid Equipa");
        }
    }
}
