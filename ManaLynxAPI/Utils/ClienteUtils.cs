using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IClienteUtils
    {
        Cliente? Model { get; }
        string? Error { get; }
        bool AddCliente(Cliente cliente);
        bool ValidateModel(Cliente cliente);
        void PreventInjection(Cliente cliente);
        bool UpdateCliente(Cliente newCliente);
    }
    public class ClienteUtils : IClienteUtils
    {
        private readonly ApplicationDbContext _db;
        private readonly IPessoaUtils _pessoa;
        private readonly IDadoClinicoUtils _dadoClinico;

        public Cliente? Model { get; private set; }
        public string? Error { get; private set; }

        public ClienteUtils(ApplicationDbContext db, IPessoaUtils pessoa, IDadoClinicoUtils dadoClinico)
        {
            _db = db;
            _pessoa = pessoa;
            _dadoClinico = dadoClinico;
        }

        public bool AddCliente(Cliente cliente)
        {
            Model = null;
            Error = null;
            if (cliente.Pessoa is null)
            {
                cliente.Pessoa = new();
            }
            if (!ValidateModel(cliente))
            {
                return false;
            }
            if (_pessoa.AddPessoa(cliente.Pessoa))
            {
                //cliente.Pessoa = _pessoa.Model;
                cliente.PessoaId = _pessoa.Model!.Id;
            }
            else
            {
                Error = "Error Adding Pessoa: " + _pessoa.Error;
                return false;
            }
            if (cliente.DadoClinico is null)
            {
                cliente.DadoClinico = new();
            }
            var dadoClinico = _dadoClinico.CreateDadoClinico(cliente.DadoClinico);
            if (string.IsNullOrEmpty(dadoClinico.Item2))
            {
                cliente.DadoClinicoId = dadoClinico.Item1!.Id;
                //cliente.DadoClinico = dadoClinico.Item1;
            }
            else
            {
                Error = "Error Adding DadoClinico: " + dadoClinico.Item2;
                return false;
            }
            _db.Clientes.Add(cliente);
            if(_db.SaveChanges() == 0)
            {
                Error = "Error Saving Changes.";
            }
            ValidateModel(cliente);
            Model = cliente;
            return true;
        }


        /// <summary>
        /// Validate Cliente Model information.
        /// </summary>
        /// <param name="cliente"></param>
        public bool ValidateModel(Cliente cliente)
        {
            cliente.Agente = null;
            cliente.DadoClinicoId = null;
            if (cliente.Profissao is not null && cliente.Profissao.Length > 45)
            {
                Error = "Profissao is too Long.";
                return false;
            }
            PreventInjection(cliente);
            _pessoa.ValidateModel(cliente.Pessoa);
            return true;
        }

        /// <summary>
        /// Prevent Data Injection.
        /// </summary>
        /// <param name="cliente"></param>
        public void PreventInjection(Cliente cliente)
        {
            cliente.ApoliceSaudes.Clear();
            cliente.ApolicePessoals.Clear();
            cliente.Veiculos.Clear();
        }


        public bool UpdateCliente(Cliente newCliente)
        {
            Cliente? oldCliente = _db.Clientes.Find(newCliente.Id);
            if(oldCliente is null)
            {
                Model = null;
                Error = "Cliente not found.";
                return false;
            }
            if (newCliente.DadoClinicoId is not null) oldCliente.DadoClinicoId = newCliente.DadoClinicoId;
            if (newCliente.Profissao is not null) oldCliente.Profissao = newCliente.Profissao;
            if (newCliente.ProfissaoRisco is not null) oldCliente.ProfissaoRisco = newCliente.ProfissaoRisco;
            if (newCliente.IsLead is not null) oldCliente.IsLead = newCliente.IsLead;
            if (newCliente.Pessoa is not null)
            {
                if (_pessoa.UpdatePessoa(newCliente.Pessoa))
                {
                }
                else
                {
                    Model = null;
                    Error = "Pessoa Update Failed: " + _pessoa.Error;
                    return false;
                }
            }

            _db.Clientes.Update(oldCliente);
            if(_db.SaveChanges() == 0)
            {
                Model = null;
                Error = "Error Saving Changes";
                return false;
            }

            ValidateModel(oldCliente);
            Model = oldCliente;
            Error = null;
            return true;
        }

        private bool DeleteCliente(Cliente cliente)
        {
            cliente.Profissao = null;
            cliente.ProfissaoRisco = null;
            return _pessoa.RemovePessoa(cliente.Pessoa);
        }
    }
}
