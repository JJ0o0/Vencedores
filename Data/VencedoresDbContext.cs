using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Vencedores.Models;

namespace Vencedores.Data;

public partial class VencedoresDbContext : DbContext
{
    public VencedoresDbContext(DbContextOptions<VencedoresDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClassificacaoLiga> ClassificacaoLigas { get; set; }

    public virtual DbSet<Jogadore> Jogadores { get; set; }

    public virtual DbSet<Liga> Ligas { get; set; }

    public virtual DbSet<LigaTime> LigaTimes { get; set; }

    public virtual DbSet<Partida> Partidas { get; set; }

    public virtual DbSet<Sede> Sedes { get; set; }

    public virtual DbSet<Time> Times { get; set; }

    public virtual DbSet<Torneio> Torneios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ClassificacaoLiga>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("classificacao_liga");

            entity.Property(e => e.Derrotas)
                .HasPrecision(23)
                .HasColumnName("derrotas");
            entity.Property(e => e.Empates)
                .HasPrecision(23)
                .HasColumnName("empates");
            entity.Property(e => e.IdLiga)
                .HasColumnType("int(11)")
                .HasColumnName("id_liga");
            entity.Property(e => e.IdTime)
                .HasColumnType("int(11)")
                .HasColumnName("id_time");
            entity.Property(e => e.Jogos)
                .HasColumnType("bigint(21)")
                .HasColumnName("jogos");
            entity.Property(e => e.Pontos)
                .HasPrecision(23)
                .HasColumnName("pontos");
            entity.Property(e => e.PontosContra)
                .HasPrecision(32)
                .HasColumnName("pontos_contra");
            entity.Property(e => e.PontosPro)
                .HasPrecision(32)
                .HasColumnName("pontos_pro");
            entity.Property(e => e.SaldoPontos)
                .HasPrecision(33)
                .HasColumnName("saldo_pontos");
            entity.Property(e => e.TimeNome)
                .HasMaxLength(100)
                .HasColumnName("time_nome");
            entity.Property(e => e.Vitorias)
                .HasPrecision(23)
                .HasColumnName("vitorias");
        });

        modelBuilder.Entity<Jogadore>(entity =>
        {
            entity.HasKey(e => e.IdJogador).HasName("PRIMARY");

            entity.ToTable("jogadores");

            entity.HasIndex(e => e.IdTime, "fk_jogadores_time");

            entity.Property(e => e.IdJogador)
                .HasColumnType("int(11)")
                .HasColumnName("id_jogador");
            entity.Property(e => e.IdTime)
                .HasColumnType("int(11)")
                .HasColumnName("id_time");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.HasOne(d => d.IdTimeNavigation).WithMany(p => p.Jogadores)
                .HasForeignKey(d => d.IdTime)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_jogadores_time");
        });

        modelBuilder.Entity<Liga>(entity =>
        {
            entity.HasKey(e => e.IdLiga).HasName("PRIMARY");

            entity.ToTable("ligas");

            entity.HasIndex(e => e.IdTorneio, "id_torneio").IsUnique();

            entity.Property(e => e.IdLiga)
                .HasColumnType("int(11)")
                .HasColumnName("id_liga");
            entity.Property(e => e.IdTorneio)
                .HasColumnType("int(11)")
                .HasColumnName("id_torneio");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");

            entity.HasOne(d => d.IdTorneioNavigation).WithOne(p => p.Liga)
                .HasForeignKey<Liga>(d => d.IdTorneio)
                .HasConstraintName("fk_ligas_torneio");
        });

        modelBuilder.Entity<LigaTime>(entity =>
        {
            entity.HasKey(e => new { e.IdLiga, e.IdTime })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("liga_time");

            entity.HasIndex(e => e.IdTime, "fk_liga_time_time");

            entity.Property(e => e.IdLiga)
                .HasColumnType("int(11)")
                .HasColumnName("id_liga");
            entity.Property(e => e.IdTime)
                .HasColumnType("int(11)")
                .HasColumnName("id_time");

            entity.HasOne(d => d.IdLigaNavigation).WithMany(p => p.LigaTimes)
                .HasForeignKey(d => d.IdLiga)
                .HasConstraintName("fk_liga_time_liga");

            entity.HasOne(d => d.IdTimeNavigation).WithMany(p => p.LigaTimes)
                .HasForeignKey(d => d.IdTime)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_liga_time_time");
        });

        modelBuilder.Entity<Partida>(entity =>
        {
            entity.HasKey(e => e.IdPartida).HasName("PRIMARY");

            entity.ToTable("partidas");

            entity.HasIndex(e => new { e.IdLiga, e.IdTimeMandante }, "fk_partidas_mandante_inscrito");

            entity.HasIndex(e => e.IdSede, "fk_partidas_sede");

            entity.HasIndex(e => new { e.IdLiga, e.IdTimeVisitante }, "fk_partidas_visitante_inscrito");

            entity.Property(e => e.IdPartida)
                .HasColumnType("int(11)")
                .HasColumnName("id_partida");
            entity.Property(e => e.DataPartida)
                .HasColumnType("datetime")
                .HasColumnName("data_partida");
            entity.Property(e => e.IdLiga)
                .HasColumnType("int(11)")
                .HasColumnName("id_liga");
            entity.Property(e => e.IdSede)
                .HasColumnType("int(11)")
                .HasColumnName("id_sede");
            entity.Property(e => e.IdTimeMandante)
                .HasColumnType("int(11)")
                .HasColumnName("id_time_mandante");
            entity.Property(e => e.IdTimeVisitante)
                .HasColumnType("int(11)")
                .HasColumnName("id_time_visitante");
            entity.Property(e => e.PontosMandante)
                .HasColumnType("int(11)")
                .HasColumnName("pontos_mandante");
            entity.Property(e => e.PontosVisitante)
                .HasColumnType("int(11)")
                .HasColumnName("pontos_visitante");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'AGENDADA'")
                .HasColumnType("enum('AGENDADA','EM_ANDAMENTO','FINALIZADA','CANCELADA')")
                .HasColumnName("status");

            entity.HasOne(d => d.IdLigaNavigation).WithMany(p => p.Partida)
                .HasForeignKey(d => d.IdLiga)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_partidas_liga");

            entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Partida)
                .HasForeignKey(d => d.IdSede)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_partidas_sede");

            entity.HasOne(d => d.LigaTime).WithMany(p => p.PartidaLigaTimes)
                .HasForeignKey(d => new { d.IdLiga, d.IdTimeMandante })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_partidas_mandante_inscrito");

            entity.HasOne(d => d.LigaTimeNavigation).WithMany(p => p.PartidaLigaTimeNavigations)
                .HasForeignKey(d => new { d.IdLiga, d.IdTimeVisitante })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_partidas_visitante_inscrito");
        });

        modelBuilder.Entity<Sede>(entity =>
        {
            entity.HasKey(e => e.IdSede).HasName("PRIMARY");

            entity.ToTable("sedes");

            entity.Property(e => e.IdSede)
                .HasColumnType("int(11)")
                .HasColumnName("id_sede");
            entity.Property(e => e.Capacidade)
                .HasColumnType("int(11)")
                .HasColumnName("capacidade");
            entity.Property(e => e.Endereco)
                .HasMaxLength(255)
                .HasColumnName("endereco");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Time>(entity =>
        {
            entity.HasKey(e => e.IdTime).HasName("PRIMARY");

            entity.ToTable("times");

            entity.Property(e => e.IdTime)
                .HasColumnType("int(11)")
                .HasColumnName("id_time");
            entity.Property(e => e.Cidade)
                .HasMaxLength(100)
                .HasColumnName("cidade");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Sigla)
                .HasMaxLength(10)
                .HasColumnName("sigla");
        });

        modelBuilder.Entity<Torneio>(entity =>
        {
            entity.HasKey(e => e.IdTorneio).HasName("PRIMARY");

            entity.ToTable("torneios");

            entity.Property(e => e.IdTorneio)
                .HasColumnType("int(11)")
                .HasColumnName("id_torneio");
            entity.Property(e => e.DataFinal).HasColumnName("data_final");
            entity.Property(e => e.DataInicio).HasColumnName("data_inicio");
            entity.Property(e => e.Descricao)
                .HasMaxLength(300)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'PLANEJADO'")
                .HasColumnType("enum('PLANEJADO','EM_ANDAMENTO','FINALIZADO')")
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
