using System.ComponentModel.DataAnnotations;

namespace Vencedores.ViewModels;

public class TimeForm
{
    public int IdTime { get; set; }
    [Required(ErrorMessage = "Informe o nome do time.")]
    [StringLength(100)]
    [Display(Name = "Nome do time")]
    public string Nome { get; set; } = "";
    [StringLength(10)]
    public string? Sigla { get; set; }
    [StringLength(100)]
    public string? Cidade { get; set; }
}

public class JogadorForm
{
    public int IdJogador { get; set; }
    [Required(ErrorMessage = "Informe o nome do jogador.")]
    [StringLength(100)]
    public string Nome { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um time.")]
    [Display(Name = "Time")]
    public int IdTime { get; set; }
}

public class SedeForm
{
    public int IdSede { get; set; }
    [Required(ErrorMessage = "Informe o nome da sede.")]
    [StringLength(150)]
    public string Nome { get; set; } = "";
    [StringLength(255)]
    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "A capacidade não pode ser negativa.")]
    public int? Capacidade { get; set; }
}

public class TorneioForm
{
    public int IdTorneio { get; set; }
    [Required(ErrorMessage = "Informe o nome do torneio.")]
    [StringLength(100)]
    public string Nome { get; set; } = "";
    [StringLength(300)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }
    [Display(Name = "Início")]
    [DataType(DataType.Date)]
    public DateTime DataInicio { get; set; } = DateTime.Today;
    [Display(Name = "Término")]
    [DataType(DataType.Date)]
    public DateTime DataFinal { get; set; } = DateTime.Today.AddMonths(1);
    [Required]
    public string Status { get; set; } = "PLANEJADO";
}

public class LigaForm
{
    public int IdLiga { get; set; }
    [Required(ErrorMessage = "Informe o nome da liga.")]
    [StringLength(100)]
    public string Nome { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um torneio.")]
    public int IdTorneio { get; set; }
}

public class PartidaForm
{
    public int IdPartida { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma liga.")]
    public int IdLiga { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o mandante.")]
    [Display(Name = "Mandante")]
    public int IdTimeMandante { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o visitante.")]
    [Display(Name = "Visitante")]
    public int IdTimeVisitante { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecione a sede.")]
    [Display(Name = "Sede")]
    public int IdSede { get; set; }
    [Display(Name = "Data e horário")]
    public DateTime DataPartida { get; set; } = DateTime.Today.AddDays(1).AddHours(15);
}

public class ResultadoForm
{
    public int IdPartida { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "O placar precisa ser zero ou maior.")]
    public int? PontosMandante { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "O placar precisa ser zero ou maior.")]
    public int? PontosVisitante { get; set; }
}

public class GerarJogosForm
{
    public int IdLiga { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Selecione a sede.")]
    public int IdSede { get; set; }
    public DateTime DataInicio { get; set; } = DateTime.Today.AddDays(1).AddHours(15);
}
