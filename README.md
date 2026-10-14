# Vencedores

Aplicação web para organizar torneios esportivos em pontos corridos. Desenvolvida com ASP.NET Core MVC, Razor, Entity Framework Core e MariaDB/MySQL.

## O que o sistema faz

- Criar, consultar, editar e excluir torneios, times, jogadores e sedes.
- Criar uma liga automaticamente junto com cada novo torneio.
- Inscrever e remover times das ligas.
- Validar pelo menos dois times e dois jogadores por time antes de iniciar um torneio.
- Agendar partidas manualmente ou gerar jogos em turno único com rodadas semanais.
- Iniciar, cancelar e reabrir partidas.
- Registrar e corrigir placares sem duplicar pontos.
- Consultar classificação por pontos, saldo e pontos marcados.

## Requisitos

- SDK .NET 8
- MariaDB com o esquema fornecido em `database/schema.sql`, ou MySQL compatível
- Conta do banco com permissões SELECT, INSERT, UPDATE, DELETE, EXECUTE e SHOW VIEW

## Instalação

Na pasta do projeto:

```bash
dotnet restore
dotnet user-secrets init
```

Importe o SQL em um banco vazio usando uma conta com permissões para criar tabelas, views, triggers e procedures:

```bash
mariadb -u root -p < database/schema.sql
```

Crie um usuário de aplicação no MariaDB e conceda as permissões indicadas acima. Não use o root na aplicação. Se quiser, importe dados de exemplo depois do esquema:

```bash
mariadb -u root -p < database/exemplo.sql
```

Configure a conexão no User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Vencedores" "Server=localhost;Port=3306;Database=vencedores;User=vencedores_app;Password=SUA_SENHA;"
```

Use uma senha real e não publique a saída de `dotnet user-secrets list`. No Zsh, é preferível ler a senha de modo oculto e evitar digitá-la no histórico de comandos.

Execute:

```bash
dotnet build
dotnet run
```

Acesse a URL exibida no terminal.

## Ordem de utilização

1. Cadastre um torneio. A liga será criada automaticamente.
2. Cadastre dois ou mais times.
3. Cadastre ao menos dois jogadores para cada time participante.
4. Inscreva os times na página da liga.
5. Cadastre uma sede.
6. Agende as partidas ou gere a tabela automaticamente.
7. Coloque o torneio em andamento, registre resultados e acompanhe a classificação.

## Observações

A classificação é calculada pela view `classificacao_liga` a partir das partidas com status FINALIZADA. A aplicação chama as procedures `registrar_resultado` e `cancelar_partida` definidas no SQL. Um banco de dados criado a partir de outro esquema pode ser incompatível com os modelos.

As páginas foram desenvolvidas para o escopo de pontos corridos definido no trabalho. Não foram implementados chaveamentos eliminatórios, contas de usuários, permissões por pessoa ou compartilhamento público. Antes de publicar o sistema na internet, adicione autenticação/autorização, revise as permissões e use HTTPS.

## Organização

- `Controllers/`: ações da aplicação e regras de fluxo
- `Models/`: entidades importadas do banco
- `ViewModels/`: dados recebidos dos formulários e exibidos nas páginas
- `Views/`: páginas Razor `.cshtml`
- `Data/`: Entity Framework Core e mapeamento do banco
- `wwwroot/`: CSS e JavaScript
- `database/`: SQL do banco e dados opcionais de demonstração

## Verificação

Depois de configurar a conexão, rode `dotnet build` e `dotnet run` e confira os seguintes cenários:

1. Criar um time, editá-lo e adicionar dois jogadores.
2. Criar um segundo time com dois jogadores.
3. Criar um torneio e conferir se a liga foi criada.
4. Inscrever os dois times na liga.
5. Criar uma sede, gerar o calendário ou agendar uma partida.
6. Iniciar o torneio, registrar placar 2 × 1 e conferir a tabela.
7. Corrigir o placar para 1 × 1 e conferir os pontos de ambas as equipes.
8. Cancelar a partida, conferir que a classificação volta a zero e depois reabri-la.
9. Conferir que a exclusão de um time inscrito e de uma sede utilizada são bloqueadas.

O repositório contém um workflow simples do GitHub Actions para compilar o código a cada push. A compilação não depende de conexão com o banco; executar a aplicação depende.
