CREATE DATABASE IF NOT EXISTS vencedores
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE vencedores;

CREATE TABLE torneios (
    id_torneio INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    descricao VARCHAR(300),
    data_inicio DATE NOT NULL,
    data_final DATE NOT NULL,
    status ENUM('PLANEJADO', 'EM_ANDAMENTO', 'FINALIZADO') NOT NULL
        DEFAULT 'PLANEJADO',
    CONSTRAINT chk_torneios_datas CHECK (data_final >= data_inicio)
) ENGINE = InnoDB;

CREATE TABLE ligas (
    id_liga INT AUTO_INCREMENT PRIMARY KEY,
    id_torneio INT NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    CONSTRAINT fk_ligas_torneio
        FOREIGN KEY (id_torneio) REFERENCES torneios(id_torneio)
        ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE times (
    id_time INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    sigla VARCHAR(10),
    cidade VARCHAR(100)
) ENGINE = InnoDB;

CREATE TABLE jogadores (
    id_jogador INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    id_time INT NOT NULL,
    CONSTRAINT fk_jogadores_time
        FOREIGN KEY (id_time) REFERENCES times(id_time)
        ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE sedes (
    id_sede INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    endereco VARCHAR(255),
    capacidade INT,
    CONSTRAINT chk_sedes_capacidade
        CHECK (capacidade IS NULL OR capacidade >= 0)
) ENGINE = InnoDB;

CREATE TABLE liga_time (
    id_liga INT NOT NULL,
    id_time INT NOT NULL,
    PRIMARY KEY (id_liga, id_time),
    CONSTRAINT fk_liga_time_liga
        FOREIGN KEY (id_liga) REFERENCES ligas(id_liga)
        ON DELETE CASCADE ON UPDATE RESTRICT,
    CONSTRAINT fk_liga_time_time
        FOREIGN KEY (id_time) REFERENCES times(id_time)
        ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE partidas (
    id_partida INT AUTO_INCREMENT PRIMARY KEY,
    id_liga INT NOT NULL,
    id_time_mandante INT NOT NULL,
    id_time_visitante INT NOT NULL,
    id_sede INT NOT NULL,
    data_partida DATETIME NOT NULL,
    pontos_mandante INT DEFAULT NULL,
    pontos_visitante INT DEFAULT NULL,
    status ENUM('AGENDADA', 'EM_ANDAMENTO', 'FINALIZADA', 'CANCELADA')
        NOT NULL DEFAULT 'AGENDADA',

    CONSTRAINT fk_partidas_liga
        FOREIGN KEY (id_liga) REFERENCES ligas(id_liga)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_partidas_mandante_inscrito
        FOREIGN KEY (id_liga, id_time_mandante)
        REFERENCES liga_time(id_liga, id_time)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_partidas_visitante_inscrito
        FOREIGN KEY (id_liga, id_time_visitante)
        REFERENCES liga_time(id_liga, id_time)
        ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_partidas_sede
        FOREIGN KEY (id_sede) REFERENCES sedes(id_sede)
        ON DELETE RESTRICT ON UPDATE RESTRICT,

    CONSTRAINT chk_partidas_times_distintos
        CHECK (id_time_mandante <> id_time_visitante),
    CONSTRAINT chk_partidas_pontos_nao_negativos
        CHECK ((pontos_mandante IS NULL OR pontos_mandante >= 0)
           AND (pontos_visitante IS NULL OR pontos_visitante >= 0)),
    CONSTRAINT chk_partidas_placar_status
        CHECK (
            (status = 'FINALIZADA'
             AND pontos_mandante IS NOT NULL
             AND pontos_visitante IS NOT NULL)
            OR
            (status <> 'FINALIZADA'
             AND pontos_mandante IS NULL
             AND pontos_visitante IS NULL)
        )
) ENGINE = InnoDB;

CREATE VIEW classificacao_liga AS
SELECT
    lt.id_liga,
    lt.id_time,
    t.nome AS time_nome,
    COUNT(r.id_time) AS jogos,
    COALESCE(SUM(CASE WHEN r.pontos_pro > r.pontos_contra THEN 1 ELSE 0 END), 0) AS vitorias,
    COALESCE(SUM(CASE WHEN r.pontos_pro = r.pontos_contra THEN 1 ELSE 0 END), 0) AS empates,
    COALESCE(SUM(CASE WHEN r.pontos_pro < r.pontos_contra THEN 1 ELSE 0 END), 0) AS derrotas,
    COALESCE(SUM(r.pontos_pro), 0) AS pontos_pro,
    COALESCE(SUM(r.pontos_contra), 0) AS pontos_contra,
    COALESCE(SUM(r.pontos_pro - r.pontos_contra), 0) AS saldo_pontos,
    COALESCE(SUM(CASE
        WHEN r.pontos_pro > r.pontos_contra THEN 3
        WHEN r.pontos_pro = r.pontos_contra THEN 1
        ELSE 0
    END), 0) AS pontos
FROM liga_time AS lt
INNER JOIN times AS t ON t.id_time = lt.id_time
LEFT JOIN (
    SELECT id_liga, id_time_mandante AS id_time,
           pontos_mandante AS pontos_pro, pontos_visitante AS pontos_contra
    FROM partidas
    WHERE status = 'FINALIZADA'
    UNION ALL
    SELECT id_liga, id_time_visitante AS id_time,
           pontos_visitante AS pontos_pro, pontos_mandante AS pontos_contra
    FROM partidas
    WHERE status = 'FINALIZADA'
) AS r ON r.id_liga = lt.id_liga AND r.id_time = lt.id_time
GROUP BY lt.id_liga, lt.id_time, t.nome;

DELIMITER $$

CREATE TRIGGER trg_torneios_validar_inicio
BEFORE UPDATE ON torneios
FOR EACH ROW
BEGIN
    DECLARE v_liga INT DEFAULT NULL;
    DECLARE v_qtd_times INT DEFAULT 0;
    DECLARE v_times_incompletos INT DEFAULT 0;

    IF NEW.status <> OLD.status AND NEW.status <> 'PLANEJADO' THEN
        SELECT MAX(id_liga) INTO v_liga
        FROM ligas WHERE id_torneio = OLD.id_torneio;

        IF v_liga IS NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'O torneio precisa de uma liga.';
        END IF;

        SELECT COUNT(*) INTO v_qtd_times
        FROM liga_time WHERE id_liga = v_liga;

        IF v_qtd_times < 2 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'A liga precisa de pelo menos 2 times.';
        END IF;

        SELECT COUNT(*) INTO v_times_incompletos
        FROM liga_time AS lt
        WHERE lt.id_liga = v_liga
          AND (SELECT COUNT(*) FROM jogadores AS j
               WHERE j.id_time = lt.id_time) < 2;

        IF v_times_incompletos > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Cada time precisa de pelo menos 2 jogadores.';
        END IF;
    END IF;
END$$

CREATE TRIGGER trg_jogadores_antes_excluir
BEFORE DELETE ON jogadores
FOR EACH ROW
BEGIN
    DECLARE v_qtd INT DEFAULT 0;
    DECLARE v_ligas_ativas INT DEFAULT 0;

    SELECT COUNT(*) INTO v_ligas_ativas
    FROM liga_time lt
    JOIN ligas l ON l.id_liga = lt.id_liga
    JOIN torneios tr ON tr.id_torneio = l.id_torneio
    WHERE lt.id_time = OLD.id_time AND tr.status = 'EM_ANDAMENTO';

    IF v_ligas_ativas > 0 THEN
        SELECT COUNT(*) INTO v_qtd
        FROM jogadores WHERE id_time = OLD.id_time;
        IF v_qtd <= 2 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Time ativo nao pode ficar com menos de 2 jogadores.';
        END IF;
    END IF;
END$$

CREATE TRIGGER trg_jogadores_antes_transferir
BEFORE UPDATE ON jogadores
FOR EACH ROW
BEGIN
    DECLARE v_qtd INT DEFAULT 0;
    DECLARE v_ligas_ativas INT DEFAULT 0;

    IF NEW.id_time <> OLD.id_time THEN
        SELECT COUNT(*) INTO v_ligas_ativas
        FROM liga_time lt
        JOIN ligas l ON l.id_liga = lt.id_liga
        JOIN torneios tr ON tr.id_torneio = l.id_torneio
        WHERE lt.id_time = OLD.id_time AND tr.status = 'EM_ANDAMENTO';

        IF v_ligas_ativas > 0 THEN
            SELECT COUNT(*) INTO v_qtd
            FROM jogadores WHERE id_time = OLD.id_time;
            IF v_qtd <= 2 THEN
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'Transferencia deixaria time ativo com menos de 2 jogadores.';
            END IF;
        END IF;
    END IF;
END$$

CREATE TRIGGER trg_liga_time_antes_excluir
BEFORE DELETE ON liga_time
FOR EACH ROW
BEGIN
    DECLARE v_ativa INT DEFAULT 0;
    DECLARE v_times INT DEFAULT 0;

    SELECT COUNT(*) INTO v_ativa
    FROM ligas l
    JOIN torneios tr ON tr.id_torneio = l.id_torneio
    WHERE l.id_liga = OLD.id_liga AND tr.status = 'EM_ANDAMENTO';

    IF v_ativa > 0 THEN
        SELECT COUNT(*) INTO v_times
        FROM liga_time WHERE id_liga = OLD.id_liga;
        IF v_times <= 2 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Liga ativa precisa manter pelo menos 2 times.';
        END IF;
    END IF;
END$$

CREATE TRIGGER trg_liga_time_antes_atualizar
BEFORE UPDATE ON liga_time
FOR EACH ROW
BEGIN
    IF NEW.id_liga <> OLD.id_liga OR NEW.id_time <> OLD.id_time THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Nao altere IDs de liga_time: exclua e cadastre novamente.';
    END IF;
END$$

CREATE TRIGGER trg_liga_time_antes_inserir
BEFORE INSERT ON liga_time
FOR EACH ROW
BEGIN
    DECLARE v_ativa INT DEFAULT 0;
    DECLARE v_jogadores INT DEFAULT 0;

    SELECT COUNT(*) INTO v_ativa
    FROM ligas l
    JOIN torneios tr ON tr.id_torneio = l.id_torneio
    WHERE l.id_liga = NEW.id_liga AND tr.status = 'EM_ANDAMENTO';

    IF v_ativa > 0 THEN
        SELECT COUNT(*) INTO v_jogadores
        FROM jogadores WHERE id_time = NEW.id_time;
        IF v_jogadores < 2 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Time precisa de 2 jogadores para entrar em liga ativa.';
        END IF;
    END IF;
END$$

CREATE PROCEDURE registrar_resultado(
    IN p_id_partida INT,
    IN p_pontos_mandante INT,
    IN p_pontos_visitante INT
)
BEGIN
    DECLARE v_status VARCHAR(20) DEFAULT NULL;
    DECLARE v_encontrada BOOLEAN DEFAULT TRUE;

    DECLARE CONTINUE HANDLER FOR NOT FOUND
        SET v_encontrada = FALSE;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    IF p_id_partida IS NULL
       OR p_pontos_mandante IS NULL OR p_pontos_mandante < 0
       OR p_pontos_visitante IS NULL OR p_pontos_visitante < 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Informe partida valida e pontuacoes nao negativas.';
    END IF;

    START TRANSACTION;

    SELECT status INTO v_status
    FROM partidas
    WHERE id_partida = p_id_partida
    FOR UPDATE;

    IF NOT v_encontrada THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Partida nao encontrada.';
    END IF;

    IF v_status = 'CANCELADA' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Nao e possivel registrar resultado de partida cancelada.';
    END IF;

    UPDATE partidas
    SET pontos_mandante = p_pontos_mandante,
        pontos_visitante = p_pontos_visitante,
        status = 'FINALIZADA'
    WHERE id_partida = p_id_partida;

    COMMIT;
END$$

CREATE PROCEDURE cancelar_partida(IN p_id_partida INT)
BEGIN
    DECLARE v_encontrada BOOLEAN DEFAULT TRUE;
    DECLARE v_status VARCHAR(20) DEFAULT NULL;

    DECLARE CONTINUE HANDLER FOR NOT FOUND
        SET v_encontrada = FALSE;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    IF p_id_partida IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'ID da partida e obrigatorio.';
    END IF;

    START TRANSACTION;

    SELECT status INTO v_status
    FROM partidas
    WHERE id_partida = p_id_partida
    FOR UPDATE;

    IF NOT v_encontrada THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Partida nao encontrada.';
    END IF;

    UPDATE partidas
    SET status = 'CANCELADA',
        pontos_mandante = NULL,
        pontos_visitante = NULL
    WHERE id_partida = p_id_partida;

    COMMIT;
END$$

DELIMITER ;


