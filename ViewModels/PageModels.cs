using Vencedores.Models;

namespace Vencedores.ViewModels;

public class DashboardViewModel
{
    public int TotalTorneios { get; set; }
    public int TotalTimes { get; set; }
    public int TotalJogadores { get; set; }
    public int PartidasFinalizadas { get; set; }
    public List<Torneio> Torneios { get; set; } = new();
    public List<PartidaRow> Partidas { get; set; } = new();
}

public class PartidaRow
{
    public int IdPartida { get; set; }
    public int IdLiga { get; set; }
    public string Liga { get; set; } = "";
    public string Mandante { get; set; } = "";
    public string Visitante { get; set; } = "";
    public string Sede { get; set; } = "";
    public DateTime DataPartida { get; set; }
    public int? PontosMandante { get; set; }
    public int? PontosVisitante { get; set; }
    public string Status { get; set; } = "";
}

public class LigaDetalhesViewModel
{
    public Liga Liga { get; set; } = null!;
    public List<Time> Times { get; set; } = new();
    public List<Time> Disponiveis { get; set; } = new();
    public List<ClassificacaoLiga> Classificacao { get; set; } = new();
    public List<PartidaRow> Partidas { get; set; } = new();
    public List<Sede> Sedes { get; set; } = new();
}

public class TimeDetalhesViewModel
{
    public Time Time { get; set; } = null!;
    public List<Jogadore> Jogadores { get; set; } = new();
    public List<Liga> Ligas { get; set; } = new();
}
