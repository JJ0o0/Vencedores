using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class LigaTime
{
    public int IdLiga { get; set; }

    public int IdTime { get; set; }

    public virtual Liga IdLigaNavigation { get; set; } = null!;

    public virtual Time IdTimeNavigation { get; set; } = null!;

    public virtual ICollection<Partida> PartidaLigaTimeNavigations { get; set; } = new List<Partida>();

    public virtual ICollection<Partida> PartidaLigaTimes { get; set; } = new List<Partida>();
}
