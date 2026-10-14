using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class Sede
{
    public int IdSede { get; set; }

    public string Nome { get; set; } = null!;

    public string? Endereco { get; set; }

    public int? Capacidade { get; set; }

    public virtual ICollection<Partida> Partida { get; set; } = new List<Partida>();
}
