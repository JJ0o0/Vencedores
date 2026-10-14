using System;
using System.Collections.Generic;

namespace Vencedores.Models;

public partial class Jogadore
{
    public int IdJogador { get; set; }

    public string Nome { get; set; } = null!;

    public int IdTime { get; set; }

    public virtual Time IdTimeNavigation { get; set; } = null!;
}
