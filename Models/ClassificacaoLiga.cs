using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class ClassificacaoLiga
{
    public int IdLiga { get; set; }

    public int IdTime { get; set; }

    public string TimeNome { get; set; } = null!;

    public long Jogos { get; set; }

    public decimal Vitorias { get; set; }

    public decimal Empates { get; set; }

    public decimal Derrotas { get; set; }

    public decimal PontosPro { get; set; }

    public decimal PontosContra { get; set; }

    public decimal SaldoPontos { get; set; }

    public decimal Pontos { get; set; }
}
