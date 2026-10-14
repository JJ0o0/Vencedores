using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class Time
{
    public int IdTime { get; set; }

    public string Nome { get; set; } = null!;

    public string? Sigla { get; set; }

    public string? Cidade { get; set; }

    public virtual ICollection<Jogadore> Jogadores { get; set; } = new List<Jogadore>();

    public virtual ICollection<LigaTime> LigaTimes { get; set; } = new List<LigaTime>();
}
