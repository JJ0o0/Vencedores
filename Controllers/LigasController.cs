using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vencedores.Data;
using Vencedores.Models;
using Vencedores.ViewModels;

namespace Vencedores.Controllers;

public class LigasController : AppController
{
    public LigasController(VencedoresDbContext db) : base(db) { }

    public async Task<IActionResult> Index() => View(await Db.Ligas.AsNoTracking()
        .Include(l => l.IdTorneioNavigation)
        .Include(l => l.LigaTimes).OrderBy(l => l.Nome).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var liga = await Db.Ligas.AsNoTracking().Include(l => l.IdTorneioNavigation)
            .FirstOrDefaultAsync(l => l.IdLiga == id);
        if (liga is null) return NotFound();
        var inscritos = await Db.LigaTimes.AsNoTracking().Where(lt => lt.IdLiga == id)
            .Select(lt => lt.IdTime).ToListAsync();
        var times = await Db.Times.AsNoTracking().OrderBy(t => t.Nome).ToListAsync();
        return View(new LigaDetalhesViewModel
        {
            Liga = liga,
            Times = times.Where(t => inscritos.Contains(t.IdTime)).ToList(),
            Disponiveis = times.Where(t => !inscritos.Contains(t.IdTime)).ToList(),
            Classificacao = await ObterClassificacao(id),
            Partidas = await ObterPartidas(id),
            Sedes = await Db.Sedes.AsNoTracking().OrderBy(s => s.Nome).ToListAsync()
        });
    }

    public async Task<IActionResult> Classificacao(int id)
    {
        var liga = await Db.Ligas.AsNoTracking().Include(l => l.IdTorneioNavigation)
            .FirstOrDefaultAsync(l => l.IdLiga == id);
        if (liga is null) return NotFound();
        ViewBag.Liga = liga;
        return View(await ObterClassificacao(id));
    }

    private async Task<List<ClassificacaoLiga>> ObterClassificacao(int id) =>
        await Db.ClassificacaoLigas.AsNoTracking().Where(c => c.IdLiga == id)
            .OrderByDescending(c => c.Pontos).ThenByDescending(c => c.SaldoPontos)
            .ThenByDescending(c => c.PontosPro).ThenBy(c => c.TimeNome).ToListAsync();

    private async Task CarregarTorneios(int id = 0)
    {
        var disponiveis = await Db.Torneios.AsNoTracking()
            .Where(t => t.Liga == null || t.IdTorneio == id)
            .OrderBy(t => t.Nome).ToListAsync();
        ViewBag.Torneios = new SelectList(disponiveis, "IdTorneio", "Nome", id);
    }

    public async Task<IActionResult> Create(int? torneioId)
    {
        var form = new LigaForm { IdTorneio = torneioId ?? 0 };
        await CarregarTorneios(form.IdTorneio);
        return View("Form", form);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LigaForm form)
    {
        if (await Db.Ligas.AnyAsync(l => l.IdTorneio == form.IdTorneio))
            ModelState.AddModelError(nameof(form.IdTorneio), "Este torneio já possui uma liga.");
        if (!await Db.Torneios.AnyAsync(t => t.IdTorneio == form.IdTorneio))
            ModelState.AddModelError(nameof(form.IdTorneio), "Selecione um torneio válido.");
        if (!ModelState.IsValid)
        {
            await CarregarTorneios(form.IdTorneio);
            return View("Form", form);
        }
        var liga = new Liga { Nome = form.Nome.Trim(), IdTorneio = form.IdTorneio };
        Db.Ligas.Add(liga);
        await Db.SaveChangesAsync();
        Sucesso("Liga criada.");
        return RedirectToAction(nameof(Details), new { id = liga.IdLiga });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var l = await Db.Ligas.FindAsync(id);
        if (l is null) return NotFound();
        await CarregarTorneios(l.IdTorneio);
        return View("Form", new LigaForm { IdLiga = id, IdTorneio = l.IdTorneio, Nome = l.Nome });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, LigaForm form)
    {
        if (id != form.IdLiga) return BadRequest();
        var liga = await Db.Ligas.FindAsync(id);
        if (liga is null) return NotFound();
        ModelState.Remove(nameof(form.IdTorneio));
        if (!ModelState.IsValid)
        {
            form.IdTorneio = liga.IdTorneio;
            await CarregarTorneios(liga.IdTorneio);
            return View("Form", form);
        }
        liga.Nome = form.Nome.Trim();
        await Db.SaveChangesAsync();
        Sucesso("Liga atualizada.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var liga = await Db.Ligas.AsNoTracking().Include(l => l.IdTorneioNavigation)
            .FirstOrDefaultAsync(l => l.IdLiga == id);
        return liga is null ? NotFound() : View(liga);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var liga = await Db.Ligas.FindAsync(id);
        if (liga is null) return NotFound();
        try
        {
            Db.Ligas.Remove(liga);
            await Db.SaveChangesAsync();
            Sucesso("Liga excluída.");
        }
        catch (DbUpdateException)
        {
            Falha("Não é possível excluir uma liga que possui partidas. Exclua as partidas primeiro.");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Inscrever(int id, int idTime)
    {
        if (!await Db.Ligas.AnyAsync(l => l.IdLiga == id)) return NotFound();
        if (!await Db.Times.AnyAsync(t => t.IdTime == idTime)) return BadRequest();
        if (await Db.LigaTimes.AnyAsync(lt => lt.IdLiga == id && lt.IdTime == idTime))
        {
            Falha("Esse time já está inscrito.");
            return RedirectToAction(nameof(Details), new { id });
        }
        Db.LigaTimes.Add(new LigaTime { IdLiga = id, IdTime = idTime });
        try
        {
            await Db.SaveChangesAsync();
            Sucesso("Time inscrito na liga.");
        }
        catch (DbUpdateException)
        {
            Falha("Não foi possível inscrever o time. Verifique se o elenco atende às regras da liga.");
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Desinscrever(int id, int idTime)
    {
        var inscricao = await Db.LigaTimes.FindAsync(id, idTime);
        if (inscricao is null) return NotFound();
        try
        {
            Db.LigaTimes.Remove(inscricao);
            await Db.SaveChangesAsync();
            Sucesso("Time removido da liga.");
        }
        catch (DbUpdateException)
        {
            Falha("Não é possível remover o time. Há partidas vinculadas ou a liga ativa ficaria com menos de dois times.");
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> GerarJogos(int id, GerarJogosForm form)
    {
        if (id != form.IdLiga) return BadRequest();
        if (!await Db.Ligas.AnyAsync(l => l.IdLiga == id)) return NotFound();
        var inscritos = await Db.LigaTimes.AsNoTracking().Where(lt => lt.IdLiga == id)
            .OrderBy(lt => lt.IdTime).Select(lt => lt.IdTime).ToListAsync();
        if (inscritos.Count < 2 || inscritos.Count > 20)
        {
            Falha("Para gerar a tabela, a liga precisa ter de 2 a 20 times.");
            return RedirectToAction(nameof(Details), new { id });
        }
        if (!await Db.Sedes.AnyAsync(s => s.IdSede == form.IdSede))
        {
            Falha("Selecione uma sede válida para os jogos.");
            return RedirectToAction(nameof(Details), new { id });
        }
        if (form.DataInicio == default)
        {
            Falha("Informe a data de início dos jogos.");
            return RedirectToAction(nameof(Details), new { id });
        }
        if (await Db.Partidas.AnyAsync(p => p.IdLiga == id))
        {
            Falha("Esta liga já possui partidas. Cadastre as demais partidas manualmente.");
            return RedirectToAction(nameof(Details), new { id });
        }

        var rotacao = inscritos.Select(x => (int?)x).ToList();
        if (rotacao.Count % 2 != 0) rotacao.Add(null);
        var total = rotacao.Count;
        var jogos = new List<Partida>();
        for (var rodada = 0; rodada < total - 1; rodada++)
        {
            for (var partida = 0; partida < total / 2; partida++)
            {
                var primeiro = rotacao[partida];
                var segundo = rotacao[total - 1 - partida];
                if (!primeiro.HasValue || !segundo.HasValue) continue;
                var inverter = (rodada + partida) % 2 != 0;
                jogos.Add(new Partida
                {
                    IdLiga = id,
                    IdTimeMandante = inverter ? segundo.Value : primeiro.Value,
                    IdTimeVisitante = inverter ? primeiro.Value : segundo.Value,
                    IdSede = form.IdSede,
                    DataPartida = form.DataInicio.AddDays(rodada * 7).AddHours(partida * 2),
                    Status = "AGENDADA"
                });
            }
            var ultimo = rotacao[total - 1];
            rotacao.RemoveAt(total - 1);
            rotacao.Insert(1, ultimo);
        }
        Db.Partidas.AddRange(jogos);
        try
        {
            await Db.SaveChangesAsync();
            Sucesso($"Tabela gerada com {jogos.Count} partidas, em turno único.");
        }
        catch (DbUpdateException)
        {
            Falha("Não foi possível gerar a tabela. Confira as inscrições e a sede.");
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
