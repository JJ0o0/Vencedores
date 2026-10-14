using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public abstract class AppController : Controller
{
    protected readonly VencedoresDbContext Db;

    protected AppController(VencedoresDbContext db)
    {
        Db = db;
    }

    protected void Sucesso(string mensagem) => TempData["Sucesso"] = mensagem;

    protected void Falha(string mensagem) => TempData["Erro"] = mensagem;

    protected IActionResult Voltar(string action = "Index") => RedirectToAction(action)!;

    protected async Task<List<PartidaRow>> ObterPartidas(int? idLiga = null, int limite = 0)
    {
        var consulta = Db.Partidas.AsNoTracking()
            .Include(p => p.IdLigaNavigation)
            .Include(p => p.IdSedeNavigation)
            .AsQueryable();

        if (idLiga.HasValue)
            consulta = consulta.Where(p => p.IdLiga == idLiga.Value);

        consulta = consulta.OrderByDescending(p => p.DataPartida);
        if (limite > 0)
            consulta = consulta.Take(limite);

        var partidas = await consulta.ToListAsync();
        var times = await Db.Times.AsNoTracking().ToDictionaryAsync(t => t.IdTime, t => t.Nome);

        return partidas.Select(p => new PartidaRow
        {
            IdPartida = p.IdPartida,
            IdLiga = p.IdLiga,
            Liga = p.IdLigaNavigation.Nome,
            Mandante = times.GetValueOrDefault(p.IdTimeMandante, "Time removido"),
            Visitante = times.GetValueOrDefault(p.IdTimeVisitante, "Time removido"),
            Sede = p.IdSedeNavigation.Nome,
            DataPartida = p.DataPartida,
            PontosMandante = p.PontosMandante,
            PontosVisitante = p.PontosVisitante,
            Status = p.Status
        }).ToList();
    }
}
