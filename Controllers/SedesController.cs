using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class SedesController : AppController
{
    public SedesController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index() => View(await Db.Sedes.AsNoTracking().OrderBy(s => s.Nome).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var sede = await Db.Sedes.AsNoTracking().FirstOrDefaultAsync(s => s.IdSede == id);
        return sede is null ? NotFound() : View(sede);
    }

    public IActionResult Create() => View("Form", new SedeForm());

    [HttpPost]
    public async Task<IActionResult> Create(SedeForm form)
    {
        if (!ModelState.IsValid) return View("Form", form);
        Db.Sedes.Add(new Sede { Nome = form.Nome.Trim(), Endereco = form.Endereco?.Trim(), Capacidade = form.Capacidade });
        await Db.SaveChangesAsync();
        Sucesso("Sede cadastrada.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var s = await Db.Sedes.FindAsync(id);
        if (s is null) return NotFound();
        return View("Form", new SedeForm { IdSede = id, Nome = s.Nome, Endereco = s.Endereco, Capacidade = s.Capacidade });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, SedeForm form)
    {
        if (id != form.IdSede) return BadRequest();
        if (!ModelState.IsValid) return View("Form", form);
        var sede = await Db.Sedes.FindAsync(id);
        if (sede is null) return NotFound();
        sede.Nome = form.Nome.Trim();
        sede.Endereco = form.Endereco?.Trim();
        sede.Capacidade = form.Capacidade;
        await Db.SaveChangesAsync();
        Sucesso("Sede atualizada.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var sede = await Db.Sedes.AsNoTracking().FirstOrDefaultAsync(s => s.IdSede == id);
        return sede is null ? NotFound() : View(sede);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sede = await Db.Sedes.FindAsync(id);
        if (sede is null) return NotFound();
        try
        {
            Db.Sedes.Remove(sede);
            await Db.SaveChangesAsync();
            Sucesso("Sede excluída.");
        }
        catch (DbUpdateException)
        {
            Falha("Não é possível excluir uma sede utilizada em partidas.");
        }
        return RedirectToAction(nameof(Index));
    }
}
