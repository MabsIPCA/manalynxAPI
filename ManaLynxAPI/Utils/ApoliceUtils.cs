using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface IApoliceUtils
    {
        Tuple<string, Apolice?> CreateApolice(Apolice ap, string tipo);
        Apolice Clear(Apolice ap);

        bool CanUpdate(int id);
        int? GetClienteId(int id);
        Tuple<string, Apolice?> UpdateApolice(Apolice updateObj, Apolice obj);
        Tuple<string, ApolicePessoal?> CreateApolicePessoal(ApolicePessoal createObj, ApolicePessoal obj);
        Tuple<string, ApoliceSaude?> CreateApoliceSaude(ApoliceSaude createObj, ApoliceSaude obj);
        Tuple<string, ApoliceVeiculo?> CreateApoliceVeiculo(ApoliceVeiculo createObj, ApoliceVeiculo obj);
        Tuple<string, ApolicePessoal?> UpdateApolicePessoal(ApolicePessoal updateObj, ApolicePessoal obj);
        Tuple<string, ApoliceVeiculo?> UpdateApoliceVeiculo(ApoliceVeiculo updateObj, ApoliceVeiculo obj);
    }

    public class ApoliceUtils : IApoliceUtils
    {
        private readonly ApplicationDbContext _db;

        public ApoliceUtils(ApplicationDbContext db)
        {
            _db = db;
        }
        /// <summary>
        /// accessd by controllers to create a new apolice
        /// </summary>
        /// <param name="ap">apolice obj</param>
        /// <param name="tipo">apolice type</param>
        /// <returns>valid id if created</returns>
        public Tuple<string, Apolice?> CreateApolice(Apolice ap, string tipo)
        {
            if (ap.Simulacao != "Não Validada" && ap.Simulacao != "Validada" && ap.Simulacao != "Pagamento Emitido" && ap.Simulacao != "Aprovada") return new Tuple<string, Apolice?>("Invalid Field", null);
            if (ap.Validade != null) return new Tuple<string, Apolice?>("Invalid Field", null);
            //if (_db.Agentes.Find(ap.AgenteId) == null) return new Tuple<string, Apolice?>("Invalid Agente", null);
            if (ap.Fracionamento != "Mensal" && ap.Fracionamento != "Trimestral" && ap.Fracionamento != "Semestral" && ap.Fracionamento != "Anual") return new Tuple<string, Apolice?>("Invalid Field", null);
            var apolice = new Apolice();
            apolice.Ativa = false;
            if (ap.AgenteId != null)
            {
                apolice.AgenteId = ap.AgenteId;
            }

            var seguro = _db.Seguros.Find(ap.SeguroId);

            if (seguro is not null && seguro.Ativo && seguro.Tipo == tipo)
            {
                apolice.Premio = ap.Premio;
                apolice.SeguroId = ap.SeguroId;
                apolice.Fracionamento = ap.Fracionamento;
                apolice.Simulacao = ap.Simulacao;
                _db.Apolices.Add(apolice);
                _db.SaveChanges();

                foreach (var cob in ap.CoberturaHasApolices)
                {
                    var coberturaCheck = _db.Coberturas.Find(cob.CoberturaId);
                    if(coberturaCheck.SeguroId == seguro.Id)
                    {
                        var cobertura = new CoberturaHasApolice();

                        cobertura.CoberturaId = cob.CoberturaId;
                        cobertura.ApoliceId = apolice.Id;
                        _db.CoberturaHasApolices.Add(cobertura);
                        if (_db.SaveChanges() == 0)
                        {
                            return new Tuple<string, Apolice?>("Invalid Cobertura", null);
                        }
                    }
                    else
                    {
                        return new Tuple<string, Apolice?>("Invalid Cobertura", null);
                    }                    
                }
                return new Tuple<string, Apolice?>("", apolice);
            }
            else return new Tuple<string, Apolice?>("Invalid Seguro", null);
        }

        public Apolice Clear(Apolice ap)
        {
            ap.Agente = null;
            ap.ApolicePessoals.Clear();
            ap.ApoliceSaudes.Clear();
            ap.ApoliceVeiculos.Clear();
            ap.CoberturaHasApolices.Clear();
            ap.Pagamentos.Clear();
            ap.Seguro = null;
            return ap;
        }

        /// <summary>
        /// Verifys if Apolice can be updated
        /// </summary>
        /// <param name="id">Apolice Id</param>
        /// <returns>true or false</returns>
        public bool CanUpdate(int id)
        {
            var ap = _db.Apolices.Find(id);
            if (ap.Ativa == true || ap.Simulacao == "Pagamento Emitido") return false;
            return true;
        }

        /// <summary>
        /// Gets apolice Cliente Id
        /// </summary>
        /// <param name="id">apoliceId</param>
        /// <returns>Cliente id</returns>
        public int? GetClienteId(int id)
        {
           if (id == 0) return null;
            var apV = _db.ApoliceVeiculos.Find(delegate (ApoliceVeiculo dummy) { _ = dummy.ApoliceId == id; });
            var apS = _db.ApoliceSaudes.Find(delegate (ApoliceSaude dummy) { _ = dummy.ApoliceId == id; });
            var apP = _db.ApolicePessoals.Find(delegate (ApolicePessoal dummy) { _ = dummy.ApoliceId == id; });
            if(apV != null)
            {
                var vei = _db.Veiculos.Find(apV.VeiculoId);
                if (vei != null && vei.ClienteId != null) return (int)vei.ClienteId;
            }else if(apS != null)
            {
                if(apS.ClienteId != null) return (int)apS.ClienteId;
            }else if(apP != null){
                if (apP.ClienteId != null) return (int)apP.ClienteId;
            }
            return null;
        }

        /// <summary>
        /// route to update apolice
        /// </summary>
        /// <param name="updateObj">apolice from bd</param>
        /// <param name="obj">object to update</param>
        /// <returns>TupleError String Apolice></returns>
        public Tuple<string, Apolice?> UpdateApolice(Apolice updateObj, Apolice obj)
        {
            if (updateObj.Simulacao == "Pagamento Emitido" || updateObj.Ativa == true) return new Tuple<string, Apolice?>("Invalid Update", null);
            if (obj.Simulacao != "Não Validada" && obj.Simulacao != "Validada" && obj.Simulacao != "Pagamento Emitido" && obj.Simulacao != "Aprovada") return new Tuple<string, Apolice?>("Invalid Field", null);
            if (!CanUpdate(updateObj.Id)) return new Tuple<string, Apolice?>("Invalid Update", null);
            if (updateObj.SeguroId != obj.SeguroId) return new Tuple<string, Apolice?>("Invalid Seguro", null);
            if (obj.Validade != null) return new Tuple<string, Apolice?>("Invalid Field", null);
            if ((_db.Agentes.Find(obj.AgenteId) == null && updateObj.AgenteId == null ) || (updateObj.AgenteId != null && updateObj.AgenteId != obj.AgenteId)) return new Tuple<string, Apolice?>("Invalid Agente", null);            
            if (obj.Fracionamento != "Mensal" && obj.Fracionamento != "Trimestral" && obj.Fracionamento != "Semestral" && obj.Fracionamento != "Anual") return new Tuple<string, Apolice?>("Invalid Field", null);

            updateObj.Premio = obj.Premio;
            updateObj.Fracionamento = obj.Fracionamento;
            updateObj.Simulacao = obj.Simulacao;
            _db.SaveChanges();

            return new Tuple<string, Apolice?>("", updateObj);
        }

        /// <summary>
        /// Create Apolice Pessoal from controller
        /// </summary>
        /// <param name="createObj">Apolice Pessoal</param>
        /// <param name="obj">Obj sent by Post</param>
        /// <returns>Tuple string, ApolicePessoal?</returns>
        public Tuple<string, ApolicePessoal?> CreateApolicePessoal(ApolicePessoal createObj, ApolicePessoal obj)
        {
            if (obj.Apolice == null || obj.Valor == null) return new Tuple<string, ApolicePessoal?>("Invalid Apolice", null);
            if (obj.Valor == 0) return new Tuple<string, ApolicePessoal?>("Invalid Valor", null);

            string errorMessage;
            Apolice? ap;

            (errorMessage ,ap) = CreateApolice(obj.Apolice, "Pessoal");
            if (ap == null) return new Tuple<string, ApolicePessoal?>(errorMessage, null);

            createObj.ApoliceId = ap.Id;
            createObj.Valor = obj.Valor;
            createObj.ClienteId = obj.ClienteId;
            _db.ApolicePessoals.Add(createObj);
            _db.SaveChanges();

            return new Tuple<string, ApolicePessoal?>("", createObj);
        }

        /// <summary>
        /// Create Apolice Saude from controller
        /// </summary>
        /// <param name="createObj">Apolice Saude</param>
        /// <param name="obj">Obj sent by Post</param>
        /// <returns>Tuple string, ApoliceSaude?</returns>
        public Tuple<string, ApoliceSaude?> CreateApoliceSaude(ApoliceSaude createObj, ApoliceSaude obj)
        {
            if (obj.Apolice == null ) return new Tuple<string, ApoliceSaude?>("Invalid Apolice", null);

            string errorMessage;
            Apolice? ap;

            (errorMessage, ap) = CreateApolice(obj.Apolice, "Saude");
            if (ap == null) return new Tuple<string, ApoliceSaude?>(errorMessage, null);

            createObj.ApoliceId = ap.Id;
            createObj.ClienteId = obj.ClienteId;
            _db.ApoliceSaudes.Add(createObj);
            _db.SaveChanges();

            return new Tuple<string, ApoliceSaude?>("", createObj);
        }

        /// <summary>
        /// Create Apolice Veiculo from controller
        /// </summary>
        /// <param name="createObj">Apolice Veiculo</param>
        /// <param name="obj">Obj sent by Post</param>
        /// <returns>Tuple string, ApoliceVeiculo?</returns>
        public Tuple<string, ApoliceVeiculo?> CreateApoliceVeiculo(ApoliceVeiculo createObj, ApoliceVeiculo obj)
        {
            if (obj.Apolice == null) return new Tuple<string, ApoliceVeiculo?>("Invalid Apolice", null);
            if (obj.DataCartaConducao == null) return new Tuple<string, ApoliceVeiculo?>("Invalid Field", null);
            if (obj.AcidentesRecentes == null) return new Tuple<string, ApoliceVeiculo?>("Invalid Field", null);

            string errorMessage;
            Apolice? ap;

            (errorMessage, ap) = CreateApolice(obj.Apolice, "Veiculo");
            if (ap == null) return new Tuple<string, ApoliceVeiculo?>(errorMessage, null);

            createObj.ApoliceId = ap.Id;
            createObj.DataCartaConducao = obj.DataCartaConducao;
            createObj.AcidentesRecentes = obj.AcidentesRecentes;
            createObj.VeiculoId = obj.VeiculoId;
            _db.ApoliceVeiculos.Add(createObj);
            _db.SaveChanges();

            return new Tuple<string, ApoliceVeiculo?>("", createObj);
        }

        /// <summary>
        /// Update Apolice Pessoal from controller
        /// </summary>
        /// <param name="createObj">Apolice Pessoal</param>
        /// <param name="obj">Obj sent by Post</param>
        /// <returns>Tuple string, ApolicePessoal?</returns>
        public Tuple<string, ApolicePessoal?> UpdateApolicePessoal(ApolicePessoal updateObj, ApolicePessoal obj)
        {
            if (obj.Valor == null) return new Tuple<string, ApolicePessoal?>("Invalid Valor", null);
            if (obj.Valor == 0) return new Tuple<string, ApolicePessoal?>("Invalid Valor", null);
            if (obj.ClienteId != updateObj.ClienteId) return new Tuple<string, ApolicePessoal?>("Invalid Cliente", null);
            if (!CanUpdate((int)updateObj.ApoliceId)) return new Tuple<string, ApolicePessoal?>("Permission Denied", null);

            updateObj.Valor = obj.Valor;
            _db.ApolicePessoals.Update(updateObj);
            _db.SaveChanges();

            return new Tuple<string, ApolicePessoal?>("", updateObj);
        }

        /// <summary>
        /// Update Apolice Veiculo from controller
        /// </summary>
        /// <param name="createObj">Apolice Veiculo</param>
        /// <param name="obj">Obj sent by Post</param>
        /// <returns>Tuple string, ApoliceVeiculo?</returns>
        public Tuple<string, ApoliceVeiculo?> UpdateApoliceVeiculo(ApoliceVeiculo updateObj, ApoliceVeiculo obj)
        {
            if (obj.DataCartaConducao == null) return new Tuple<string, ApoliceVeiculo?>("Invalid Field", null);
            if (obj.AcidentesRecentes == null) return new Tuple<string, ApoliceVeiculo?>("Invalid Field", null);
            if (obj.VeiculoId != updateObj.VeiculoId) return new Tuple<string, ApoliceVeiculo?>("Invalid Veiculo", null);
            if (!CanUpdate((int)updateObj.ApoliceId)) return new Tuple<string, ApoliceVeiculo?>("Permission Denied", null);

            updateObj.DataCartaConducao = obj.DataCartaConducao;
            updateObj.AcidentesRecentes = obj.AcidentesRecentes;
            _db.ApoliceVeiculos.Update(updateObj);
            _db.SaveChanges();

            return new Tuple<string, ApoliceVeiculo?>("", updateObj);
        }
    }
}
