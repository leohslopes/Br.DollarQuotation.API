<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>

<body>

<h1>Br.DollarQuotation API — Câmbio Pulse</h1>

<p>
    API de consulta e gerenciamento de cotações de moedas desenvolvida em
    <strong>.NET 9</strong>, utilizando conceitos de
    <strong>Clean Architecture, SOLID, Entity Framework Core, PostgreSQL,
    RabbitMQ, Worker Services e comunicação em tempo real</strong>.
</p>

<p>
    O projeto faz parte da plataforma <strong>Câmbio Pulse</strong> e tem como
    objetivo fornecer cotações de moedas em relação ao Real, armazenar o
    histórico das cotações, gerenciar usuários e alertas de preço, além de
    disponibilizar atualização automática das informações por meio de
    processamento em background.
</p>


<h2>Principais funcionalidades</h2>

<ul>
    <li>Consulta de cotações de moedas.</li>
    <li>Suporte a múltiplos pares de moedas.</li>
    <li>Histórico de cotações.</li>
    <li>Resumo das cotações por período.</li>
    <li>Atualização automática das cotações.</li>
    <li>Integração com provedor externo de câmbio.</li>
    <li>Processamento em background através de Worker Service.</li>
    <li>Mensageria utilizando RabbitMQ.</li>
    <li>Atualização em tempo real utilizando SignalR.</li>
    <li>Cadastro e autenticação de usuários.</li>
    <li>Autenticação e autorização utilizando JWT.</li>
    <li>Gerenciamento de usuários.</li>
    <li>Foto de usuário armazenada em Base64.</li>
    <li>Criação e gerenciamento de alertas de cotação.</li>
    <li>Recuperação de senha por e-mail.</li>
    <li>Token de recuperação de senha com expiração e uso único.</li>
    <li>Documentação e testes dos endpoints através do Swagger.</li>
    <li>Execução completa da infraestrutura utilizando Docker Compose.</li>
</ul>


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
</ul>


<h3>Testes</h3>

<ul>
    <li>xUnit</li>
    <li>Testes unitários</li>
</ul>


<h2>Arquitetura da solução</h2>

<table>
    <thead>
        <tr>
            <th>Projeto</th>
            <th>Responsabilidade</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td><strong>Br.DollarQuotation.API</strong></td>
            <td>
                Camada de apresentação responsável pelos Controllers,
                Middlewares, autenticação, SignalR, Swagger e configuração
                da aplicação.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.Application</strong></td>
            <td>
                Contém os serviços, DTOs, interfaces e casos de uso
                da aplicação.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.Domain</strong></td>
            <td>
                Contém entidades, enums, objetos de domínio e regras
                de negócio.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.Repository</strong></td>
            <td>
                Responsável pelo Entity Framework Core, AppDbContext,
                repositories, migrations e integrações de persistência.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.CrossCutting</strong></td>
            <td>
                Responsável pela configuração de Dependency Injection
                e componentes compartilhados entre as camadas.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.Messaging</strong></td>
            <td>
                Responsável pela infraestrutura de mensageria utilizando
                RabbitMQ.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.Worker</strong></td>
            <td>
                Worker Service responsável pela atualização automática
                das cotações e comunicação com os demais componentes
                da aplicação.
            </td>
        </tr>
        <tr>
            <td><strong>Br.DollarQuotation.Tests</strong></td>
            <td>
                Projeto responsável pelos testes automatizados da solução.
            </td>
        </tr>
    </tbody>
</table>


<h2>Fluxo principal da aplicação</h2>

<pre>
Provedor de Cotações
        |
        v
Br.DollarQuotation.Worker
        |
        v
PostgreSQL
        |
        v
RabbitMQ
        |
        v
Br.DollarQuotation.API
        |
        v
SignalR
        |
        v
Câmbio Pulse Web
</pre>


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
    Em ambiente local, a configuração pode ser realizada através do
    <code>appsettings.json</code>, User Secrets ou variáveis de ambiente.
</p>

<pre>
Host=localhost;
Port=5432;
Database=dollar_quotation_db;
Username=postgres;
Password=&lt;sua-senha&gt;
</pre>


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


<h2>Cotações</h2>

<p>
    A API permite consultar cotações utilizando pares de moedas.
</p>

<p>
    Exemplos:
</p>

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
    O resumo disponibiliza informações como:
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
   v
PostgreSQL
   |
   v
RabbitMQ
</pre>


<h2>RabbitMQ</h2>

<p>
    A solução utiliza <strong>RabbitMQ</strong> para comunicação assíncrona
    entre os componentes.
</p>

<p>
    Configuração utilizada:
</p>

<pre>
Exchange:
dollarquotation.exchange

Queue:
dollarquotation.quotation.queue

Routing Key:
quotation.updated
</pre>

<p>
    O painel administrativo do RabbitMQ fica disponível, em ambiente Docker,
    através da porta:
</p>

<pre>
15672
</pre>


<h2>SignalR</h2>

<p>
    A API utiliza <strong>SignalR</strong> para disponibilizar atualizações
    de cotações em tempo real para o front-end.
</p>

<pre>
/hubs/quotations
</pre>


<h2>Autenticação</h2>

<p>
    A autenticação da aplicação utiliza <strong>JWT Bearer Token</strong>.
</p>

<p>
    Após o login, a API gera um token JWT que deve ser enviado nas
    requisições protegidas.
</p>

<pre>
Authorization: Bearer &lt;token&gt;
</pre>


<h3>Configuração JWT</h3>

<pre>
Jwt__SecretKey
Jwt__Issuer
Jwt__Audience
Jwt__ExpirationInMinutes
</pre>

<p>
    Dados sensíveis, como a chave secreta do JWT, não devem ser armazenados
    diretamente no código-fonte ou enviados ao repositório.
</p>


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
    Credenciais SMTP devem ser fornecidas através de variáveis de ambiente
    ou User Secrets e nunca versionadas no Git.
</p>


<h2>Docker</h2>

<p>
    A solução possui suporte a containers Docker para execução da
    infraestrutura completa.
</p>

<p>
    Os principais containers são:
</p>

<table>
    <thead>
        <tr>
            <th>Container</th>
            <th>Serviço</th>
            <th>Porta</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>dollarquotation-api</td>
            <td>ASP.NET Core API</td>
            <td>8080</td>
        </tr>
        <tr>
            <td>dollarquotation-postgres</td>
            <td>PostgreSQL</td>
            <td>5432</td>
        </tr>
        <tr>
            <td>dollarquotation-rabbitmq</td>
            <td>RabbitMQ</td>
            <td>5672 / 15672</td>
        </tr>
        <tr>
            <td>dollarquotation-worker</td>
            <td>Worker Service</td>
            <td>-</td>
        </tr>
        <tr>
            <td>dollarquotation-web</td>
            <td>Angular / Nginx</td>
            <td>4200</td>
        </tr>
    </tbody>
</table>


<h3>Subir a infraestrutura</h3>

<pre>
docker compose up -d
</pre>


<h3>Verificar containers</h3>

<pre>
docker compose ps
</pre>


<h3>Visualizar logs da API</h3>

<pre>
docker compose logs api --tail=100
</pre>


<h3>Visualizar logs do Worker</h3>

<pre>
docker compose logs worker --tail=100
</pre>


<h3>Parar os containers</h3>

<pre>
docker compose down
</pre>


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


<h3>Executar API</h3>

<pre>
dotnet run --project Br.DollarQuotation.API
</pre>


<h3>Executar Worker</h3>

<pre>
dotnet run --project Br.DollarQuotation.Worker
</pre>


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
    O Swagger também possui suporte à autenticação JWT para testes dos
    endpoints protegidos.
</p>


<h2>Tratamento global de erros</h2>

<p>
    A API possui middleware global para tratamento e padronização de
    exceções, evitando que detalhes internos da aplicação sejam expostos
    diretamente ao cliente.
</p>


<h2>Logging</h2>

<p>
    As requisições e principais processos da aplicação possuem logging,
    permitindo acompanhar operações da API, Worker, integrações externas
    e processamento das cotações.
</p>


<h2>Segurança</h2>

<ul>
    <li>Autenticação utilizando JWT.</li>
    <li>Endpoints protegidos por autorização.</li>
    <li>Senhas armazenadas através de hash.</li>
    <li>Segredos externos ao código-fonte.</li>
    <li>Tokens de recuperação de senha com expiração.</li>
    <li>Tokens de recuperação de senha de uso único.</li>
    <li>Credenciais SMTP fornecidas através de configuração segura.</li>
</ul>


<h2>Testes</h2>

<p>
    Para executar os testes automatizados:
</p>

<pre>
dotnet test
</pre>


<h2>Estrutura de execução</h2>

<pre>
                 +----------------------+
                 |    Câmbio Pulse Web  |
                 |    Angular / Nginx   |
                 +----------+-----------+
                            |
                            v
                 +----------------------+
                 | Br.DollarQuotation   |
                 |        API           |
                 +----+------------+----+
                      |            |
               SignalR|            |RabbitMQ
                      |            |
                      v            v
                Front-end      Mensageria
                                   ^
                                   |
                         +---------+---------+
                         | Quotation Worker  |
                         +---------+---------+
                                   |
                                   v
                         Provedor de Câmbio
                                   |
                                   v
                              PostgreSQL
</pre>


<h2>Melhorias futuras</h2>

<ul>
    <li>Cache distribuído utilizando Redis.</li>
    <li>Pipeline CI/CD completo.</li>
    <li>Deploy da aplicação em ambiente Cloud.</li>
    <li>Observabilidade e métricas.</li>
    <li>Centralização de logs.</li>
    <li>Expansão dos provedores de cotação.</li>
    <li>Ampliação da cobertura de testes automatizados.</li>
</ul>


<h2>Projeto</h2>

<p>
    <strong>Câmbio Pulse</strong><br>
    Plataforma para acompanhamento de cotações de moedas, histórico,
    alertas e atualizações em tempo real.
</p>

</body>
</html>
