USE vencedores;

INSERT INTO torneios (nome, descricao, data_inicio, data_final)
VALUES ('Copa Vencedores', 'Exemplo de liga por pontos corridos', '2026-10-15', '2026-11-30');
SET @torneio = LAST_INSERT_ID();

INSERT INTO ligas (id_torneio, nome)
VALUES (@torneio, 'Liga Principal');
SET @liga = LAST_INSERT_ID();

INSERT INTO times (nome, sigla, cidade)
VALUES ('Time Azul', 'AZL', 'Aracaju');
SET @azul = LAST_INSERT_ID();
INSERT INTO times (nome, sigla, cidade)
VALUES ('Time Vermelho', 'VRM', 'Aracaju');
SET @vermelho = LAST_INSERT_ID();

INSERT INTO jogadores (nome, id_time)
VALUES ('Ana', @azul), ('Bruno', @azul),
       ('Carla', @vermelho), ('Diego', @vermelho);

INSERT INTO liga_time (id_liga, id_time)
VALUES (@liga, @azul), (@liga, @vermelho);

INSERT INTO sedes (nome, endereco, capacidade)
VALUES ('Ginásio Central', 'Centro, Aracaju - SE', 500);
SET @sede = LAST_INSERT_ID();

INSERT INTO partidas
    (id_liga, id_time_mandante, id_time_visitante, id_sede, data_partida)
VALUES (@liga, @azul, @vermelho, @sede, '2026-10-20 15:00:00');
SET @partida = LAST_INSERT_ID();

UPDATE torneios SET status = 'EM_ANDAMENTO'
WHERE id_torneio = @torneio;

CALL registrar_resultado(@partida, 2, 1);

SELECT * FROM classificacao_liga
WHERE id_liga = @liga
ORDER BY pontos DESC, saldo_pontos DESC, pontos_pro DESC, time_nome ASC;

