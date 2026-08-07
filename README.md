<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Br.DollarQuotation.API - README</title>
  <style>
    :root {
      --blue: #0b4b7d;
      --blue-dark: #062f50;
      --orange: #ec7000;
      --orange-light: #ff8a1f;
      --text: #24384b;
      --muted: #6f7f8f;
      --border: #e3e8ed;
      --bg: #f6f8fb;
      --card: #ffffff;
      --code: #0f2233;
      --success: #16713a;
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      font-family: "Segoe UI", Arial, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }

    .hero {
      padding: 56px 24px;
      background: linear-gradient(135deg, var(--blue-dark), var(--blue));
      color: #fff;
      border-bottom: 5px solid var(--orange);
    }

    .hero-inner,
    .container {
      width: min(1100px, calc(100% - 32px));
      margin: 0 auto;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 14px;
      margin-bottom: 18px;
    }

    .brand-icon {
      width: 54px;
      height: 54px;
      display: grid;
      place-items: center;
      border-radius: 14px 6px 14px 6px;
      background: linear-gradient(135deg, var(--orange-light), var(--orange));
      font-weight: 800;
      box-shadow: 0 10px 28px rgba(0,0,0,.2);
    }

    h1 {
      margin: 0;
      font-size: 34px;
      line-height: 1.15;
    }

    .subtitle {
      margin-top: 12px;
      max-width: 820px;
      color: rgba(255,255,255,.84);
      font-size: 15px;
    }

    .badges {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 20px;
    }

    .badge {
      padding: 6px 10px;
      border: 1px solid rgba(255,255,255,.18);
      border-radius: 999px;
      background: rgba(255,255,255,.08);
      font-size: 12px;
      font-weight: 700;
    }

    .container {
      padding: 28px 0 56px;
    }

    section {
      margin: 22px 0;
      padding: 24px;
      background: var(--card);
      border: 1px solid var(--border);
      border-radius: 16px;
      box-shadow: 0 6px 24px rgba(0,31,60,.045);
    }

    h2 {
      margin: 0 0 14px;
      color: var(--blue-dark);
      font-size: 22px;
    }

    h3 {
      margin: 22px 0 10px;
      color: var(--blue);
      font-size: 16px;
    }

    p {
      margin: 10px 0;
    }

    ul {
      margin: 10px 0 10px 22px;
    }

    li {
      margin: 5px 0;
    }

    .accent {
      color: var(--orange);
      font-weight: 800;
    }

    .note {
      margin: 14px 0;
      padding: 13px 15px;
      border-left: 4px solid var(--orange);
      border-radius: 8px;
      background: #fff8f1;
      color: #60452f;
    }

    .success {
      border-left-color: var(--success);
      background: #eefaf2;
      color: #275d3b;
    }

    pre {
      margin: 14px 0;
      padding: 16px;
      overflow-x: auto;
      border-radius: 12px;
      background: var(--code);
      color: #eef6ff;
      font-family: Consolas, "Courier New", monospace;
      font-size: 13px;
      line-height: 1.5;
    }

    code {
      font-family: Consolas, "Courier New", monospace;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 14px;
      overflow: hidden;
      border-radius: 10px;
    }

    th, td {
      padding: 12px 14px;
      border-bottom: 1px solid var(--border);
      text-align: left;
      vertical-align: top;
      font-size: 13px;
    }

    th {
      background: #f3f6f9;
      color: var(--blue-dark);
      font-weight: 800;
    }

    tr:last-child td {
      border-bottom: 0;
    }

    a {
      color: var(--orange);
      font-weight: 700;
      text-decoration: none;
    }

    a:hover {
      text-decoration: underline;
    }

    .footer {
      text-align: center;
      color: var(--muted);
      font-size: 12px;
      padding-top: 18px;
    }

    @media (max-width: 700px) {
      h1 {
        font-size: 28px;
      }

      section {
        padding: 18px;
      }

      th, td {
        padding: 10px;
      }
    }
  </style>
</head>
<body>

  <header class="hero">
    <div class="hero-inner">
      <div class="brand">
        <div class="brand-icon">CP</div>
        <div>
          <h1>Br.DollarQuotation.API</h1>
          <div class="subtitle">
            Backend do Câmbio Pulse para consulta, armazenamento, histórico e atualização em tempo real de cotações de moedas.
          </div>
        </div>
      </div>

      <div class="badges">
        <span class="badge">.NET 9</span>
        <span class="badge">ASP.NET Core Web API</span>
        <span class="badge">PostgreSQL</span>
        <span class="badge">Entity Framework Core</span>
        <span class="badge">JWT</span>
        <span class="badge">SignalR</span>
        <span class="badge">Worker Service</span>
        <span class="badge">xUnit</span>
      </div>
    </div>
  </header>

  <main class="container">

    <section>
      <h2>Sobre o projeto</h2>
      <p>
        O <strong>Br.DollarQuotation.API</strong> é o backend do sistema <strong>Câmbio Pulse</strong>.
        A solução centraliza autenticação de usuários, consulta de cotações, histórico,
        resumos estatísticos, persistência em PostgreSQL, processamento periódico por Worker
        e comunicação em tempo real com o front-end através de SignalR.
      </p>

      <p>
        As cotações são obtidas por um provedor externo, persistidas pela aplicação e
        notificadas para clientes conectados quando novas informações são processadas.
      </p>
    </section>

    <section>
      <h2>Arquitetura da solution</h2>

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
            <td>Controllers, autenticação HTTP, middlewares, filtros, Hub SignalR e endpoints públicos/internos.</td>
          </tr>
          <tr>
            <td><strong>Br.DollarQuotation.Application</strong></td>
            <td>Casos de uso, DTOs, contratos de serviços e regras de aplicação.</td>
          </tr>
          <tr>
            <td><strong>Br.DollarQuotation.Domain</strong></td>
            <td>Entidades, Value Objects, exceções, enums e contratos de domínio.</td>
          </tr>
          <tr>
            <td><strong>Br.DollarQuotation.Repository</strong></td>
            <td>Entity Framework Core, PostgreSQL, repositories, serviços de infraestrutura e integração com AwesomeAPI.</td>
          </tr>
          <tr>
            <td><strong>Br.DollarQuotation.CrossCutting</strong></td>
            <td>Registro centralizado de dependências e configuração de IoC.</td>
          </tr>
          <tr>
            <td><strong>Br.DollarQuotation.Worker</strong></td>
            <td>Busca periódica de cotações e envio de notificações internas para a API.</td>
          </tr>
          <tr>
            <td><strong>Br.DollarQuotation.Tests</strong></td>
            <td>Testes de domínio, aplicação, infraestrutura e integração.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section>
      <h2>Principais tecnologias</h2>
      <ul>
        <li>.NET 9 / C#</li>
        <li>ASP.NET Core Web API</li>
        <li>Entity Framework Core</li>
        <li>PostgreSQL</li>
        <li>JWT Bearer Authentication</li>
        <li>ASP.NET Core SignalR</li>
        <li>Worker Service</li>
        <li>HttpClient</li>
        <li>AwesomeAPI para cotações</li>
        <li>xUnit</li>
      </ul>
    </section>

    <section>
      <h2>Pré-requisitos</h2>
      <ul>
        <li>.NET SDK 9 instalado.</li>
        <li>PostgreSQL disponível localmente ou em ambiente remoto.</li>
        <li>Visual Studio 2022, Visual Studio Code ou IDE compatível.</li>
        <li>Acesso HTTPS local para execução da API.</li>
      </ul>

      <pre>dotnet --version</pre>

      <p>O resultado deve indicar uma versão compatível com .NET 9.</p>
    </section>

    <section>
      <h2>Configuração de segurança</h2>

      <div class="note">
        Nunca publique senhas, JWT Secret, API Keys ou connection strings com credenciais reais no GitHub.
        Utilize <strong>User Secrets</strong>, variáveis de ambiente ou um serviço de gerenciamento de segredos.
      </div>

      <h3>Inicializar User Secrets</h3>

      <pre>dotnet user-secrets init --project .\Br.DollarQuotation.API\Br.DollarQuotation.API.csproj</pre>

      <h3>Connection String</h3>

      <pre>dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=BrDollarQuotation;Username=postgres;Password=SUA_SENHA" --project .\Br.DollarQuotation.API\Br.DollarQuotation.API.csproj</pre>

      <h3>JWT</h3>

      <pre>dotnet user-secrets set "Jwt:SecretKey" "SUA_CHAVE_JWT_COM_PELO_MENOS_32_CARACTERES" --project .\Br.DollarQuotation.API\Br.DollarQuotation.API.csproj</pre>

      <p>
        Também configure <code>Jwt:Issuer</code>, <code>Jwt:Audience</code> e
        <code>Jwt:ExpirationInMinutes</code> conforme o ambiente.
      </p>
    </section>

    <section>
      <h2>Exemplo de appsettings.json da API</h2>

      <pre>{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },

  "Jwt": {
    "SecretKey": "",
    "Issuer": "Br.DollarQuotation.API",
    "Audience": "Br.DollarQuotation.Web",
    "ExpirationInMinutes": 60
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "AllowedHosts": "*"
}</pre>

      <div class="note">
        Os valores acima são exemplos de estrutura. Mantenha os valores sensíveis fora do arquivo versionado.
      </div>
    </section>

    <section>
      <h2>Configuração do Worker</h2>

      <p>
        O Worker realiza consultas periódicas ao provedor de cotações e envia a atualização para a API.
      </p>

      <pre>{
  "AwesomeApi": {
    "BaseUrl": "https://economia.awesomeapi.com.br/"
  },

  "QuotationWorker": {
    "Enabled": true,
    "IntervalInSeconds": 60,
    "DelayBetweenRequestsInMilliseconds": 1000,
    "CurrencyPairs": [
      "USD-BRL",
      "EUR-BRL",
      "GBP-BRL"
    ]
  },

  "InternalApi": {
    "BaseUrl": "https://localhost:7123"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
      "System.Net.Http.HttpClient": "Warning"
    }
  }
}</pre>

      <p>
        A chave utilizada para proteger a comunicação interna entre Worker e API deve ser armazenada
        fora do repositório.
      </p>
    </section>

    <section>
      <h2>Banco de dados</h2>

      <p>
        O projeto utiliza <strong>PostgreSQL</strong> com Entity Framework Core.
        As migrations estão no projeto <code>Br.DollarQuotation.Repository</code>.
      </p>

      <h3>Aplicar migrations</h3>

      <pre>dotnet ef database update --project .\Br.DollarQuotation.Repository\Br.DollarQuotation.Repository.csproj --startup-project .\Br.DollarQuotation.API\Br.DollarQuotation.API.csproj</pre>

      <h3>Criar nova migration</h3>

      <pre>dotnet ef migrations add NomeDaMigration --project .\Br.DollarQuotation.Repository\Br.DollarQuotation.Repository.csproj --startup-project .\Br.DollarQuotation.API\Br.DollarQuotation.API.csproj</pre>
    </section>

    <section>
      <h2>Executando a aplicação</h2>

      <h3>API</h3>

      <pre>dotnet run --project .\Br.DollarQuotation.API\Br.DollarQuotation.API.csproj</pre>

      <p>
        Durante o desenvolvimento, o front-end foi configurado para consumir a API em:
      </p>

      <pre>https://localhost:7123</pre>

      <h3>Worker</h3>

      <pre>dotnet run --project .\Br.DollarQuotation.Worker\Br.DollarQuotation.Worker.csproj</pre>

      <div class="success note">
        Para validar o fluxo completo em tempo real, deixe API, Worker e front-end Angular executando simultaneamente.
      </div>
    </section>

    <section>
      <h2>Autenticação</h2>

      <p>
        A API utiliza JWT. Após o login, o token deve ser enviado nos endpoints protegidos:
      </p>

      <pre>Authorization: Bearer SEU_TOKEN</pre>

      <h3>Login</h3>

      <pre>POST /api/auth/login</pre>

      <h3>Exemplo</h3>

      <pre>{
  "email": "usuario@exemplo.com",
  "password": "SuaSenha"
}</pre>
    </section>

    <section>
      <h2>Endpoints de usuários</h2>

      <table>
        <thead>
          <tr>
            <th>Método</th>
            <th>Endpoint</th>
            <th>Descrição</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>POST</td>
            <td><code>/api/users</code></td>
            <td>Cadastra um usuário.</td>
          </tr>
          <tr>
            <td>GET</td>
            <td><code>/api/users?page=1&amp;pageSize=10</code></td>
            <td>Lista usuários com paginação.</td>
          </tr>
          <tr>
            <td>GET</td>
            <td><code>/api/users/{id}</code></td>
            <td>Obtém usuário por ID.</td>
          </tr>
          <tr>
            <td>PUT</td>
            <td><code>/api/users/{id}</code></td>
            <td>Atualiza nome e e-mail.</td>
          </tr>
          <tr>
            <td>PATCH</td>
            <td><code>/api/users/{id}/photo</code></td>
            <td>Atualiza a foto do usuário, quando o endpoint estiver habilitado no controller.</td>
          </tr>
          <tr>
            <td>PATCH</td>
            <td><code>/api/users/{id}/activate</code></td>
            <td>Ativa um usuário.</td>
          </tr>
          <tr>
            <td>PATCH</td>
            <td><code>/api/users/{id}/deactivate</code></td>
            <td>Desativa um usuário.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section>
      <h2>Endpoints de cotações</h2>

      <table>
        <thead>
          <tr>
            <th>Método</th>
            <th>Endpoint</th>
            <th>Descrição</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>GET</td>
            <td><code>/api/currency-quotations/current</code></td>
            <td>Obtém a cotação atual de um par de moedas.</td>
          </tr>
          <tr>
            <td>GET</td>
            <td><code>/api/currency-quotations/history</code></td>
            <td>Consulta o histórico por moeda e período.</td>
          </tr>
          <tr>
            <td>GET</td>
            <td><code>/api/currency-quotations/summary</code></td>
            <td>Retorna resumo estatístico do período.</td>
          </tr>
          <tr>
            <td>GET</td>
            <td><code>/api/currency-quotations/paged</code></td>
            <td>Retorna cotações paginadas.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section>
      <h2>SignalR</h2>

      <p>
        O Hub responsável pelas cotações em tempo real é:
      </p>

      <pre>/hubs/quotations</pre>

      <p>O Hub exige autenticação JWT.</p>

      <p>Evento enviado para os clientes:</p>

      <pre>QuotationUpdated</pre>

      <p>
        O fluxo de atualização em tempo real é:
      </p>

      <pre>Worker
  ↓
InternalQuotationNotificationsController
  ↓
QuotationNotificationService
  ↓
QuotationHub
  ↓
QuotationUpdated
  ↓
Cliente Angular</pre>
    </section>

    <section>
      <h2>Endpoint interno do Worker</h2>

      <p>
        O Worker envia as novas cotações para:
      </p>

      <pre>POST /api/internal/quotation-notifications</pre>

      <p>
        Esse endpoint utiliza o filtro <code>InternalApiKey</code> e não deve ser exposto sem proteção.
      </p>
    </section>

    <section>
      <h2>Testes</h2>

      <p>
        O projeto possui testes de domínio, aplicação, infraestrutura e integração.
      </p>

      <h3>Executar todos os testes</h3>

      <pre>dotnet test .\Br.DollarQuotation.Tests\Br.DollarQuotation.Tests.csproj</pre>

      <p>
        Durante o desenvolvimento, a suíte chegou ao estado:
      </p>

      <pre>Total: 24
Failed: 0
Succeeded: 24
Skipped: 0</pre>

      <h3>Executar testes específicos</h3>

      <pre>dotnet test .\Br.DollarQuotation.Tests\Br.DollarQuotation.Tests.csproj --filter "AuthControllerIntegrationTests"</pre>
    </section>

    <section>
      <h2>Estrutura de domínio</h2>

      <p>Entre os principais componentes estão:</p>

      <ul>
        <li><code>User</code></li>
        <li><code>CurrencyQuotation</code></li>
        <li><code>Email</code></li>
        <li><code>CurrencyPair</code></li>
        <li><code>ICurrencyQuotationRepository</code></li>
        <li><code>IUserRepository</code></li>
        <li><code>ICurrencyQuotationProvider</code></li>
        <li><code>IPasswordHasher</code></li>
        <li><code>ITokenService</code></li>
      </ul>
    </section>

    <section>
      <h2>Boas práticas utilizadas</h2>

      <ul>
        <li>Separação por camadas.</li>
        <li>Inversão de dependência através de interfaces.</li>
        <li>Injeção de dependências centralizada.</li>
        <li>Entidades com regras de domínio encapsuladas.</li>
        <li>Value Objects para conceitos de domínio.</li>
        <li>Tratamento global de exceções.</li>
        <li>Autenticação JWT.</li>
        <li>Segredos fora do código-fonte.</li>
        <li>Comunicação em tempo real com SignalR.</li>
        <li>Testes automatizados.</li>
      </ul>
    </section>

    <section>
      <h2>Repositório</h2>

      <p>
        GitHub:
        <a href="https://github.com/leohslopes/Br.DollarQuotation.API">
          https://github.com/leohslopes/Br.DollarQuotation.API
        </a>
      </p>
    </section>

    <div class="footer">
      Br.DollarQuotation.API • Backend do Câmbio Pulse
    </div>

  </main>

</body>
</html># Br.DollarQuotation.API