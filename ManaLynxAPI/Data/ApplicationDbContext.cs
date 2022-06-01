using Microsoft.EntityFrameworkCore;
using ManaLynxAPI.Models;

namespace ManaLynxAPI.Data
{


    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Agente> Agentes { get; set; } = null!;
        public virtual DbSet<Apolice> Apolices { get; set; } = null!;
        public virtual DbSet<ApolicePessoal> ApolicePessoals { get; set; } = null!;
        public virtual DbSet<ApoliceSaude> ApoliceSaudes { get; set; } = null!;
        public virtual DbSet<ApoliceVeiculo> ApoliceVeiculos { get; set; } = null!;
        public virtual DbSet<CategoriaVeiculo> CategoriaVeiculos { get; set; } = null!;
        public virtual DbSet<Cliente> Clientes { get; set; } = null!;
        public virtual DbSet<Cobertura> Coberturas { get; set; } = null!;
        public virtual DbSet<CoberturaHasApolice> CoberturaHasApolices { get; set; } = null!;
        public virtual DbSet<Contacto> Contactos { get; set; } = null!;
        public virtual DbSet<DadoClinico> DadoClinicos { get; set; } = null!;
        public virtual DbSet<DadosClinicoHasDoenca> DadosClinicoHasDoencas { get; set; } = null!;
        public virtual DbSet<Doenca> Doencas { get; set; } = null!;
        public virtual DbSet<Equipa> Equipas { get; set; } = null!;
        public virtual DbSet<Gestor> Gestors { get; set; } = null!;
        public virtual DbSet<Log> Logs { get; set; } = null!;
        public virtual DbSet<LoginCredential> LoginCredentials { get; set; } = null!;
        public virtual DbSet<ManaUser> ManaUsers { get; set; } = null!;
        public virtual DbSet<Pagamento> Pagamentos { get; set; } = null!;
        public virtual DbSet<Pessoa> Pessoas { get; set; } = null!;
        public virtual DbSet<Prova> Provas { get; set; } = null!;
        public virtual DbSet<RelatorioPeritagem> RelatorioPeritagems { get; set; } = null!;
        public virtual DbSet<Seguro> Seguros { get; set; } = null!;
        public virtual DbSet<Sinistro> Sinistros { get; set; } = null!;
        public virtual DbSet<SinistroPessoal> SinistroPessoals { get; set; } = null!;
        public virtual DbSet<SinistroVeiculo> SinistroVeiculos { get; set; } = null!;
        public virtual DbSet<Transacao> Transacaos { get; set; } = null!;
        public virtual DbSet<Tratamento> Tratamentos { get; set; } = null!;
        public virtual DbSet<Veiculo> Veiculos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=tcp:manalynx.database.windows.net,1433;Database=ManaLynx;Initial Catalog=ManaLynx;Persist Security Info=False;User ID=ManalynxAdmin;Password=ManaLynx?1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agente>(entity =>
            {
                entity.ToTable("Agente", "Manalynx");

                entity.Property(e => e.Nagente).HasColumnName("NAgente");

                entity.HasOne(d => d.Equipa)
                    .WithMany(p => p.Agentes)
                    .HasForeignKey(d => d.EquipaId)
                    .HasConstraintName("FK__Agente__EquipaId__00DF2177");

                entity.HasOne(d => d.Pessoa)
                    .WithMany(p => p.Agentes)
                    .HasForeignKey(d => d.PessoaId)
                    .HasConstraintName("FK__Agente__PessoaId__7A3223E8");
            });

            modelBuilder.Entity<Apolice>(entity =>
            {
                entity.ToTable("Apolice", "Manalynx");

                entity.Property(e => e.Fracionamento).HasMaxLength(45);
                entity.Property(e => e.Simulacao).HasMaxLength(45);

                entity.Property(e => e.Validade).HasColumnType("date");

                entity.HasOne(d => d.Agente)
                    .WithMany(p => p.Apolices)
                    .HasForeignKey(d => d.AgenteId)
                    .HasConstraintName("FK__Apolice__AgenteI__27F8EE98");

                entity.HasOne(d => d.Seguro)
                    .WithMany(p => p.Apolices)
                    .HasForeignKey(d => d.SeguroId)
                    .HasConstraintName("FK__Apolice__SeguroI__2704CA5F");
            });

            modelBuilder.Entity<ApolicePessoal>(entity =>
            {
                entity.ToTable("ApolicePessoal", "Manalynx");

                entity.HasOne(d => d.Apolice)
                    .WithMany(p => p.ApolicePessoals)
                    .HasForeignKey(d => d.ApoliceId)
                    .HasConstraintName("FK__ApolicePe__Apoli__4865BE2A");

                entity.HasOne(d => d.Cliente)
                    .WithMany(p => p.ApolicePessoals)
                    .HasForeignKey(d => d.ClienteId)
                    .HasConstraintName("FK__ApolicePe__Clien__4959E263");
            });

            modelBuilder.Entity<ApoliceSaude>(entity =>
            {
                entity.ToTable("ApoliceSaude", "Manalynx");

                entity.HasOne(d => d.Apolice)
                    .WithMany(p => p.ApoliceSaudes)
                    .HasForeignKey(d => d.ApoliceId)
                    .HasConstraintName("FK__ApoliceSa__Apoli__32767D0B");

                entity.HasOne(d => d.Cliente)
                    .WithMany(p => p.ApoliceSaudes)
                    .HasForeignKey(d => d.ClienteId)
                    .HasConstraintName("FK__ApoliceSa__Clien__318258D2");
            });

            modelBuilder.Entity<ApoliceVeiculo>(entity =>
            {
                entity.ToTable("ApoliceVeiculo", "Manalynx");

                entity.HasOne(d => d.Apolice)
                    .WithMany(p => p.ApoliceVeiculos)
                    .HasForeignKey(d => d.ApoliceId)
                    .HasConstraintName("FK__ApoliceVe__Apoli__3BFFE745");

                entity.HasOne(d => d.Veiculo)
                    .WithMany(p => p.ApoliceVeiculos)
                    .HasForeignKey(d => d.VeiculoId)
                    .HasConstraintName("FK__ApoliceVe__Veicu__3CF40B7E");
                entity.Property(e => e.DataCartaConducao).HasColumnType("date");
            });

            modelBuilder.Entity<CategoriaVeiculo>(entity =>
            {
                entity.ToTable("CategoriaVeiculo", "Manalynx");

                entity.Property(e => e.Categoria)
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente", "Manalynx");

                entity.Property(e => e.AgenteId).HasColumnName("AgenteID");

                entity.Property(e => e.Profissao)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.Agente)
                    .WithMany(p => p.Clientes)
                    .HasForeignKey(d => d.AgenteId)
                    .HasConstraintName("FK__Cliente__AgenteI__1F63A897");

                entity.HasOne(d => d.DadoClinico)
                    .WithMany(p => p.Clientes)
                    .HasForeignKey(d => d.DadoClinicoId)
                    .HasConstraintName("FK__Cliente__DadoCli__1E6F845E");

                entity.HasOne(d => d.Pessoa)
                    .WithMany(p => p.Clientes)
                    .HasForeignKey(d => d.PessoaId)
                    .HasConstraintName("FK__Cliente__PessoaI__1D7B6025");
            });

            modelBuilder.Entity<Cobertura>(entity =>
            {
                entity.ToTable("Cobertura", "Manalynx");

                entity.Property(e => e.DescricaoCobertura)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.Seguro)
                    .WithMany(p => p.Coberturas)
                    .HasForeignKey(d => d.SeguroId)
                    .HasConstraintName("FK__Cobertura__Segur__10216507");
            });

            modelBuilder.Entity<CoberturaHasApolice>(entity =>
            {
                entity.ToTable("CoberturaHasApolice", "Manalynx");

                entity.HasOne(d => d.Apolice)
                    .WithMany(p => p.CoberturaHasApolices)
                    .HasForeignKey(d => d.ApoliceId)
                    .HasConstraintName("FK__Cobertura__Apoli__2EA5EC27");

                entity.HasOne(d => d.Cobertura)
                    .WithMany(p => p.CoberturaHasApolices)
                    .HasForeignKey(d => d.CoberturaId)
                    .HasConstraintName("FK__Cobertura__Cober__2DB1C7EE");
            });

            modelBuilder.Entity<Contacto>(entity =>
            {
                entity.ToTable("Contacto", "Manalynx");

                entity.Property(e => e.Tipo).HasMaxLength(45);

                entity.Property(e => e.Valor)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.Pessoa)
                    .WithMany(p => p.Contactos)
                    .HasForeignKey(d => d.PessoaId)
                    .HasConstraintName("FK__Contacto__Pessoa__39237A9A");
            });

            modelBuilder.Entity<DadoClinico>(entity =>
            {
                entity.ToTable("DadoClinico", "Manalynx");

                entity.Property(e => e.Tensao).HasMaxLength(45);
            });

            modelBuilder.Entity<DadosClinicoHasDoenca>(entity =>
            {
                entity.ToTable("DadosClinicoHasDoenca", "Manalynx");

                entity.HasOne(d => d.DadoClinico)
                    .WithMany(p => p.DadosClinicoHasDoencas)
                    .HasForeignKey(d => d.DadoClinicoId)
                    .HasConstraintName("FK__DadosClin__DadoC__17C286CF");

                entity.HasOne(d => d.Doenca)
                    .WithMany(p => p.DadosClinicoHasDoencas)
                    .HasForeignKey(d => d.DoencaId)
                    .HasConstraintName("FK__DadosClin__Doenc__18B6AB08");
            });

            modelBuilder.Entity<Doenca>(entity =>
            {
                entity.ToTable("Doenca", "Manalynx");

                entity.Property(e => e.Descricao)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.NomeDoenca)
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Equipa>(entity =>
            {
                entity.ToTable("Equipa", "Manalynx");

                entity.Property(e => e.Nome)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Regiao)
                    .HasMaxLength(127)
                    .IsUnicode(false);

                entity.HasOne(d => d.Gestor)
                    .WithMany(p => p.Equipas)
                    .HasForeignKey(d => d.GestorId)
                    .HasConstraintName("FK__Equipa__GestorId__7FEAFD3E");
            });

            modelBuilder.Entity<Gestor>(entity =>
            {
                entity.ToTable("Gestor", "Manalynx");

                entity.HasOne(d => d.Agente)
                    .WithMany(p => p.Gestors)
                    .HasForeignKey(d => d.AgenteId)
                    .HasConstraintName("FK__Gestor__AgenteId__7D0E9093");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Logs", "Manalynx");

                entity.Property(e => e.LogDate)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.Property(e => e.Query)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LoginCredential>(entity =>
            {
                entity.ToTable("LoginCredential", "Manalynx");

                entity.Property(e => e.ManaHash)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ManaSalt)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ManaUser>(entity =>
            {
                entity.ToTable("ManaUser", "Manalynx");

                entity.Property(e => e.Email)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.UserRole).HasMaxLength(45);

                entity.Property(e => e.Username)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.LoginCredentialNavigation)
                    .WithMany(p => p.ManaUsers)
                    .HasForeignKey(d => d.LoginCredential)
                    .HasConstraintName("FK__ManaUser__LoginC__078C1F06");

                entity.HasOne(d => d.Pessoa)
                    .WithMany(p => p.ManaUsers)
                    .HasForeignKey(d => d.PessoaId)
                    .HasConstraintName("FK__ManaUser__Pessoa__0697FACD");
            });

            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.ToTable("Pagamento", "Manalynx");

                entity.Property(e => e.DataEmissao).HasColumnType("date");

                entity.Property(e => e.DataPagamento).HasColumnType("date");

                entity.Property(e => e.Metodo)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.Apolice)
                    .WithMany(p => p.Pagamentos)
                    .HasForeignKey(d => d.ApoliceId)
                    .HasConstraintName("FK__Pagamento__Apoli__2AD55B43");
            });

            modelBuilder.Entity<Pessoa>(entity =>
            {
                entity.ToTable("Pessoa", "Manalynx");

                entity.Property(e => e.Cc)
                    .HasMaxLength(45)
                    .IsUnicode(false)
                    .HasColumnName("CC");

                entity.Property(e => e.DataNascimento).HasColumnType("date");

                entity.Property(e => e.EstadoCivil).HasMaxLength(45);

                entity.Property(e => e.Nacionalidade)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Nif)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("NIF");

                entity.Property(e => e.Nome)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Nss)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("NSS");

                entity.Property(e => e.Nus)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("NUS");

                entity.Property(e => e.ValidadeCc)
                    .HasColumnType("date")
                    .HasColumnName("ValidadeCC");
            });

            modelBuilder.Entity<Prova>(entity =>
            {
                entity.ToTable("Prova", "Manalynx");

                entity.Property(e => e.Conteudo)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DataSubmissao).HasColumnType("date");

                entity.HasOne(d => d.Sinistro)
                    .WithMany(p => p.Provas)
                    .HasForeignKey(d => d.SinistroId)
                    .HasConstraintName("FK__Prova__SinistroI__42ACE4D4");
            });

            modelBuilder.Entity<RelatorioPeritagem>(entity =>
            {
                entity.ToTable("RelatorioPeritagem", "Manalynx");

                entity.Property(e => e.Conteudo)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DataRelatorio).HasColumnType("date");

                entity.HasOne(d => d.Sinistro)
                    .WithMany(p => p.RelatorioPeritagems)
                    .HasForeignKey(d => d.SinistroId)
                    .HasConstraintName("FK__Relatorio__Sinis__4589517F");
            });

            modelBuilder.Entity<Seguro>(entity =>
            {
                entity.ToTable("Seguro", "Manalynx");

                entity.Property(e => e.Nome)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Tipo).HasMaxLength(45);
            });

            modelBuilder.Entity<Sinistro>(entity =>
            {
                entity.ToTable("Sinistro", "Manalynx");

                entity.Property(e => e.DataSinistro).HasColumnType("date");

                entity.Property(e => e.Descricao)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Estado).HasMaxLength(45);
            });

            modelBuilder.Entity<SinistroPessoal>(entity =>
            {
                entity.ToTable("SinistroPessoal", "Manalynx");

                entity.HasOne(d => d.ApolicePessoal)
                    .WithMany(p => p.SinistroPessoals)
                    .HasForeignKey(d => d.ApolicePessoalId)
                    .HasConstraintName("FK__SinistroP__Apoli__4C364F0E");

                entity.HasOne(d => d.Sinistro)
                    .WithMany(p => p.SinistroPessoals)
                    .HasForeignKey(d => d.SinistroId)
                    .HasConstraintName("FK__SinistroP__Sinis__4D2A7347");
            });

            modelBuilder.Entity<SinistroVeiculo>(entity =>
            {
                entity.ToTable("SinistroVeiculo", "Manalynx");

                entity.HasOne(d => d.ApoliceVeiculo)
                    .WithMany(p => p.SinistroVeiculos)
                    .HasForeignKey(d => d.ApoliceVeiculoId)
                    .HasConstraintName("FK__SinistroV__Apoli__50FB042B");

                entity.HasOne(d => d.Sinistro)
                    .WithMany(p => p.SinistroVeiculos)
                    .HasForeignKey(d => d.SinistroId)
                    .HasConstraintName("FK__SinistroV__Sinis__5006DFF2");
            });

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.ToTable("Transacao", "Manalynx");

                entity.Property(e => e.DataTransacao).HasColumnType("date");

                entity.Property(e => e.Descricao)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.HasOne(d => d.ApoliceSaude)
                    .WithMany(p => p.Transacaos)
                    .HasForeignKey(d => d.ApoliceSaudeId)
                    .HasConstraintName("FK__Transacao__Apoli__3552E9B6");
            });

            modelBuilder.Entity<Tratamento>(entity =>
            {
                entity.ToTable("Tratamento", "Manalynx");

                entity.Property(e => e.Frequencia)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.NomeTratamento)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.UltimaToma).HasColumnType("date");

                entity.HasOne(d => d.DadoClinico)
                    .WithMany(p => p.Tratamentos)
                    .HasForeignKey(d => d.DadoClinicoId)
                    .HasConstraintName("FK__Tratament__DadoC__12FDD1B2");
            });

            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable("Veiculo", "Manalynx");

                entity.Property(e => e.Marca)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Matricula)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Modelo)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Vin)
                    .HasMaxLength(17)
                    .IsUnicode(false)
                    .HasColumnName("VIN");

                entity.HasOne(d => d.CategoriaVeiculo)
                    .WithMany(p => p.Veiculos)
                    .HasForeignKey(d => d.CategoriaVeiculoId)
                    .HasConstraintName("FK__Veiculo__Categor__22401542");

                entity.HasOne(d => d.Cliente)
                    .WithMany(p => p.Veiculos)
                    .HasForeignKey(d => d.ClienteId)
                    .HasConstraintName("FK__Veiculo__Cliente__2334397B");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}    
