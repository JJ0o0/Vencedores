using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class JogadoresController : AppController
{
    public JogadoresController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index(int? timeId)
    {
        var consulta = Db.Jogadores.AsNoTracking().Include(j => j.IdTimeNavigation).AsQueryable();
        if (timeId.HasValue) consulta = consulta.Where(j => j.IdTime == timeId.Value);
        ViewBag.TimeId = timeId;
        ViewBag.Times = new SelectList(await Db.Times.OrderBy(t => t.Nome).ToListAsync(), "IdTime", "Nome", timeId);
        return View(await consulta.OrderBy(j => j.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var j = await Db.Jogadores.AsNoTracking().Include(j => j.IdTimeNavigation)
            .FirstOrDefaultAsync(j => j.IdJogador == id);
        return j is null ? NotFound() : View(j);
    }

    private async Task CarregarTimes(int selecionado = 0)
    {
        ViewBag.Times = new SelectList(await Db.Times.AsNoTracking().OrderBy(t => t.Nome).ToListAsync(), "IdTime", "Nome", selecionado);
    }

    public async Task<IActionResult> Create(int? timeId)
    {
        var form = new JogadorForm { IdTime = timeId ?? 0 };
        await CarregarTimes(form.IdTime);
        return View("Form", form);
    }

    [HttpPost]
    public async Task<IActionResult> Create(JogadorForm form)
    {
        if (!await Db.Times.AnyAsync(t => t.IdTime == form.IdTime))
            ModelState.AddModelError(nameof(form.IdTime), "Selecione um time válido.");
        if (!ModelState.IsValid)
        {
            await CarregarTimes(form.IdTime);
            return View("Form", form);
        }
        Db.Jogadores.Add(new Jogadore { Nome = form.Nome.Trim(), IdTime = form.IdTime });
        await Db.SaveChangesAsync();
        Sucesso("Jogador cadastrado.");
        return RedirectToAction("Details", "Times", new { id = form.IdTime });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var jogador = await Db.Jogadores.FindAsync(id);
        if (jogador is null) return NotFound();
        await CarregarTimes(jogador.IdTime);
        return View("Form", new JogadorForm { IdJogador = id, Nome = jogador.Nome, IdTime = jogador.IdTime });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, JogadorForm form)
    {
        if (id != form.IdJogador) return BadRequest();
        if (!await Db.Times.AnyAsync(t => t.IdTime == form.IdTime))
            ModelState.AddModelError(nameof(form.IdTime), "Selecione um time válido.");
        if (!ModelState.IsValid)
        {
            await CarregarTimes(form.IdTime);
            return View("Form", form);
        }
        var jogador = await Db.Jogadores.FindAsync(id);
        if (jogador is null) return NotFound();
        jogador.Nome = form.Nome.Trim();
        jogador.IdTime = form.IdTime;
        try
        {
            await Db.SaveChangesAsync();
            Sucesso("Jogador atualizado.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "A transferência não é permitida se deixar um time ativo com menos de dois jogadores.");
            await CarregarTimes(form.IdTime);
            return View("Form", form);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var jogador = await Db.Jogadores.AsNoTracking().Include(j => j.IdTimeNavigation)
            .FirstOrDefaultAsync(j => j.IdJogador == id);
        return jogador is null ? NotFound() : View(jogador);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var jogador = await Db.Jogadores.FindAsync(id);
        if (jogador is null) return NotFound();
        var idTime = jogador.IdTime;
        try
        {
            Db.Jogadores.Remove(jogador);
            await Db.SaveChangesAsync();
            Sucesso("Jogador excluído.");
        }
        catch (DbUpdateException)
        {
            Falha("Não é possível deixar um time de torneio em andamento com menos de dois jogadores.");
        }
        return RedirectToAction("Details", "Times", new { id = idTime });
    }
}
