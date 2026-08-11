<div align="center">

# 💱 Br.DollarQuotation.API

### Backend do Câmbio Pulse

API para consulta, armazenamento e atualização em tempo real de cotações de moedas.

<br>

<img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet">
<img src="https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?style=for-the-badge&logo=dotnet">
<img src="https://img.shields.io/badge/PostgreSQL-Database-4169E1?style=for-the-badge&logo=postgresql">
<img src="https://img.shields.io/badge/SignalR-Real_Time-512BD4?style=for-the-badge">
<img src="https://img.shields.io/badge/JWT-Authentication-000000?style=for-the-badge&logo=jsonwebtokens">

</div>

---

## 📌 Sobre o projeto

O **Br.DollarQuotation.API** é o backend do sistema **Câmbio Pulse**.

A aplicação foi desenvolvida utilizando **.NET 9**, seguindo uma arquitetura em camadas e princípios de **SOLID**, com responsabilidades bem definidas entre domínio, aplicação, infraestrutura, API e processamento em background.

Entre as principais funcionalidades estão:

- 💵 Consulta de cotações de moedas
- 📊 Histórico de cotações
- 📈 Resumo de mínima, máxima e média
- 🔄 Atualização automática através de Worker
- ⚡ Atualização em tempo real utilizando SignalR
- 🔐 Autenticação utilizando JWT
- 👤 Gerenciamento de usuários
- 🗄️ Persistência utilizando PostgreSQL
- 🧪 Testes automatizados

---

## 🏗️ Arquitetura

| Projeto | Responsabilidade |
|---|---|
| **Br.DollarQuotation.API** | Controllers, Middlewares, Filters, SignalR e configuração HTTP |
| **Br.DollarQuotation.Application** | Casos de uso, DTOs e serviços da aplicação |
| **Br.DollarQuotation.Domain** | Entidades, Value Objects, interfaces e regras de domínio |
| **Br.DollarQuotation.Repository** | Entity Framework Core, PostgreSQL, repositories e integrações externas |
| **Br.DollarQuotation.CrossCutting** | Injeção de dependências e IoC |
| **Br.DollarQuotation.Worker** | Processamento periódico das cotações |
| **Br.DollarQuotation.Tests** | Testes unitários e de integração |

---

## 🛠️ Tecnologias

- .NET 9
- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- SignalR
- Worker Service
- HttpClient
- AwesomeAPI
- xUnit

---

## 🔄 Fluxo da aplicação

```text
AwesomeAPI
    │
    ▼
Br.DollarQuotation.Worker
    │
    ▼
Br.DollarQuotation.API
    │
    ├──────────────► PostgreSQL
    │
    ▼
SignalR
    │
    ▼
Angular
