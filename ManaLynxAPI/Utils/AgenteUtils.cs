using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManaLynxAPI.Utils
{

    public interface IAgenteUtils
    {
        bool AgenteExists(Agente agente);
        Agente? AddAgente(Agente agente);
        Tuple<Agente?, string> CreateAgente(Agente obj);
        Tuple<Agente?, string> UpdateAgente(Agente obj);
    }



    public class AgenteUtils : IAgenteUtils
    {
        private ApplicationDbContext _db;
        private IPessoaUtils _pesUtils;

        public AgenteUtils(ApplicationDbContext db, IPessoaUtils pesUtils)
        {
            _db = db;
            _pesUtils = pesUtils;
        }

        public bool AgenteExists(Agente agente)
        {
            return false;
        }

        /// <summary>
        /// Creates an agente from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Tuple<Agente?, string> CreateAgente(Agente obj){


            var createObj = new Agente();

            if (obj.Pessoa != null)
            {
                Pessoa? pessoaObj;
                if (_pesUtils.AddPessoa(obj.Pessoa))
                    pessoaObj = _pesUtils.Model;
                else
                    return Tuple.Create<Agente?, string>(null, _pesUtils.Error);

                if (pessoaObj != null)
                {

                    //Verify if equipa Exists
                    if (_db.Equipas.Find(obj.EquipaId) == null) return Tuple.Create<Agente?, string>(null, "equipa inválida");

                    //Assigns variables to the updateObj
                    createObj.EquipaId = obj.EquipaId;
                    createObj.PessoaId = pessoaObj.Id;
                    createObj.Nagente = obj.Nagente;

                    //Updates Agente with the data given
                    _db.Agentes.Add(createObj);
                    _db.SaveChanges();

                    return Tuple.Create<Agente?, string>(createObj, "");
                }
            } return Tuple.Create<Agente?, string>(null, "Invalid Agente");
        }



        /// <summary>
        /// Updates an agente from route
        /// that calls this function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public Tuple <Agente?, string> UpdateAgente(Agente obj)
        {
            var updateObj = _db.Agentes.Find(obj.Id);

            if (updateObj != null)
            {

                //Verify if equipa Exists
                if (_db.Equipas.Find(obj.EquipaId) == null) return Tuple.Create<Agente?, string>(null, "Equipa Inválida");

                //Assigns variables to the updateObj
                if (obj.EquipaId != 0) updateObj.EquipaId = obj.EquipaId;

                //Updates Agente with the data given
                _db.Agentes.Update(updateObj);
                _db.SaveChanges();

                return Tuple.Create<Agente?, string>(updateObj, "");
            }
            else return Tuple.Create<Agente?, string>(updateObj, "Please provide a valid object");
        }



        public Agente? AddAgente(Agente agente)
        {
            var createObj = new Agente();

            if (agente != null)
            {
                if (agente.Pessoa != null)
                {
                    Pessoa? pessoaObj;
                    if (_pesUtils.AddPessoa(agente.Pessoa))
                        pessoaObj = _pesUtils.Model;
                    else
                        return null;
                    if (pessoaObj != null)
                    {
                        //Assigns variables to the updateObj
                        createObj.EquipaId = agente.EquipaId;
                        createObj.PessoaId = pessoaObj.Id;
                        createObj.Nagente = agente.Nagente;

                        //Updates Agente with the data given
                        _db.Agentes.Add(createObj);
                        _db.SaveChanges();
                        return createObj;
                    }
                }
            }
            return null;
        }
    }
}
