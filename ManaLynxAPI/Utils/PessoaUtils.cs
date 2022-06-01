using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    /// <summary>
    /// Interface for Pessoa Utillitaries
    /// </summary>
    public interface IPessoaUtils
    {
        /// <summary>
        /// Finds Database Entry with unique information.
        /// </summary>
        /// <param name="pessoa">Object with information</param>
        /// <returns>True if Successful, otherwise False</returns>
        bool PessoaExists(Pessoa pessoa);
        /// <summary>
        /// Adds Database Entry with sent Information
        /// </summary>
        /// <param name="pessoa">Object with Information</param>
        /// <returns>True if Successful, otherwise False</returns>
        bool AddPessoa(Pessoa pessoa);
        /// <summary>
        /// Update Database Entry with Sent Information
        /// </summary>
        /// <param name="pessoa">Object with information</param>
        /// <returns>True if successful, otherwise False.</returns>
        bool UpdatePessoa(Pessoa pessoa);
        /// <summary>
        /// Validate and Prevent from Data Injection.
        /// </summary>
        /// <param name="pessoa">Pessoa Information.</param>
        bool ValidateModel(Pessoa? pessoa);
        /// <summary>
        /// Nulls Pessoa Fields and Updates Database Entry
        /// </summary>
        /// <param name="pessoa">Object with information</param>
        /// <returns>True if Successful, otherwise False</returns>
        bool RemovePessoa(Pessoa? pessoa);

        /// <summary>
        /// Contains database object in case of successful event.
        /// Otherwise Null.
        /// </summary>
        Pessoa? Model { get; }
        /// <summary>
        /// Contains error message in case of unsuccessful event.
        /// Otherwise Null.
        /// </summary>
        string? Error { get; }
    }

    /// <summary>
    /// Utillitaries for Pessoa CRUD Operations.
    /// </summary>
    public class PessoaUtils : IPessoaUtils
    {
        private readonly ApplicationDbContext _db;
        /// <summary>
        /// Populated with database object in case of successful event.
        /// Otherwise Null.
        /// </summary>
        public Pessoa? Model { get; private set; }
        /// <summary>
        /// Populated with error message in case of unsuccessful event.
        /// Otherwise Null.
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="db">DatabaseContext</param>
        public PessoaUtils(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Checks for repeated unique Information.
        /// </summary>
        /// <param name="pessoa">Object with Information.</param>
        /// <returns>True if found equal, otherwise False.</returns>
        public bool PessoaExists(Pessoa pessoa)
        {
            var exists = _db.Pessoas.Where(p =>
                p.Nif == pessoa.Nif ||
                p.Nss == pessoa.Nss ||
                p.Nus == pessoa.Nus
            ).FirstOrDefault();

            if (exists is not null) return true;
            return false;
        }

        /// <summary>
        /// Add Object to DB.
        /// Calls ValidateModel.
        /// </summary>
        /// <param name="pessoa">Information Object</param>
        /// <returns>True if Successful, otherwise False.</returns>
        public bool AddPessoa(Pessoa pessoa)
        {
            if(ValidateModel(pessoa) == false)
                return false;
            pessoa.Id = 0;
            _db.Pessoas.Add(pessoa);
            if (_db.SaveChanges() == 0)
            {
                Error = "Error Saving Changes to DB";
                return false;
            }
            Model = pessoa;
            Error = null;
            return true;
        }

        /// <summary>
        /// Updates Original Object of given Id, with given information
        /// </summary>
        /// <param name="pessoa">New Object Information</param>
        /// <returns>True if Successful, False Otherwise.</returns>
        public bool UpdatePessoa(Pessoa pessoa)
        {
            if (ValidateModel(pessoa) == false)
                return false;
            var dbPessoa = _db.Pessoas.Find(pessoa.Id);
            if (dbPessoa is null)
            {
                Model = null;
                Error = "Pessoa does not Exist";
                return false;
            }
            dbPessoa.Cc = pessoa.Cc;
            dbPessoa.DataNascimento = pessoa.DataNascimento;
            dbPessoa.EstadoCivil = pessoa.EstadoCivil;
            dbPessoa.Nacionalidade = pessoa.Nacionalidade;
            dbPessoa.Nss = pessoa.Nss;
            dbPessoa.Nus = pessoa.Nus;
            dbPessoa.Nif = pessoa.Nif;
            dbPessoa.Nome = pessoa.Nome;
            dbPessoa.ValidadeCc = pessoa.ValidadeCc;
            _db.Pessoas.Update(dbPessoa);
            if (_db.SaveChanges() == 0)
            {
                Model = null;
                Error = "Error Saving Changes to DB";
                return false;
            }
            Model = pessoa;
            Error = null;
            return true;
        }

        /// <summary>
        /// Nulls Every Field, and Updates Object.
        /// </summary>
        /// <param name="pessoa"></param>
        /// <returns></returns>
        public bool RemovePessoa(Pessoa? pessoa)
        {
            if (pessoa is null) return false;
            pessoa.Cc = null;
            pessoa.DataNascimento = null;
            pessoa.EstadoCivil = null;
            pessoa.Nacionalidade = null;
            pessoa.Nif = null;
            pessoa.Nome = null;
            pessoa.Nss = null;
            pessoa.Nus = null;
            pessoa.ValidadeCc = null;
            _db.Pessoas.Update(pessoa);
            if (_db.SaveChanges() == 0) return false;
            return true;
        }

        /// <summary>
        /// Validate and Prevent from Data Injection.
        /// </summary>
        /// <param name="pessoa">Pessoa Information.</param>
        public bool ValidateModel(Pessoa? pessoa)
        {
            if (pessoa is null)
            {
                Error = "Pessoa is null";
                return false;
            }
            pessoa.Agentes.Clear();
            pessoa.Clientes.Clear();
            pessoa.Contactos.Clear();
            pessoa.ManaUsers.Clear();
            if (pessoa.Nome is not null && pessoa.Nome.Length >= 45)
            {
                Error = "Nome is too Long.";
                return false;
            }
            if (pessoa.Cc is not null && pessoa.Cc.Length >= 45)
            {
                Error = "Cc is too Long.";
                return false;
            }
            if (pessoa.Nif is not null && pessoa.Nif.Length >= 45)
            {
                Error = "Nif is too Long.";
                return false;
            }
            if (pessoa.Nss is not null && pessoa.Nss.Length >= 45)
            {
                Error = "Nss is too Long.";
                return false;
            }
            if (pessoa.Nus is not null && pessoa.Nus.Length >= 45)
            {
                Error = "Nus is too Long.";
                return false;
            }
            if (pessoa.Nacionalidade is not null && pessoa.Nacionalidade.Length >= 45)
            {
                Error = "Nacionalidade is too Long.";
                return false;
            }
            
            if(pessoa.EstadoCivil is not null)
            {
                bool exists = false;
                foreach (EstadoCivil estado in Enum.GetValues(typeof(EstadoCivil)))
                {
                    if (pessoa.EstadoCivil.Equals(estado.ToString()) || pessoa.EstadoCivil.Equals("Uniao de Facto"))
                        exists = true;
                }
                if(exists == false)
                {
                    Error = "EstadoCivil is too Long.";
                    return false;
                }
            }
            return true;
        }
    }
}
