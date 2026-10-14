using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class Partida
{
    public int IdPartida { get; set; }

    public int IdLiga { get; set; }

    public int IdTimeMandante { get; set; }

    public int IdTimeVisitante { get; set; }

    public int IdSede { get; set; }

    public DateTime DataPartida { get; set; }

    public int? PontosMandante { get; set; }

    public int? PontosVisitante { get; set; }

    public string Status { get; set; } = null!;

    public virtual Liga IdLigaNavigation { get; set; } = null!;

    public virtual Sede IdSedeNavigation { get; set; } = null!;

    public virtual LigaTime LigaTime { get; set; } = null!;

    public virtual LigaTime LigaTimeNavigation { get; set; } = null!;
}
