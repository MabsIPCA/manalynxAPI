using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using Microsoft.AspNetCore.Mvc;


namespace ManaLynxAPI.Utils
{

    public interface IGestorUtils
    {
        Tuple<Gestor?, string> createGestor(int agenteId, int equipaId);
    }

    public class GestorUtils : IGestorUtils
    {
        private readonly ApplicationDbContext _db;

        public GestorUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Utils called by gestor controller to create a new
        /// Gestor and all its dependencies
        /// </summary>
        /// <param name="agenteId"></param>
        /// <param name="equipaId"></param>
        /// <returns></returns>
        public Tuple<Gestor?, string> createGestor(int agenteId, int equipaId)
        {

            var createObj = new Gestor();

            //Assigns variables to the updateObj
            if (agenteId != null)
            {

                //Verifications
                var agenteUpdate = _db.Agentes.Find(agenteId);
                if (agenteUpdate == null) return Tuple.Create<Gestor?, string>(null, "Please Provide a valid Agente");

                var equipaUpdate = _db.Equipas.Find(equipaId);
                if (equipaUpdate == null) return Tuple.Create<Gestor?, string>(null, "Please Provide a valid Equipa");

                var pessoa = _db.Pessoas.Find(agenteUpdate.PessoaId);
                if (pessoa == null) return Tuple.Create<Gestor?, string>(null, "That agente has no pessoa associated, please fix");

                var userUpdate = _db.ManaUsers.Where(x => x.PessoaId == pessoa.Id).FirstOrDefault();
                if (userUpdate == null) return Tuple.Create<Gestor?, string>(null, "That agente has no user associated please fix");


                //Assigns AgenteID to the Aux Object for creation
                createObj.AgenteId = agenteId;

                //Updates Agente with the data given
                _db.Gestors.Add(createObj);
                _db.SaveChanges();

                //Agente to assign equipa
                agenteUpdate.EquipaId = equipaId;
                _db.Agentes.Update(agenteUpdate);

                //equipa to update gestor
                equipaUpdate.GestorId = createObj.Id;
                _db.Equipas.Update(equipaUpdate);

                //Update user role for agente to gestor
                userUpdate.UserRole = "Gestor";
                _db.ManaUsers.Update(userUpdate);

                _db.SaveChanges();

                return Tuple.Create<Gestor?, string>(createObj, "");

            }
            return Tuple.Create<Gestor?, string>(null, "Please Provide a valid Agente");
        }


    }
}
