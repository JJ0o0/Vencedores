using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class HomeController : AppController
{
    public HomeController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalTorneios = await Db.Torneios.CountAsync(),
            TotalTimes = await Db.Times.CountAsync(),
            TotalJogadores = await Db.Jogadores.CountAsync(),
            PartidasFinalizadas = await Db.Partidas.CountAsync(p => p.Status == "FINALIZADA"),
            Torneios = await Db.Torneios.AsNoTracking()
                .OrderByDescending(t => t.DataInicio).Take(5).ToListAsync(),
            Partidas = await ObterPartidas(limite: 5)
        };
        return View(model);
    }

    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
