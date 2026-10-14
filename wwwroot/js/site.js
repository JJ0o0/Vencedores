document.querySelectorAll('form[data-confirm]').forEach(form => {
    form.addEventListener('submit', event => {
        if (!window.confirm(form.dataset.confirm || 'Confirmar esta operação?')) {
            event.preventDefault();
        }
    });
});

const inscricoes = document.getElementById('inscricoes-data');
if (inscricoes) {
    const liga = document.getElementById('IdLiga');
    const mandante = document.getElementById('IdTimeMandante');
    const visitante = document.getElementById('IdTimeVisitante');
    const registros = JSON.parse(inscricoes.textContent);

    const atualizar = () => {
        const permitidos = new Set(registros
            .filter(item => item.IdLiga === Number(liga.value))
            .map(item => item.IdTime));

        [mandante, visitante].forEach(select => {
            Array.from(select.options).forEach(option => {
                if (option.value !== '0') {
                    option.hidden = !permitidos.has(Number(option.value));
                    option.disabled = !permitidos.has(Number(option.value));
                }
            });
            if (!permitidos.has(Number(select.value))) {
                select.value = '0';
            }
        });
    };

    liga.addEventListener('change', atualizar);
    atualizar();
}
