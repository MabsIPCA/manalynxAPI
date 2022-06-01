using ManaLynxAPI.Data;
using ManaLynxAPI.Models;
using System.IdentityModel.Tokens.Jwt;

namespace ManaLynxAPI.Utils
{
    /// <summary>
    /// Interface for Transient Service that makes available Application Utillitaries.
    /// </summary>
    public interface IAppUtils
    {
        /// <summary>
        /// Get user id claim from Bearer Token
        /// </summary>
        /// <param name="Bearer">Bearer Token</param>
        /// <returns>user id</returns>
        int? GetUserId(string Bearer);
        /// <summary>
        /// Get user role claim from Bearer Token
        /// </summary>
        /// <param name="Bearer">Bearer Token</param>
        /// <returns>user role</returns>
        Roles? GetUserRole(string Bearer);
        /// <summary>
        /// Get Id from equipa of a Gestor user id
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <returns>EquipaId</returns>
        int? GetEquipaId(int? manaUserId);
        /// <summary>
        /// Get agente id from a Agente user id
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <returns>AgenteId</returns>
        int? GetAgenteId(int? manaUserId);
        /// <summary>
        /// Get Cliente id from a Cliente user id
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <returns>ClienteId</returns>
        int? GetClienteId(int? manaUserId);
        /// <summary>
        /// Given a <paramref name="equipaId"/>
        /// </summary>
        /// <param name="equipaId"></param>
        /// <returns></returns>
        List<int>? GetEquipaAgentes(int? equipaId);
        /// <summary>
        /// String to Fracionamento `Conversion`
        /// </summary>
        /// <param name="fracionamentoName"></param>
        /// <returns>Returns Fracionamento or null if equivalent not found.</returns>
        Fracionamento? GetFracionamento(string fracionamentoName);
        /// <summary>
        /// String to Tensao `Conversion`
        /// </summary>
        /// <param name="tensaoName"></param>
        /// <returns>Returns Tensao or null if equivalent not found.</returns>
        Tensao? GetTensao(string tensaoName);
        /// <summary>
        /// Sets Injectable Fields to null.
        /// </summary>
        /// <param name="apolice"></param>
        void PreventApoliceInjection(ref Apolice apolice);
        /// <summary>
        /// Algorithm to calculate ApóliceSaude Prémio
        /// </summary>
        /// <param name="clienteId"></param>
        /// <returns>Prémio in euros</returns>
        double? CalculateSaudePremio(int clienteId);
    }

    /// <summary>
    /// Class for Transient Service that makes available Application Utillitaries.
    /// </summary>
    public class AppUtils : IAppUtils
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Default Constructor for Application Utilitaries
        /// </summary>
        /// <param name="db">Transient for DbContext</param>
        public AppUtils(ApplicationDbContext db)
        {
            _db = db;
        }
        
        /// <summary>
        /// Get user id claim from Bearer Token
        /// </summary>
        /// <param name="Bearer">Bearer Token</param>
        /// <returns>user id</returns>
        public int? GetUserId(string Bearer)
        {
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Bearer);
            var idToken = jwtSecurityToken.Claims.First(claim => claim.Type == "Id").Value;
            int id;
            bool success = int.TryParse(idToken, out id);
            if (!success)
            {
                return null;
            }
            else return id;
        }

        /// <summary>
        /// Get user role claim from Bearer Token
        /// </summary>
        /// <param name="Bearer">Bearer Token</param>
        /// <returns>user role</returns>
        public Roles? GetUserRole(string Bearer)
        {
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(Bearer);
            var roleToken = jwtSecurityToken.Claims.First(claim => claim.Type == "role").Value;
            var role = GetRole(roleToken);
            return role;
        }

        /// <summary>
        /// Get Id from equipa of a Gestor user id
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <returns>EquipaId</returns>
        public int? GetEquipaId(int? manaUserId)
        {
            if (manaUserId == null) return null;
            var equipaId = (from equipa in _db.Equipas
                            join agente in _db.Agentes on equipa.Id equals agente.EquipaId
                            join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                            join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                            where manauser.Id == manaUserId
                            select equipa.Id).ToList().FirstOrDefault();
            if(equipaId != 0)
            {
                return equipaId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get agente id from a Agente user id
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <returns>AgenteId</returns>
        public int? GetAgenteId(int? manaUserId)
        {
            if (manaUserId == null) return null;
            var agenteId = (from agente in _db.Agentes
                            join pessoa in _db.Pessoas on agente.PessoaId equals pessoa.Id
                            join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                            where manauser.Id == manaUserId
                            select agente.Id).ToList().FirstOrDefault();
            if (agenteId != 0)
            {
                return agenteId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get Cliente id from a Cliente user id
        /// </summary>
        /// <param name="manaUserId">user Id</param>
        /// <returns>ClienteId</returns>
        public int? GetClienteId(int? manaUserId)
        {
            if (manaUserId == null) return null;
            var clienteId = (from cliente in _db.Clientes
                             join pessoa in _db.Pessoas on cliente.PessoaId equals pessoa.Id
                             join manauser in _db.ManaUsers on pessoa.Id equals manauser.PessoaId
                             where manauser.Id == manaUserId
                             select cliente.Id).ToList().FirstOrDefault();
            if (clienteId != 0)
            {
                return clienteId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Given a <paramref name="equipaId"/>
        /// </summary>
        /// <param name="equipaId"></param>
        /// <returns></returns>
        public List<int>? GetEquipaAgentes(int? equipaId)
        {
            List<int> agentesList= new();
            if (equipaId is null) return null;

            var agentes = (from equipa in _db.Equipas
                           where equipa.Id == equipaId
                           select equipa.Agentes).FirstOrDefault();

            if(agentes is not null)
            {
                foreach(var agente in agentes)
                {
                    agentesList.Add(agente.Id);
                }
            }

            return agentesList;
        }

        /// <summary>
        /// String to Role `Conversion`
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns>Returns Role or null if equivalent not found.</returns>
        private static Roles? GetRole(string roleName)
        {
            foreach (Roles role in Enum.GetValues(typeof(Roles)))
            {
                if (role.ToString()!.Equals(roleName))
                    return role;
            }
            return null;
        }

        /// <summary>
        /// String to Fracionamento `Conversion`
        /// </summary>
        /// <param name="fracionamentoName"></param>
        /// <returns>Returns Fracionamento or null if equivalent not found.</returns>
        public Fracionamento? GetFracionamento(string fracionamentoName)
        {
            foreach (Fracionamento fracionamento in Enum.GetValues(typeof(Fracionamento)))
            {
                if (fracionamento.ToString()!.Equals(fracionamentoName))
                    return fracionamento;
            }
            return null;
        }

        /// <summary>
        /// String to Tensao `Conversion`
        /// </summary>
        /// <param name="tensaoName"></param>
        /// <returns>Returns Tensao or null if equivalent not found.</returns>
        public Tensao? GetTensao(string tensaoName)
        {
            foreach (Tensao tensao in Enum.GetValues(typeof(Tensao)))
            {
                if (tensao.ToString()!.Equals(tensaoName))
                    return tensao;
            }
            return null;
        }

        /// <summary>
        /// Sets Injectable Fields to null.
        /// </summary>
        /// <param name="apolice"></param>
        public void PreventApoliceInjection(ref Apolice apolice)
        {
            apolice.ApolicePessoals.Clear();
            apolice.ApoliceSaudes.Clear();
            apolice.ApoliceVeiculos.Clear();
            apolice.CoberturaHasApolices.Clear();
            apolice.Pagamentos.Clear();
            apolice.Agente = null;
            apolice.Seguro = null;
        }

        /// <summary>
        /// Algorithm to calculate ApóliceSaude Prémio
        /// </summary>
        /// <param name="clienteId"></param>
        /// <returns>Prémio in euros</returns>
        public double? CalculateSaudePremio(int clienteId)
        {
            double premio = 0;
             
            var cliente = _db.Clientes.Find(clienteId);
            if (cliente is null) return null;

            var pessoa = _db.Pessoas.Find(cliente.PessoaId);
            if (pessoa is null || pessoa.DataNascimento is null) return null;

            var idadeCliente = DateTime.Today.Year - pessoa.DataNascimento.Value.Year;
            double mul = idadeCliente / 50;

            var dadoClinico = _db.Clientes.Where(c => c.Id == clienteId).Select(c => c.DadoClinico).FirstOrDefault();
            if (dadoClinico is null) return null;
            if (dadoClinico.Altura is null || dadoClinico.Peso is null) return null;

            int numDoencas = dadoClinico.DadosClinicoHasDoencas.Count;

            double imc = dadoClinico.Peso.Value / Math.Pow(dadoClinico.Altura.Value, 2);
            imc = Math.Round(imc, 1);

            if (imc <= 0) return null;

            if (imc < 18.5) // Magreza
            {
                premio += 20;
            }
            else if (imc >= 18.5 && imc < 25) // Normal
            {

            }
            else if (imc >= 25 && imc < 30) // Sobrepeso
            {
                premio += 10;
            }
            else if (imc >= 30 && imc < 35) // Obesidade I
            {
                premio += 20;
            }
            else if (imc >= 35 && imc < 40) // Obesidade II
            {
                premio += 30;
            }
            else if (imc >= 40) // Obesidade III
            {
                premio += 50;
            }

            if (dadoClinico.Tensao is null) return null;
            var tensao = GetTensao(dadoClinico.Tensao.ToString());
            switch (tensao)
            {
                case Tensao.Hipotenso:
                    premio += 20;
                    break;
                case Tensao.Hipertenso:
                    premio += 30;
                    break;
            }

            premio += numDoencas * 10;

            return (mul * premio);
        }
    }
}
