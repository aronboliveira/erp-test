# 📑✍ THIS IS A PORTFOLIO PROJECT

# ACME ERP — Admin Platform

> Full-stack enterprise resource planning system: **.NET 8 + Angular 20 SSR**
> Java backend is preserved in the `backend-java-version` branch

🇺🇸 [English (US)](#en-us) · 🇧🇷 [Português (BR)](#pt-br) · 🇬🇧 [English (UK)](#en-gb) · 🇫🇷 [Français](#fr) · 🇮🇹 [Italiano](#it) · 🇨🇳 [中文](#zh) · 🇷🇺 [Русский](#ru)

---

<a id="en-us"></a>

<details open>
<summary>🇺🇸 English (US)</summary>

## Overview

ACME Admin is a monorepo containing two workspaces:

| Module             | Stack                                                       | Port   |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### Features

- **Finance** — Revenues, expenses, budgets, bills, purchases, hiring records
- **Billing** — Stripe checkout sessions, payment intents, webhook ingestion
- **Catalog** — Products & services with categories, SKU and tax linking
- **Auth & RBAC** — Users, roles, permissions (BCrypt + stateless security)
- **Schema compatibility** — Reuses the existing PostgreSQL schema from the Java line

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (or use the provided `docker-compose.yml`)

### Quick Start

```bash
# 1. Start PostgreSQL
docker compose up -d postgres

# 2. Run the API (dev)
cd acme-admin-dotnet && ./start.sh

# 3. In another terminal — start the dashboard
cd admin-dashboard && npm install && npm start
```

### Project Structure

```
acme-admin-dotnet/
├── Controllers/          # REST controllers
├── Data/                 # EF Core DbContext and mappings
├── Domain/               # Entities and enums
├── DTO/                  # Request/response contracts
├── Middleware/           # API exception mapping
├── Security/             # Auth handler and policy catalog
├── Services/             # Business logic layer
└── Validation/           # Validation framework
admin-dashboard/
├── src/app/
│   ├── core/             # Alerts, API client, auth, bootstrap
│   ├── features/         # Billing, dashboard, orders, profile
│   ├── layout/           # App shell, sidebar, topbar
│   ├── lib/              # Interfaces, types, shared models
│   ├── pages/            # Configs, expenses, revenues pages
│   └── shared/           # Directives, services, theme, utils
```

### Environment Variables

| Variable                 | Description                   | Required |
| ------------------------ | ----------------------------- | -------- |
| `ConnectionStrings__Default` | PostgreSQL connection string (`Host=...;Port=...`) | prod |
| `Stripe__PublishableKey` | Stripe public key             | prod     |
| `Billing__Stripe__SecretKey` | Stripe secret key        | prod     |
| `Billing__Stripe__WebhookSecret` | Stripe webhook signing secret | prod |
| `Billing__Stripe__SuccessUrl` | Billing success redirect URL | optional |
| `Billing__Stripe__CancelUrl` | Billing cancel redirect URL | optional |
| `Auth__EnableMockHeader` | Enables `X-Mock-User` / `X-Mock-Perms` auth headers in non-production environments only | optional |
| `ENABLE_MOCK_HEADERS` | Enables SSR proxy injection of mock headers to backend requests | optional |
| `MOCK_USER` | User value for SSR-injected `X-Mock-User` header (used only when `ENABLE_MOCK_HEADERS=true`) | optional |
| `MOCK_PERMS` | Permission list for SSR-injected `X-Mock-Perms` header | optional |

### API Endpoints

| Method         | Path                            | Description             |
| -------------- | ------------------------------- | ----------------------- |
| `GET/POST`     | `/api/sales/orders`             | Orders endpoints        |
| `GET/POST`     | `/api/finance/revenue`          | Revenue records         |
| `GET/POST`     | `/api/finance/expenses`         | Expense records         |
| `GET/POST`     | `/api/finance/budgets`          | Budget periods          |
| `GET/POST`     | `/api/finance/bills`            | Bill management         |
| `GET/POST`     | `/api/catalog/items`            | Products & services     |
| `GET/POST`     | `/api/taxes`                    | Tax configuration       |
| `POST`         | `/api/billing/checkout-session` | Stripe checkout         |
| `POST`         | `/api/billing/payment-intents`  | Stripe payment intents  |
| `POST`         | `/api/billing/webhook`          | Stripe webhook receiver |
| `GET`          | `/api/billing/events`           | Billing event log       |
| `GET/POST/PUT` | `/api/admin/users`              | User management         |
| `GET/POST/PUT` | `/api/admin/roles`              | Role management         |
| `GET`          | `/api/me`                       | Current user profile    |

### Docker

```bash
# Full stack (API + DB + Dashboard)
docker compose -f docker-compose.yml up --build
```

### License

Private — all rights reserved.

</details>

---

<a id="pt-br"></a>

<details>
<summary>🇧🇷 Português (BR)</summary>

## Visão Geral

ACME Admin é um monorepo contendo dois workspaces:

| Módulo             | Stack                                                       | Porta  |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### Funcionalidades

- **Financeiro** — Receitas, despesas, orçamentos, contas, compras, contratações
- **Cobrança** — Sessões de checkout Stripe, payment intents, ingestão de webhooks
- **Catálogo** — Produtos e serviços com categorias, SKU e vinculação de impostos
- **Auth & RBAC** — Usuários, papéis, permissões (BCrypt + segurança stateless)
- **Compatibilidade de schema** — Reutiliza o schema PostgreSQL existente da linha Java

### Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (ou use o `docker-compose.yml` fornecido)

### Início Rápido

```bash
# 1. Iniciar PostgreSQL
docker compose up -d postgres

# 2. Executar a API (perfil dev — popula dados demo)
cd acme-admin-dotnet && ./start.sh

# 3. Em outro terminal — iniciar o dashboard
cd admin-dashboard && npm install && npm start
```

### Variáveis de Ambiente

| Variável                 | Descrição                     | Obrigatória |
| ------------------------ | ----------------------------- | ----------- |
| `ConnectionStrings__Default` | String de conexão PostgreSQL (`Host=...;Port=...`) | prod |
| `Stripe__PublishableKey` | Chave pública Stripe          | prod        |
| `Billing__Stripe__SecretKey` | Chave secreta Stripe    | prod        |
| `Billing__Stripe__WebhookSecret` | Segredo de assinatura webhook | prod |
| `Billing__Stripe__SuccessUrl` | URL de sucesso da cobrança | opcional |
| `Billing__Stripe__CancelUrl` | URL de cancelamento da cobrança | opcional |
| `Auth__EnableMockHeader` | Habilita headers `X-Mock-User` / `X-Mock-Perms` em ambientes não produtivos | opcional |
| `ENABLE_MOCK_HEADERS` | Habilita injeção de headers mock no proxy SSR | opcional |
| `MOCK_USER` | Usuário para header `X-Mock-User` (quando `ENABLE_MOCK_HEADERS=true`) | opcional |
| `MOCK_PERMS` | Lista de permissões para header `X-Mock-Perms` | opcional |

### Licença

Privado — todos os direitos reservados.

</details>

---

<a id="en-gb"></a>

<details>
<summary>🇬🇧 English (UK)</summary>

## Overview

ACME Admin is a monorepo containing two workspaces:

| Module             | Stack                                                       | Port   |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### Features

- **Finance** — Revenues, expenses, budgets, bills, purchases, hiring records
- **Billing** — Stripe checkout sessions, payment intents, webhook ingestion
- **Catalogue** — Products & services with categories, SKU and tax linking
- **Auth & RBAC** — Users, roles, permissions (BCrypt + stateless security)
- **Schema compatibility** — Reuses the existing PostgreSQL schema from the Java line

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (or use the provided `docker-compose.yml`)

### Quick Start

```bash
# 1. Start PostgreSQL
docker compose up -d postgres

# 2. Run the API (dev profile — auto-seeds demo data)
cd acme-admin-dotnet && ./start.sh

# 3. In another terminal — start the dashboard
cd admin-dashboard && npm install && npm start
```

### Licence

Private — all rights reserved.

</details>

---

<a id="fr"></a>

<details>
<summary>🇫🇷 Français</summary>

## Aperçu

ACME Admin est un monorepo contenant deux espaces de travail :

| Module             | Stack                                                       | Port   |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### Fonctionnalités

- **Finance** — Revenus, dépenses, budgets, factures, achats, embauches
- **Facturation** — Sessions Stripe checkout, payment intents, ingestion de webhooks
- **Catalogue** — Produits et services avec catégories, SKU et liaison fiscale
- **Auth & RBAC** — Utilisateurs, rôles, permissions (BCrypt + sécurité stateless)
- **Compatibilité du schéma** — Réutilise le schéma PostgreSQL existant de la ligne Java

### Prérequis

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (ou utilisez le `docker-compose.yml` fourni)

### Démarrage Rapide

```bash
# 1. Démarrer PostgreSQL
docker compose up -d postgres

# 2. Lancer l'API (profil dev — peuple les données démo)
cd acme-admin-dotnet && ./start.sh

# 3. Dans un autre terminal — lancer le tableau de bord
cd admin-dashboard && npm install && npm start
```

### Licence

Privé — tous droits réservés.

</details>

---

<a id="it"></a>

<details>
<summary>🇮🇹 Italiano</summary>

## Panoramica

ACME Admin è un monorepo contenente due workspace:

| Modulo             | Stack                                                       | Porta  |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### Funzionalità

- **Finanza** — Entrate, spese, budget, fatture, acquisti, assunzioni
- **Fatturazione** — Sessioni Stripe checkout, payment intents, ingestione webhook
- **Catalogo** — Prodotti e servizi con categorie, SKU e collegamento fiscale
- **Auth & RBAC** — Utenti, ruoli, permessi (BCrypt + sicurezza stateless)
- **Compatibilità schema** — Riutilizza lo schema PostgreSQL esistente della linea Java

### Prerequisiti

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (o utilizzare il `docker-compose.yml` fornito)

### Avvio Rapido

```bash
# 1. Avviare PostgreSQL
docker compose up -d postgres

# 2. Eseguire l'API (profilo dev — popola dati demo)
cd acme-admin-dotnet && ./start.sh

# 3. In un altro terminale — avviare la dashboard
cd admin-dashboard && npm install && npm start
```

### Licenza

Privato — tutti i diritti riservati.

</details>

---

<a id="zh"></a>

<details>
<summary>🇨🇳 中文</summary>

## 概述

ACME Admin 是一个包含两个工作区的 monorepo：

| 模块               | 技术栈                                                      | 端口   |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### 功能

- **财务** — 收入、支出、预算、账单、采购、招聘记录
- **计费** — Stripe 结账会话、支付意向、Webhook 接收
- **目录** — 产品与服务，支持分类、SKU 和税务关联
- **认证与 RBAC** — 用户、角色、权限（BCrypt + 无状态安全）
- **Schema 兼容性** — 复用 Java 线已有的 PostgreSQL schema

### 前置要求

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16（或使用提供的 `docker-compose.yml`）

### 快速开始

```bash
# 1. 启动 PostgreSQL
docker compose up -d postgres

# 2. 运行 API（dev 配置文件 — 自动填充演示数据）
cd acme-admin-dotnet && ./start.sh

# 3. 在另一个终端 — 启动仪表板
cd admin-dashboard && npm install && npm start
```

### 许可证

私有 — 保留所有权利。

</details>

---

<a id="ru"></a>

<details>
<summary>🇷🇺 Русский</summary>

## Обзор

ACME Admin — это монорепозиторий, содержащий два рабочих пространства:

| Модуль             | Стек                                                        | Порт   |
| ------------------ | ----------------------------------------------------------- | ------ |
| `acme-admin-dotnet/` | .NET 8 · ASP.NET Core · EF Core · PostgreSQL 16 · Stripe(noop-first) | `8080` |
| `admin-dashboard/` | Angular 20 · SSR (Express 5) · Tailwind v4 · ngx-charts     | `4200` |

### Возможности

- **Финансы** — Доходы, расходы, бюджеты, счета, закупки, найм
- **Биллинг** — Сессии Stripe checkout, платёжные намерения, приём вебхуков
- **Каталог** — Товары и услуги с категориями, SKU и привязкой налогов
- **Авторизация и RBAC** — Пользователи, роли, разрешения (BCrypt + stateless)
- **Совместимость схемы** — Переиспользует существующую PostgreSQL-схему из Java-линии

### Предварительные требования

- .NET 8 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (или используйте предоставленный `docker-compose.yml`)

### Быстрый старт

```bash
# 1. Запустить PostgreSQL
docker compose up -d postgres

# 2. Запустить API (профиль dev — автозаполнение демо-данными)
cd acme-admin-dotnet && ./start.sh

# 3. В другом терминале — запустить дашборд
cd admin-dashboard && npm install && npm start
```

### Лицензия

Частный — все права защищены.

</details>
