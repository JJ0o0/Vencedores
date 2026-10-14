using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class TimesController : AppController
{
    public TimesController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index(string? busca)
    {
        var consulta = Db.Times.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            consulta = consulta.Where(t => t.Nome.Contains(busca) || (t.Cidade != null && t.Cidade.Contains(busca)));
        ViewBag.Busca = busca;
        return View(await consulta.OrderBy(t => t.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var time = await Db.Times.AsNoTracking().FirstOrDefaultAsync(t => t.IdTime == id);
        if (time is null) return NotFound();
        return View(new TimeDetalhesViewModel
        {
            Time = time,
            Jogadores = await Db.Jogadores.AsNoTracking().Where(j => j.IdTime == id).OrderBy(j => j.Nome).ToListAsync(),
            Ligas = await Db.Ligas.AsNoTracking()
                .Where(l => l.LigaTimes.Any(lt => lt.IdTime == id)).OrderBy(l => l.Nome).ToListAsync()
        });
    }

    public IActionResult Create() => View("Form", new TimeForm());

    [HttpPost]
    public async Task<IActionResult> Create(TimeForm form)
    {
        if (!ModelState.IsValid) return View("Form", form);
        Db.Times.Add(new Time { Nome = form.Nome.Trim(), Sigla = form.Sigla?.Trim(), Cidade = form.Cidade?.Trim() });
        await Db.SaveChangesAsync();
        Sucesso("Time cadastrado com sucesso.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var t = await Db.Times.FindAsync(id);
        if (t is null) return NotFound();
        return View("Form", new TimeForm { IdTime = t.IdTime, Nome = t.Nome, Sigla = t.Sigla, Cidade = t.Cidade });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, TimeForm form)
    {
        if (id != form.IdTime) return BadRequest();
        if (!ModelState.IsValid) return View("Form", form);
        var time = await Db.Times.FindAsync(id);
        if (time is null) return NotFound();
        time.Nome = form.Nome.Trim();
        time.Sigla = form.Sigla?.Trim();
        time.Cidade = form.Cidade?.Trim();
        await Db.SaveChangesAsync();
        Sucesso("Time atualizado.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var time = await Db.Times.AsNoTracking().FirstOrDefaultAsync(t => t.IdTime == id);
        return time is null ? NotFound() : View(time);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var time = await Db.Times.FindAsync(id);
        if (time is null) return NotFound();
        try
        {
            Db.Times.Remove(time);
            await Db.SaveChangesAsync();
            Sucesso("Time excluído.");
        }
        catch (DbUpdateException)
        {
            Falha("Não é possível excluir este time enquanto houver jogadores, ligas ou partidas vinculadas.");
        }
        return RedirectToAction(nameof(Index));
    }
}
