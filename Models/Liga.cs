using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class Liga
{
    public int IdLiga { get; set; }

    public int IdTorneio { get; set; }

    public string Nome { get; set; } = null!;

    public virtual Torneio IdTorneioNavigation { get; set; } = null!;

    public virtual ICollection<LigaTime> LigaTimes { get; set; } = new List<LigaTime>();

    public virtual ICollection<Partida> Partida { get; set; } = new List<Partida>();
}
