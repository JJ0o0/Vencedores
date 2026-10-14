using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class PartidasController : AppController
{
    public PartidasController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index(int? ligaId, string? status)
    {
        var partidas = await ObterPartidas(ligaId);
        if (!string.IsNullOrWhiteSpace(status))
            partidas = partidas.Where(p => p.Status == status).ToList();
        ViewBag.LigaId = ligaId;
        ViewBag.Status = status;
        ViewBag.Ligas = new SelectList(await Db.Ligas.AsNoTracking().OrderBy(l => l.Nome).ToListAsync(), "IdLiga", "Nome", ligaId);
        return View(partidas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var partida = (await ObterPartidas()).FirstOrDefault(p => p.IdPartida == id);
        return partida is null ? NotFound() : View(partida);
    }

    private async Task CarregarListas(PartidaForm form)
    {
        ViewBag.Ligas = new SelectList(await Db.Ligas.AsNoTracking().OrderBy(l => l.Nome).ToListAsync(), "IdLiga", "Nome", form.IdLiga);
        ViewBag.Sedes = new SelectList(await Db.Sedes.AsNoTracking().OrderBy(s => s.Nome).ToListAsync(), "IdSede", "Nome", form.IdSede);
        var times = await Db.Times.AsNoTracking().OrderBy(t => t.Nome).ToListAsync();
        ViewBag.Times = new SelectList(times, "IdTime", "Nome");
        var inscricoes = await Db.LigaTimes.AsNoTracking()
            .Select(lt => new { lt.IdLiga, lt.IdTime }).ToListAsync();
        ViewBag.InscricoesJson = JsonSerializer.Serialize(inscricoes);
    }

    private async Task Validar(PartidaForm form)
    {
        if (form.IdTimeMandante == form.IdTimeVisitante && form.IdTimeMandante != 0)
            ModelState.AddModelError(nameof(form.IdTimeVisitante), "Mandante e visitante precisam ser diferentes.");
        if (!await Db.Ligas.AnyAsync(l => l.IdLiga == form.IdLiga))
            ModelState.AddModelError(nameof(form.IdLiga), "Selecione uma liga válida.");
        if (!await Db.Sedes.AnyAsync(s => s.IdSede == form.IdSede))
            ModelState.AddModelError(nameof(form.IdSede), "Selecione uma sede válida.");
        if (form.DataPartida == default)
            ModelState.AddModelError(nameof(form.DataPartida), "Informe a data da partida.");
        foreach (var idTime in new[] { form.IdTimeMandante, form.IdTimeVisitante }.Distinct())
        {
            if (!await Db.LigaTimes.AnyAsync(lt => lt.IdLiga == form.IdLiga && lt.IdTime == idTime))
                ModelState.AddModelError("", "Os dois times precisam estar inscritos na liga selecionada.");
        }
    }

    public async Task<IActionResult> Create(int? ligaId)
    {
        var form = new PartidaForm { IdLiga = ligaId ?? 0 };
        await CarregarListas(form);
        return View("Form", form);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PartidaForm form)
    {
        await Validar(form);
        if (!ModelState.IsValid)
        {
            await CarregarListas(form);
            return View("Form", form);
        }
        var partida = new Partida
        {
            IdLiga = form.IdLiga,
            IdTimeMandante = form.IdTimeMandante,
            IdTimeVisitante = form.IdTimeVisitante,
            IdSede = form.IdSede,
            DataPartida = form.DataPartida,
            Status = "AGENDADA"
        };
        Db.Partidas.Add(partida);
        try
        {
            await Db.SaveChangesAsync();
            Sucesso("Partida agendada.");
            return RedirectToAction(nameof(Details), new { id = partida.IdPartida });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível cadastrar a partida. Confira os times e a liga.");
            await CarregarListas(form);
            return View("Form", form);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await Db.Partidas.FindAsync(id);
        if (p is null) return NotFound();
        var form = new PartidaForm
        {
            IdPartida = p.IdPartida, IdLiga = p.IdLiga,
            IdTimeMandante = p.IdTimeMandante, IdTimeVisitante = p.IdTimeVisitante,
            IdSede = p.IdSede, DataPartida = p.DataPartida
        };
        await CarregarListas(form);
        return View("Form", form);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, PartidaForm form)
    {
        if (id != form.IdPartida) return BadRequest();
        await Validar(form);
        if (!ModelState.IsValid)
        {
            await CarregarListas(form);
            return View("Form", form);
        }
        var partida = await Db.Partidas.FindAsync(id);
        if (partida is null) return NotFound();
        if (partida.Status == "FINALIZADA" &&
            (partida.IdLiga != form.IdLiga || partida.IdTimeMandante != form.IdTimeMandante || partida.IdTimeVisitante != form.IdTimeVisitante))
        {
            ModelState.AddModelError("", "Não altere os times ou a liga de uma partida finalizada. Corrija apenas o placar ou os dados de agendamento.");
            await CarregarListas(form);
            return View("Form", form);
        }
        partida.IdLiga = form.IdLiga;
        partida.IdTimeMandante = form.IdTimeMandante;
        partida.IdTimeVisitante = form.IdTimeVisitante;
        partida.IdSede = form.IdSede;
        partida.DataPartida = form.DataPartida;
        try
        {
            await Db.SaveChangesAsync();
            Sucesso("Partida atualizada.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível atualizar a partida. Confira a liga e os participantes.");
            await CarregarListas(form);
            return View("Form", form);
        }
    }

    public async Task<IActionResult> Resultado(int id)
    {
        var partida = await Db.Partidas.FindAsync(id);
        if (partida is null) return NotFound();
        if (partida.Status == "CANCELADA")
        {
            Falha("Reabra a partida antes de registrar um resultado.");
            return RedirectToAction(nameof(Details), new { id });
        }
        ViewBag.Partida = (await ObterPartidas()).First(p => p.IdPartida == id);
        return View(new ResultadoForm
        {
            IdPartida = id,
            PontosMandante = partida.PontosMandante,
            PontosVisitante = partida.PontosVisitante
        });
    }

    [HttpPost]
    public async Task<IActionResult> Resultado(int id, ResultadoForm form)
    {
        if (id != form.IdPartida) return BadRequest();
        if (!form.PontosMandante.HasValue || !form.PontosVisitante.HasValue)
            ModelState.AddModelError("", "Informe os dois placares.");
        if (!ModelState.IsValid)
        {
            var partida = (await ObterPartidas()).FirstOrDefault(p => p.IdPartida == id);
            if (partida is null) return NotFound();
            ViewBag.Partida = partida;
            return View(form);
        }
        try
        {
            await Db.Database.ExecuteSqlInterpolatedAsync($"CALL registrar_resultado({id}, {form.PontosMandante!.Value}, {form.PontosVisitante!.Value})");
            Sucesso("Resultado registrado. A classificação foi atualizada.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex) when (ex is DbUpdateException || ex is InvalidOperationException || ex is MySqlConnector.MySqlException)
        {
            Falha("O resultado não pôde ser registrado. Verifique se a partida não foi cancelada.");
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Iniciar(int id)
    {
        var partida = await Db.Partidas.FindAsync(id);
        if (partida is null) return NotFound();
        if (partida.Status != "AGENDADA")
        {
            Falha("Apenas partidas agendadas podem ser iniciadas.");
            return RedirectToAction(nameof(Details), new { id });
        }
        partida.Status = "EM_ANDAMENTO";
        await Db.SaveChangesAsync();
        Sucesso("Partida em andamento.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (!await Db.Partidas.AnyAsync(p => p.IdPartida == id)) return NotFound();
        try
        {
            await Db.Database.ExecuteSqlInterpolatedAsync($"CALL cancelar_partida({id})");
            Sucesso("Partida cancelada. O resultado não conta na classificação.");
        }
        catch (Exception ex) when (ex is DbUpdateException || ex is InvalidOperationException || ex is MySqlConnector.MySqlException)
        {
            Falha("Não foi possível cancelar a partida.");
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Reabrir(int id)
    {
        var partida = await Db.Partidas.FindAsync(id);
        if (partida is null) return NotFound();
        if (partida.Status != "CANCELADA")
        {
            Falha("Apenas partidas canceladas podem ser reabertas.");
            return RedirectToAction(nameof(Details), new { id });
        }
        partida.PontosMandante = null;
        partida.PontosVisitante = null;
        partida.Status = "AGENDADA";
        await Db.SaveChangesAsync();
        Sucesso("Partida reaberta e agendada.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var partida = (await ObterPartidas()).FirstOrDefault(p => p.IdPartida == id);
        return partida is null ? NotFound() : View(partida);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var partida = await Db.Partidas.FindAsync(id);
        if (partida is null) return NotFound();
        Db.Partidas.Remove(partida);
        await Db.SaveChangesAsync();
        Sucesso("Partida excluída. A classificação foi recalculada.");
        return RedirectToAction(nameof(Index));
    }
}
