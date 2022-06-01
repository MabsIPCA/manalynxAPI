using ManaLynxAPI.Data;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Utils
{
    public interface ISinistroUtils
    {
        int? CreateSinistro(Sinistro sin);
        bool CanAddProva(int? id);
        int? SinistroToAguardarValidacao(int? id);
        int? SinistroSubmitRelatorio(int? id, bool deferido);
        Tuple<Sinistro?, string> UpdateSinistro(Sinistro obj);
        Tuple<SinistroPessoal?, string> CreateSinistroPessoal(SinistroPessoal sinPe);
        Tuple<SinistroVeiculo?, string> CreateSinistroVeiculo(SinistroVeiculo sinVe);
    }

    public class SinistroUtils : ISinistroUtils
    {
        private readonly ApplicationDbContext _db;
        public SinistroUtils(ApplicationDbContext db)
        {
            _db = db;
        }
        public int? CreateSinistro(Sinistro sin)
        {
            var sinistro = new Sinistro();
            sinistro.Estado = "Aguardar Validação";
            sinistro.Descricao = sin.Descricao;
            sinistro.DataSinistro = sin.DataSinistro;
            sinistro.Valido = false;
            sinistro.Deferido = false;

            _db.Sinistros.Add(sinistro);
            _db.SaveChanges();

            return sinistro.Id;
        }

        public bool CanAddProva(int? id)
        {
            var sin = _db.Sinistros.Find(id);
            if (sin == null) return false;
            if (sin.Estado == "Aguardar Peritagem") return false;
            else return true;
        }
        public int? SinistroToAguardarValidacao(int? id)
        {
            var sin = _db.Sinistros.Find(id);
            if (sin == null) return null;
            sin.Estado = "Aguardar Validação";
            _db.Sinistros.Update(sin);
            _db.SaveChanges();
            return sin.Id;
        }

        public int? SinistroSubmitRelatorio(int? id, bool deferido)
        {
            var sin = _db.Sinistros.Find(id);
            if (sin == null) return null;
            if (deferido)
            {
                sin.Estado = "Resultado Emitido";
                sin.Deferido = true;
                sin.Valido = true;
            }
            else
            {
                sin.Estado = "Resultado Emitido";
                sin.Deferido = false;
                sin.Valido = true;
            }
            return sin.Id;
        }

        public Tuple<Sinistro?, string> UpdateSinistro(Sinistro obj)
        {
            var updateObj = _db.Sinistros.Find(obj.Id);

            if (updateObj != null)
            {
                //Assigns variables to the updateObj
                if (obj.Descricao is not null) updateObj.Descricao = obj.Descricao;
                if (obj.Reembolso is not null) updateObj.Reembolso = obj.Reembolso;
                if (obj.Valido is not null) updateObj.Valido = obj.Valido;
                if (obj.Deferido is not null) updateObj.Deferido = obj.Deferido;

                if (obj.Valido == false && obj.Deferido == false) {
                    updateObj.Estado = "Aguardar Validação";
                    updateObj.Reembolso = 0;
                } else {
                    if (obj.Valido == true && obj.Deferido == true) {
                        updateObj.Estado = "Resultado Emitido";
                    } else  {
                        if (obj.Valido == true && obj.Deferido == false)
                        {
                            updateObj.Estado = "Reportado";
                            updateObj.Reembolso = 0;
                        } else {
                            if (obj.Valido == false && obj.Deferido == true)
                            {
                                updateObj.Estado = "Resultado Emitido";
                                updateObj.Reembolso = 0;
                            }
                        }
                    }
                }

                if ((obj.Valido == false || obj.Deferido == false) && (obj.Reembolso > 0 ))
                    return Tuple.Create<Sinistro?, string>(updateObj, "Reembolso not appliable");

                _db.Sinistros.Update(updateObj);
                _db.SaveChanges();

                return Tuple.Create<Sinistro?, string>(updateObj, "");
            }
            else return Tuple.Create<Sinistro?, string>(updateObj, "Sinistro not found");
        }

        public Tuple<SinistroPessoal?, string> CreateSinistroPessoal(SinistroPessoal sinPe)
        {
            if (sinPe != null)
            {
                var sin = sinPe.Sinistro;
                var createObj = new SinistroPessoal();

                if (sin.DataSinistro > DateTime.Now)
                {
                    return Tuple.Create<SinistroPessoal?, string>(createObj, "Data not accepted");
                }

                sinPe.SinistroId = CreateSinistro(sin);
                createObj.SinistroId = sinPe.SinistroId;
                createObj.ApolicePessoalId = sinPe.ApolicePessoalId;

                //Update SinistroPessoal with the data given
                _db.SinistroPessoals.Add(createObj);
                _db.SaveChanges();

                return Tuple.Create<SinistroPessoal?, string>(createObj, "");
            }
            else return Tuple.Create<SinistroPessoal?, string>(null, "Sinistro or Sinistro Pessoal is invalid");
        }

        public Tuple<SinistroVeiculo?, string> CreateSinistroVeiculo(SinistroVeiculo sinVe)
        {
            if (sinVe != null)
            {
                var sin = sinVe.Sinistro;
                var createObj = new SinistroVeiculo();

                sinVe.SinistroId = CreateSinistro(sin);
                createObj.SinistroId = sinVe.SinistroId;
                createObj.ApoliceVeiculoId = sinVe.ApoliceVeiculoId;

                //Update SinistroVeiculo with the data given
                _db.SinistroVeiculos.Add(createObj);
                _db.SaveChanges();

                return Tuple.Create<SinistroVeiculo?, string>(createObj, "");
            }
            else return Tuple.Create<SinistroVeiculo?, string>(null, "Sinistro or Sinistro Veiculo is invalid");
        }

    }
}
