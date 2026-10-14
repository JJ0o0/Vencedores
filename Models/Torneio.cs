using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class Torneio
{
    public int IdTorneio { get; set; }

    public string Nome { get; set; } = null!;

    public string? Descricao { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly DataFinal { get; set; }

    public string Status { get; set; } = null!;

    public virtual Liga? Liga { get; set; }
}
