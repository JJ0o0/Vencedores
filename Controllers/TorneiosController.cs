using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class TorneiosController : AppController
{
    public TorneiosController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        return View(await Db.Torneios.AsNoTracking().Include(t => t.Liga)
            .OrderByDescending(t => t.DataInicio).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var torneio = await Db.Torneios.AsNoTracking().Include(t => t.Liga)
            .FirstOrDefaultAsync(t => t.IdTorneio == id);
        return torneio is null ? NotFound() : View(torneio);
    }

    public IActionResult Create() => View("Form", new TorneioForm());

    private void Validar(TorneioForm form)
    {
        if (form.DataInicio == default || form.DataFinal == default)
            ModelState.AddModelError("", "Informe as datas do torneio.");
        if (form.DataFinal.Date < form.DataInicio.Date)
            ModelState.AddModelError(nameof(form.DataFinal), "O término deve ser igual ou posterior ao início.");
        if (form.Status != "PLANEJADO" && form.Status != "EM_ANDAMENTO" && form.Status != "FINALIZADO")
            ModelState.AddModelError(nameof(form.Status), "Selecione um status válido.");
    }

    [HttpPost]
    public async Task<IActionResult> Create(TorneioForm form)
    {
        form.Status = "PLANEJADO";
        Validar(form);
        if (!ModelState.IsValid) return View("Form", form);

        var torneio = new Torneio
        {
            Nome = form.Nome.Trim(),
            Descricao = form.Descricao?.Trim(),
            DataInicio = DateOnly.FromDateTime(form.DataInicio),
            DataFinal = DateOnly.FromDateTime(form.DataFinal),
            Status = "PLANEJADO",
            Liga = new Liga { Nome = form.Nome.Trim() }
        };
        Db.Torneios.Add(torneio);
        await Db.SaveChangesAsync();
        Sucesso("Torneio e liga criados. Agora inscreva os times participantes.");
        return RedirectToAction(nameof(Details), new { id = torneio.IdTorneio });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var t = await Db.Torneios.FindAsync(id);
        if (t is null) return NotFound();
        return View("Form", new TorneioForm
        {
            IdTorneio = t.IdTorneio,
            Nome = t.Nome,
            Descricao = t.Descricao,
            DataInicio = t.DataInicio.ToDateTime(TimeOnly.MinValue),
            DataFinal = t.DataFinal.ToDateTime(TimeOnly.MinValue),
            Status = t.Status
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, TorneioForm form)
    {
        if (id != form.IdTorneio) return BadRequest();
        Validar(form);
        if (!ModelState.IsValid) return View("Form", form);
        var t = await Db.Torneios.FindAsync(id);
        if (t is null) return NotFound();

        if (t.Status == "PLANEJADO" && form.Status == "FINALIZADO")
        {
            ModelState.AddModelError(nameof(form.Status), "Inicie o torneio antes de finalizá-lo.");
            return View("Form", form);
        }

        if (t.Status == "FINALIZADO" && form.Status != "FINALIZADO")
        {
            ModelState.AddModelError(nameof(form.Status), "Um torneio finalizado não pode ser reaberto.");
            return View("Form", form);
        }

        if (t.Status == "PLANEJADO" && form.Status != "PLANEJADO")
        {
            var liga = await Db.Ligas.FirstOrDefaultAsync(l => l.IdTorneio == id);
            if (liga is null)
                ModelState.AddModelError(nameof(form.Status), "Crie a liga antes de iniciar o torneio.");
            else
            {
                var ids = await Db.LigaTimes.Where(lt => lt.IdLiga == liga.IdLiga)
                    .Select(lt => lt.IdTime).ToListAsync();
                if (ids.Count < 2)
                    ModelState.AddModelError(nameof(form.Status), "Inscreva pelo menos dois times.");
                else
                {
                    var elencos = await Db.Jogadores.Where(j => ids.Contains(j.IdTime))
                        .GroupBy(j => j.IdTime).Select(g => new { Time = g.Key, Total = g.Count() }).ToListAsync();
                    if (ids.Any(timeId => elencos.All(e => e.Time != timeId || e.Total < 2)))
                        ModelState.AddModelError(nameof(form.Status), "Cada time inscrito precisa de pelo menos dois jogadores.");
                }
            }
        }

        if (!ModelState.IsValid) return View("Form", form);

        t.Nome = form.Nome.Trim();
        t.Descricao = form.Descricao?.Trim();
        t.DataInicio = DateOnly.FromDateTime(form.DataInicio);
        t.DataFinal = DateOnly.FromDateTime(form.DataFinal);
        t.Status = form.Status;

        try
        {
            await Db.SaveChangesAsync();
            Sucesso("Torneio atualizado.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "O banco recusou a alteração. Verifique os participantes e as regras do torneio.");
            return View("Form", form);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var t = await Db.Torneios.AsNoTracking().FirstOrDefaultAsync(t => t.IdTorneio == id);
        return t is null ? NotFound() : View(t);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var torneio = await Db.Torneios.FindAsync(id);
        if (torneio is null) return NotFound();
        try
        {
            Db.Torneios.Remove(torneio);
            await Db.SaveChangesAsync();
            Sucesso("Torneio excluído.");
        }
        catch (DbUpdateException)
        {
            Falha("Este torneio possui partidas vinculadas. Remova as partidas antes de excluir o torneio.");
        }
        return RedirectToAction(nameof(Index));
    }
}
