# CS2 Inventory Valuator

A small Windows desktop app (WPF / .NET 8) that connects to a Steam account, imports its
public **Counter-Strike 2** inventory, and **marks it to market** — showing both the gross
market value and the realistic, **net-of-fees** value you'd actually walk away with.

> **Why this project?** A CS2 skin inventory is a real speculative market. Valuing it touches the
> same core jobs as a trading/portfolio system: value holdings, mark to market, account for fees
> and liquidity, watch prices over time, and surface the biggest movers. It's a compact way to
> demonstrate both **modern .NET desktop engineering** and a grasp of **finance fundamentals**.

---

## Screenshots

<!-- Capture these on first run and drop them in docs/. -->
<!-- ![Overview](docs/overview.png) -->
<!-- ![Item detail & price history](docs/item-detail.png) -->

_Screenshots live in [`docs/`](docs/) — run the app (below) and add `overview.png` and
`item-detail.png`._

---

## What it does

- **Connect** by pasting a SteamID64 / profile URL / custom URL, **or** with a polished
  **“Sign in through Steam”** OpenID flow (embedded WebView2).
- **Auto-imports** the inventory (images, quantities, rarity/exterior/weapon metadata).
- **Values every item** against live **Skinport** prices, showing **gross** and **net-of-fees**
  per line and a prominent **portfolio total**.
- **Sort & filter** the holdings; unpriced items are shown honestly as `—`.
- **Caches** to local SQLite for instant startup and an **offline fallback**; manual **Refresh**.
- **Price history & movers, instantly**: a single Skinport *sales-history* call (trailing
  24h/7d/30d/90d windows) powers a per-item **price trend chart** (LiveCharts2) and a
  **biggest-movers** ranking with no waiting. A background service also records fine-grained
  snapshots over time, which the chart prefers once enough have accumulated.
- **Second opinion on demand**: the detail panel also pulls the **Steam Market** price + trade
  **volume** (a liquidity signal) for the selected item.
- Friendly, actionable error when an inventory is **private**.

## Market concepts modeled

| Concept | Where it shows up |
| --- | --- |
| Mark-to-market valuation | Whole-inventory valuation against current prices |
| **Gross vs. net** (transaction fees / realizable value) | `FeeModel`, per-line and total net |
| Liquidity | Skinport listing counts + Steam Market trade volume |
| Multi-source pricing / spread | Skinport (bulk) vs. Steam Market (on-demand) |
| Time-series & momentum | Recorded price history + “biggest movers” (% change) |

> Cost-basis P&L (realized/unrealized gains vs. what you paid) is a documented future phase.

## Architecture

A clean, layered solution — dependencies point inward only:

```
CStoValuation.Core            net8.0          domain models, enums, service contracts, pure valuation
        ▲
CStoValuation.Infrastructure  net8.0          HTTP clients (Steam, Skinport), EF Core/SQLite, repos,
        ▲                                      background snapshot service
CStoValuation.App             net8.0-windows  WPF views + view-models (MVVM), DI host, OpenID window
CStoValuation.Tests           net8.0          xUnit + Moq
```

- **Core** has no dependencies; it defines the interfaces the outer layers implement
  (dependency inversion). The valuation logic is pure and the most heavily unit-tested code.
- **Infrastructure** owns all I/O: typed `HttpClient`s (Brotli + resilience for Skinport),
  EF Core with a SQLite database under `%AppData%/CStoValuation/`, and the hosted snapshot service.
- **App** is WPF + MVVM (CommunityToolkit.Mvvm), composed by the generic `Host` so it gets
  constructor injection and hosted services just like ASP.NET Core.

## Tech stack

.NET 8 · WPF · C# 12 · CommunityToolkit.Mvvm · Microsoft.Extensions.Hosting & DI ·
Microsoft.Extensions.Http.Resilience · EF Core 8 + SQLite · LiveChartsCore (SkiaSharp) ·
Microsoft.Web.WebView2 · xUnit + Moq. Nullable reference types and **warnings-as-errors** on.

## Build, run, test

Prerequisites: the .NET SDK (see [`global.json`](global.json)) on Windows, and the
[WebView2 runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (preinstalled on
current Windows 10/11) for the Steam sign-in window.

```bash
# from the repository root
dotnet build CStoValuation.sln
dotnet test
dotnet run --project src/CStoValuation.App
```

In the app, paste a **public** SteamID64 or profile URL and click **Connect** (or use
**Sign in through Steam**). If an inventory is private, the app explains how to make it public:
Steam → *Profile → Edit Profile → Privacy Settings → Game details: Public*.

### Database migrations

The database is created/updated automatically at startup. To add a schema change:

```bash
dotnet tool restore   # restores the pinned dotnet-ef local tool
dotnet ef migrations add <Name> --project src/CStoValuation.Infrastructure --output-dir Persistence/Migrations
```

## Configuration

- **Currency** defaults to **EUR** (Skinport's native currency; Steam currency id 3) and is
  centralized for easy change.
- **Seller fee** defaults to **8%** via `FeeModel`, the single source of truth for the gross→net
  relationship.

## Continuous integration

Every push / PR builds the whole solution and runs the tests on `windows-latest`
(warnings are errors) — see [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

## Roadmap

- **Phase 2** — catalogue browser (price any item, not just owned).
- **Phase 4** — cost-basis P&L, simple arbitrage across venues, CSV export.
