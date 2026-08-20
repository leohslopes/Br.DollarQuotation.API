<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>

<body>

<h1>Br.DollarQuotation API — Câmbio Pulse</h1>

<p>
    Plataforma de consulta, processamento e monitoramento de cotações de moedas,
    desenvolvida em <strong>.NET 9</strong> utilizando conceitos de
    <strong>Clean Architecture, SOLID, Entity Framework Core, PostgreSQL,
    RabbitMQ, Worker Services, SignalR, JWT e Docker</strong>.
</p>

<p>
    O projeto faz parte da plataforma <strong>Câmbio Pulse</strong> e tem como objetivo
    disponibilizar cotações de moedas em relação ao Real, armazenar histórico,
    gerenciar usuários, criar alertas de preço e distribuir atualizações em tempo real
    para o front-end.
</p>

<hr>

<h2>Principais funcionalidades</h2>

<ul>
    <li>Consulta de cotações de moedas.</li>
    <li>Suporte a múltiplos pares de moedas.</li>
    <li>Histórico de cotações.</li>
    <li>Resumo das cotações por período.</li>
    <li>Paginação e filtros.</li>
    <li>Atualização automática das cotações.</li>
    <li>Integração com provedor externo de câmbio.</li>
    <li>Processamento em background através de Worker Service.</li>
    <li>Persistência utilizando PostgreSQL.</li>
    <li>Mensageria utilizando RabbitMQ.</li>
    <li>Retry automático do consumer RabbitMQ.</li>
    <li>Atualização em tempo real utilizando SignalR.</li>
    <li>Cadastro e autenticação de usuários.</li>
    <li>Autenticação utilizando JWT Bearer Token.</li>
    <li>Perfis de acesso <strong>User</strong> e <strong>Admin</strong>.</li>
    <li>Autorização baseada em roles.</li>
    <li>Gerenciamento administrativo de usuários.</li>
    <li>Atualização do próprio perfil.</li>
    <li>Foto de usuário armazenada em Base64.</li>
    <li>Criação e gerenciamento de alertas de cotação.</li>
    <li>Notificações de alertas através do SignalR.</li>
    <li>Envio de e-mail quando um alerta é atingido.</li>
    <li>Recuperação de senha por e-mail.</li>
    <li>Token de recuperação de senha com expiração e uso único.</li>
    <li>Health checks dos principais serviços.</li>
    <li>Tratamento global de erros.</li>
    <li>Logging com TraceId.</li>
    <li>Documentação através do Swagger / OpenAPI.</li>
    <li>Testes automatizados unitários e de integração.</li>
    <li>Execução completa da infraestrutura utilizando Docker Compose.</li>
    <li>CI/CD utilizando GitHub Actions.</li>
    <li>Publicação de imagens Docker no GitHub Container Registry.</li>
</ul>

<hr>

<h2>Tecnologias utilizadas</h2>

<h3>Back-end</h3>

<ul>
    <li>.NET 9</li>
    <li>ASP.NET Core Web API</li>
    <li>Entity Framework Core 9</li>
    <li>PostgreSQL 16</li>
    <li>Npgsql</li>
    <li>RabbitMQ</li>
    <li>SignalR</li>
    <li>JWT Bearer Authentication</li>
    <li>Worker Service</li>
    <li>Swagger / OpenAPI</li>
    <li>MailKit</li>
    <li>Docker</li>
    <li>Docker Compose</li>
    <li>GitHub Actions</li>
    <li>GitHub Container Registry</li>
</ul>

<h3>Testes</h3>

<ul>
    <li>xUnit</li>
    <li>Moq</li>
    <li>Testes unitários</li>
    <li>Testes de integração</li>
    <li>Testes de autenticação e autorização</li>
    <li>Testes HTTP da API</li>
    <li>Testes do consumer RabbitMQ</li>
    <li>Testes de retry e shutdown gracioso</li>
</ul>

<hr>

<h2>Arquitetura da solução</h2>

<h3>Br.DollarQuotation.API</h3>

<p>
    Camada de apresentação responsável pelos Controllers,
    Middlewares, autenticação, autorização, SignalR,
    Swagger, consumers e configuração da aplicação.
</p>

<h3>Br.DollarQuotation.Application</h3>

<p>
    Contém serviços, DTOs, interfaces e casos de uso
    da aplicação.
</p>

<h3>Br.DollarQuotation.Domain</h3>

<p>
    Contém entidades, enums, Value Objects e regras
    de negócio.
</p>

<h3>Br.DollarQuotation.Repository</h3>

<p>
    Responsável pelo Entity Framework Core, AppDbContext,
    repositories, migrations e persistência dos dados.
</p>

<h3>Br.DollarQuotation.CrossCutting</h3>

<p>
    Responsável pela configuração de Dependency Injection
    e componentes compartilhados entre as camadas.
</p>

<h3>Br.DollarQuotation.Messaging</h3>

<p>
    Responsável pela infraestrutura de mensageria,
    contratos, publishers e consumers RabbitMQ.
</p>

<h3>Br.DollarQuotation.Worker</h3>

<p>
    Worker Service responsável pela sincronização automática
    das cotações, persistência e publicação dos eventos no RabbitMQ.
</p>

<h3>Br.DollarQuotation.Tests</h3>

<p>
    Projeto responsável pelos testes automatizados da solução,
    incluindo testes unitários, integração, segurança e resiliência.
</p>

<hr>

<h2>Fluxo principal da aplicação</h2>

<pre>
Provedor externo de cotações
          |
          v
Br.DollarQuotation.Worker
          |
          +--------------------> PostgreSQL
          |
          v
       RabbitMQ
          |
          v
Br.DollarQuotation.API
          |
          +--------------------> SignalR
          |                         |
          |                         v
          |                 Câmbio Pulse Web
          |
          +--------------------> Alertas
                                    |
                                    v
                                  E-mail
</pre>

<hr>

<h2>Banco de dados</h2>

<p>
    A aplicação utiliza <strong>PostgreSQL 16</strong> como banco de dados
    relacional.
</p>

<p>
    O banco utilizado pelo projeto é:
</p>

<pre>
dollar_quotation_db
</pre>

<h3>Connection String</h3>

<p>
    Em ambiente local, a configuração pode ser fornecida através de
    <code>appsettings.json</code>, User Secrets ou variáveis de ambiente.
</p>

<pre>
Host=localhost;
Port=5432;
Database=dollar_quotation_db;
Username=postgres;
Password=&lt;sua-senha&gt;
</pre>

<hr>

<h2>Entity Framework Core</h2>

<p>
    O projeto utiliza <strong>Entity Framework Core Migrations</strong>
    para versionamento e criação da estrutura do banco de dados.
</p>

<h3>Migrations atuais</h3>

<pre>
20260807002507_InitialCreate
20260807195721_AddQuotationAlerts
20260810173523_AddPasswordResetTokens
</pre>

<h3>Listar migrations</h3>

<pre>
dotnet ef migrations list --project Br.DollarQuotation.Repository --startup-project Br.DollarQuotation.API
</pre>

<h3>Atualizar banco de dados</h3>

<pre>
dotnet ef database update --project Br.DollarQuotation.Repository --startup-project Br.DollarQuotation.API
</pre>

<h3>Criar nova migration</h3>

<pre>
dotnet ef migrations add NomeDaMigration --project Br.DollarQuotation.Repository --startup-project Br.DollarQuotation.API
</pre>

<hr>

<h2>Cotações</h2>

<p>
    A API permite consultar cotações utilizando pares de moedas.
</p>

<p>Exemplos:</p>

<pre>
USD/BRL
EUR/BRL
GBP/BRL
</pre>

<h3>Cotação atual</h3>

<pre>
GET /api/currency-quotations/current?BaseCurrency=USD&amp;QuoteCurrency=BRL
</pre>

<h3>Resumo da cotação</h3>

<pre>
GET /api/currency-quotations/summary?BaseCurrency=USD&amp;QuoteCurrency=BRL
</pre>

<p>
    O resumo pode disponibilizar informações como:
</p>

<ul>
    <li>Último preço de compra.</li>
    <li>Último preço de venda.</li>
    <li>Preço mínimo.</li>
    <li>Preço máximo.</li>
    <li>Preço médio.</li>
    <li>Variação percentual.</li>
    <li>Data da última cotação.</li>
    <li>Total de cotações encontradas.</li>
</ul>

<h3>Histórico</h3>

<pre>
GET /api/currency-quotations/history
</pre>

<h3>Paginação</h3>

<pre>
GET /api/currency-quotations/paged?page=1&amp;pageSize=10
</pre>

<hr>

<h2>Worker de cotações</h2>

<p>
    O projeto <strong>Br.DollarQuotation.Worker</strong> executa em background
    e é responsável pela sincronização automática das cotações.
</p>

<p>
    O intervalo de execução pode ser configurado através das configurações
    da aplicação.
</p>

<pre>
Worker
   |
   v
Provedor externo
   |
   v
Cotação
   |
   +------> PostgreSQL
   |
   v
RabbitMQ
</pre>

<hr>

<h2>RabbitMQ</h2>

<p>
    A solução utiliza <strong>RabbitMQ</strong> para comunicação assíncrona
    entre o Worker e a API.
</p>

<h3>Configuração</h3>

<pre>
Exchange:
dollarquotation.exchange

Queue:
dollarquotation.quotation.queue

Routing Key:
quotation.updated
</pre>

<h3>Resiliência do Consumer</h3>

<p>
    O consumer RabbitMQ da API possui política de retry com
    <strong>backoff exponencial</strong> quando o broker está
    temporariamente indisponível.
</p>

<pre>
2s → 4s → 8s → 16s → 30s
</pre>

<p>
    Após atingir 30 segundos, o intervalo permanece nesse valor
    até que a conexão seja restabelecida.
</p>

<p>
    Uma indisponibilidade temporária do RabbitMQ não encerra a API.
    Quando o broker volta a ficar disponível, o consumer tenta
    estabelecer a conexão novamente.
</p>

<h3>RabbitMQ Management</h3>

<p>
    Em ambiente Docker local, o painel administrativo fica disponível em:
</p>

<pre>
http://localhost:15672
</pre>

<hr>

<h2>SignalR</h2>

<p>
    A API utiliza <strong>SignalR</strong> para disponibilizar atualizações
    de cotação e alertas em tempo real para o front-end.
</p>

<h3>Hub</h3>

<pre>
/hubs/quotations
</pre>

<p>
    Entre os eventos enviados para o cliente estão:
</p>

<pre>
QuotationUpdated
QuotationAlertTriggered
</pre>

<hr>

<h2>Autenticação e autorização</h2>

<p>
    A autenticação utiliza <strong>JWT Bearer Token</strong>.
</p>

<p>
    Após o login, a API gera um token JWT que deve ser enviado nas
    requisições protegidas.
</p>

<pre>
Authorization: Bearer &lt;token&gt;
</pre>

<h3>Roles</h3>

<p>
    A aplicação possui dois perfis:
</p>

<ul>
    <li><strong>User</strong> — acesso às funcionalidades gerais e ao próprio perfil.</li>
    <li><strong>Admin</strong> — acesso às funcionalidades administrativas de usuários.</li>
</ul>

<h3>Regras administrativas</h3>

<ul>
    <li>Usuários comuns não podem acessar a administração de usuários.</li>
    <li>Usuários comuns não podem alterar a própria role.</li>
    <li>Administradores podem gerenciar outros usuários.</li>
    <li>Um administrador não pode desativar a própria conta através da administração.</li>
    <li>Um administrador não pode remover indevidamente o próprio acesso administrativo.</li>
    <li>A aplicação protege regras relacionadas à disponibilidade de administradores ativos.</li>
</ul>

<h3>Configuração JWT</h3>

<pre>
Jwt__SecretKey
Jwt__Issuer
Jwt__Audience
Jwt__ExpirationInMinutes
</pre>

<p>
    Dados sensíveis, como a chave secreta JWT, nunca devem ser
    armazenados diretamente no código-fonte.
</p>

<hr>

<h2>Usuários</h2>

<p>
    A API possui endpoints administrativos e endpoints específicos
    para o usuário autenticado.
</p>

<h3>Meu perfil</h3>

<pre>
GET   /api/users/me
PUT   /api/users/me
PATCH /api/users/me/photo
</pre>

<h3>Administração</h3>

<pre>
GET   /api/users
GET   /api/users/{id}
POST  /api/users
PUT   /api/users/{id}

PATCH /api/users/{id}/photo
PATCH /api/users/{id}/activate
PATCH /api/users/{id}/deactivate
</pre>

<hr>

<h2>Alertas de cotação</h2>

<p>
    Usuários podem criar alertas para serem notificados quando uma
    cotação atingir determinado preço.
</p>

<p>
    Quando um alerta é disparado:
</p>

<pre>
Cotação recebida
      |
      v
Consumer RabbitMQ
      |
      v
Avaliação dos alertas
      |
      +------> PostgreSQL
      |
      +------> SignalR
      |
      +------> E-mail
</pre>

<hr>

<h2>Recuperação de senha</h2>

<p>
    A aplicação possui fluxo completo de recuperação de senha por e-mail.
</p>

<pre>
Usuário solicita recuperação
        |
        v
POST /api/auth/forgot-password
        |
        v
Token seguro é criado
        |
        v
Token armazenado no PostgreSQL
        |
        v
E-mail de recuperação enviado
        |
        v
Usuário acessa /reset-password
        |
        v
Nova senha é definida
        |
        v
Token é invalidado
</pre>

<p>
    Os tokens possuem:
</p>

<ul>
    <li>Tempo de expiração configurável.</li>
    <li>Uso único.</li>
    <li>Persistência no banco de dados.</li>
</ul>

<h3>Configurações de e-mail</h3>

<pre>
Email__SmtpHost
Email__SmtpPort
Email__SenderName
Email__SenderEmail
Email__Username
Email__Password
</pre>

<p>
    Credenciais SMTP devem ser fornecidas através de variáveis
    de ambiente ou User Secrets e nunca versionadas no Git.
</p>

<hr>

<h2>Tratamento global de erros</h2>

<p>
    A API possui middleware global para tratamento e padronização
    das exceções.
</p>

<p>
    Entre os status HTTP tratados estão:
</p>

<pre>
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
500 Internal Server Error
</pre>

<hr>

<h2>Logging</h2>

<p>
    Requisições e processos importantes possuem logging estruturado.
</p>

<p>
    Os logs incluem informações como:
</p>

<ul>
    <li>Método HTTP.</li>
    <li>Endpoint.</li>
    <li>Status HTTP.</li>
    <li>Tempo de execução.</li>
    <li>TraceId.</li>
    <li>Processamento do Worker.</li>
    <li>Mensageria RabbitMQ.</li>
    <li>Alertas e notificações.</li>
</ul>

<hr>

<h2>Health Check</h2>

<p>
    A API disponibiliza endpoint de health check:
</p>

<pre>
GET /health
</pre>

<p>
    No Docker Compose, esse endpoint é utilizado para determinar
    se a API está saudável antes de iniciar serviços dependentes.
</p>

<hr>

<h2>Docker</h2>

<p>
    A solução possui suporte a containers Docker para execução
    completa da infraestrutura.
</p>

<h3>Containers</h3>

<ul>
    <li>
        <strong>dollarquotation-api</strong><br>
        ASP.NET Core API<br>
        Porta: <code>8080</code>
    </li>

    <br>

    <li>
        <strong>dollarquotation-postgres</strong><br>
        PostgreSQL 16<br>
        Porta: <code>5432</code>
    </li>

    <br>

    <li>
        <strong>dollarquotation-rabbitmq</strong><br>
        RabbitMQ<br>
        Porta AMQP: <code>5672</code><br>
        Management: <code>15672</code>
    </li>

    <br>

    <li>
        <strong>dollarquotation-worker</strong><br>
        Worker Service responsável pela sincronização das cotações.<br>
        Não expõe porta HTTP.
    </li>

    <br>

    <li>
        <strong>dollarquotation-web</strong><br>
        Angular + Nginx<br>
        Porta: <code>4200</code>
    </li>
</ul>

<h2>Configuração de ambiente</h2>

<p>
    O projeto utiliza um arquivo <code>.env</code> para fornecer
    informações sensíveis ao Docker Compose.
</p>

<p>
    Crie o arquivo a partir do exemplo:
</p>

<h3>Windows</h3>

<pre>
copy .env.example .env
</pre>

<h3>Linux / macOS</h3>

<pre>
cp .env.example .env
</pre>

<h3>Variáveis necessárias</h3>

<pre>
POSTGRES_DB=
POSTGRES_USER=
POSTGRES_PASSWORD=

JWT_SECRET_KEY=

EMAIL_PASSWORD=

RABBITMQ_USER=
RABBITMQ_PASSWORD=
</pre>

<p>
    O arquivo <code>.env</code> não deve ser versionado.
</p>

<hr>

<h2>Executando com Docker</h2>

<h3>Subir a infraestrutura</h3>

<pre>
docker compose up -d
</pre>

<h3>Verificar containers</h3>

<pre>
docker compose ps
</pre>

<p>
    O estado esperado é semelhante a:
</p>

<pre>
API          healthy
PostgreSQL   healthy
RabbitMQ     healthy
Frontend     healthy
Worker       running
</pre>

<h3>Logs da API</h3>

<pre>
docker compose logs api --tail=100
</pre>

<h3>Logs do Worker</h3>

<pre>
docker compose logs worker --tail=100
</pre>

<h3>Acompanhar logs</h3>

<pre>
docker compose logs -f
</pre>

<h3>Parar a infraestrutura</h3>

<pre>
docker compose down
</pre>

<hr>

<h2>Executando sem Docker</h2>

<h3>Pré-requisitos</h3>

<ul>
    <li>.NET SDK 9</li>
    <li>PostgreSQL 16</li>
    <li>RabbitMQ</li>
    <li>Visual Studio 2022 ou VS Code</li>
</ul>

<h3>Restaurar dependências</h3>

<pre>
dotnet restore
</pre>

<h3>Compilar</h3>

<pre>
dotnet build
</pre>

<h3>Atualizar banco</h3>

<pre>
dotnet ef database update --project Br.DollarQuotation.Repository --startup-project Br.DollarQuotation.API
</pre>

<h3>Executar API</h3>

<pre>
dotnet run --project Br.DollarQuotation.API
</pre>

<h3>Executar Worker</h3>

<pre>
dotnet run --project Br.DollarQuotation.Worker
</pre>

<hr>

<h2>Swagger</h2>

<p>
    A documentação dos endpoints pode ser acessada através do Swagger.
</p>

<p>
    Utilizando Docker:
</p>

<pre>
http://localhost:8080/swagger/index.html
</pre>

<p>
    O Swagger possui suporte à autenticação JWT para testes
    dos endpoints protegidos.
</p>

<hr>

<h2>Testes automatizados</h2>

<p>
    A solução possui uma suíte automatizada cobrindo domínio,
    aplicação, infraestrutura e endpoints HTTP.
</p>

<ul>
    <li>Regras de domínio.</li>
    <li>Value Objects.</li>
    <li>Serviços de aplicação.</li>
    <li>Autenticação JWT.</li>
    <li>Autorização por role.</li>
    <li>Gerenciamento de usuários.</li>
    <li>Alertas de cotação.</li>
    <li>Controllers.</li>
    <li>Testes de integração.</li>
    <li>Consumer RabbitMQ.</li>
    <li>Retry do consumer.</li>
    <li>Shutdown gracioso do BackgroundService.</li>
</ul>

<h3>Executar testes</h3>

<pre>
dotnet test --configuration Release
</pre>

<hr>

<h2>CI/CD</h2>

<p>
    A solução utiliza <strong>GitHub Actions</strong> para integração
    contínua e publicação das imagens Docker.
</p>

<h3>Backend CI</h3>

<p>
    O pipeline realiza:
</p>

<ul>
    <li>Checkout do código.</li>
    <li>Setup do .NET 9.</li>
    <li>Restore das dependências.</li>
    <li>Build Release.</li>
    <li>Inicialização de PostgreSQL 16 para os testes de integração.</li>
    <li>Execução automatizada dos testes.</li>
</ul>

<h3>Docker Publish</h3>

<p>
    As imagens da API e Worker são construídas e publicadas
    automaticamente no GitHub Container Registry.
</p>

<pre>
ghcr.io/leohslopes/br-dollarquotation-api:latest
ghcr.io/leohslopes/br-dollarquotation-worker:latest
</pre>

<p>
    Também são geradas tags baseadas no commit e nas versões
    publicadas através de tags Git.
</p>

<hr>

<h2>Segurança</h2>

<ul>
    <li>Autenticação utilizando JWT.</li>
    <li>Autorização baseada em roles.</li>
    <li>Endpoints administrativos protegidos.</li>
    <li>Senhas armazenadas através de hash.</li>
    <li>Segredos externos ao código-fonte.</li>
    <li><code>.env</code> ignorado pelo Git.</li>
    <li><code>.env.example</code> sem credenciais reais.</li>
    <li>Tokens de recuperação de senha com expiração.</li>
    <li>Tokens de recuperação de senha de uso único.</li>
    <li>Credenciais SMTP fornecidas através de configuração segura.</li>
    <li>Tratamento global de exceções.</li>
</ul>

<hr>

<h2>Estrutura de execução</h2>

<pre>
                    +-------------------------+
                    |     Câmbio Pulse Web    |
                    |     Angular / Nginx     |
                    +------------+------------+
                                 |
                                 v
                    +-------------------------+
                    | Br.DollarQuotation.API  |
                    +--------+-----------+----+
                             |           |
                      SignalR|           | RabbitMQ
                             |           |
                             v           |
                          Front-end      |
                                         |
                                         ^
                                         |
                              +----------+----------+
                              | Quotation Worker    |
                              +----------+----------+
                                         |
                                         v
                               Provedor de Câmbio
                                         |
                                         v
                                    PostgreSQL
</pre>

<hr>

<h2>Imagens Docker</h2>

<h3>API</h3>

<pre>
ghcr.io/leohslopes/br-dollarquotation-api:latest
</pre>

<h3>Worker</h3>

<pre>
ghcr.io/leohslopes/br-dollarquotation-worker:latest
</pre>

<h3>Frontend</h3>

<pre>
ghcr.io/leohslopes/br-dollarquotation-web:latest
</pre>

<hr>

<h2>Melhorias futuras</h2>

<ul>
    <li>Cache distribuído utilizando Redis.</li>
    <li>Deploy da aplicação em ambiente Cloud.</li>
    <li>Observabilidade e métricas.</li>
    <li>Centralização de logs.</li>
    <li>Expansão dos provedores de cotação.</li>
    <li>Health check específico do Worker.</li>
    <li>Execução dos containers com políticas adicionais de segurança.</li>
    <li>Ampliação contínua da cobertura de testes automatizados.</li>
</ul>

<hr>

<h2>Projeto</h2>

<p>
    <strong>Câmbio Pulse</strong><br>
    Plataforma para acompanhamento de cotações de moedas,
    histórico, alertas e atualizações em tempo real.
</p>

<p>
    Back-end desenvolvido em <strong>.NET 9</strong> com PostgreSQL,
    RabbitMQ, SignalR, Worker Services e Docker.
</p>

</body>

</html>
