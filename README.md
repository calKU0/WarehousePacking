# PackFlow (KontrolaPakowania)

Web-based packing control system consisting of a Blazor Server UI and a dedicated ASP.NET Core Web API.

The solution retrieves the contents of logistic units from a WMS, supports warehouse operators during packing, generates shipping labels, and creates parcels/shipments in **Comarch ERP XL**.

## Solution structure

- `KontrolaPakowania.Server` (`net8.0`) – Blazor web application (UI)
- `KontrolaPakowania.API` (`net8.0`) – Web API used by the UI and integrations
- `KontrolaPakowania.Shared` (`net8.0`) – shared contracts/models used by both UI and API
- `KontrolaPakowania.API.Tests` (`net9.0`) – automated tests for the API
- `KontrolaPakowania.PrintService` (`.NET Framework 4.8`) – Windows printing/label related helper service

## Key features (high level)

- Import/read logistic unit content from WMS
- Warehouse packing workflow (scanning, packing control, creating/closing parcels)
- Shipping label generation/printing support
- Courier integrations (as implemented in the API): DPD, DPD Romania, GLS, FedEx (REST + SOAP)
- Creating parcels/shipments in Comarch ERP XL
- Email notifications via SMTP

## Tech stack

- Blazor (`KontrolaPakowania.Server`)
- ASP.NET Core Web API (`KontrolaPakowania.API`)
- .NET 8 shared library (`KontrolaPakowania.Shared`)
- .NET Framework 4.8 desktop component (`KontrolaPakowania.PrintService`)
- Data access: Dapper + `Microsoft.Data.SqlClient`
- Logging: Serilog
- API docs: Swagger (Swashbuckle)
- Resilience: Polly

## Prerequisites

- .NET SDK 8.x (to run `KontrolaPakowania.Server` and `KontrolaPakowania.API`)
- .NET SDK 9.x (only required to build/run `KontrolaPakowania.API.Tests`)
- Visual Studio 2022 (recommended)
- Windows + .NET Framework 4.8 Developer Pack (to build `KontrolaPakowania.PrintService`)
- Access/configuration for:
  - WMS endpoints
  - Comarch ERP XL integration endpoints
  - SQL Server (if used by your environment)

## Configuration

Configuration is stored in `appsettings.json` / `appsettings.*.json` in the API and Server projects.

### UI (`KontrolaPakowania.Server`)

Configured in `KontrolaPakowania.Server/appsettings.json`:

- `Apis:Database:BaseUrl` — base URL of the API/backend called by the UI.
- `Apis:Database:ApiKey` — API key sent as `X-Api-Key`.
- `CrystalReports` — report templates and DB access used for printing (labels/invoices).

### API (`KontrolaPakowania.API`)

Configured in `KontrolaPakowania.API/appsettings.json`:

- `ConnectionStrings` — connections to ERP XL.
- `WMSApi` — WMS base URL and token.
- `CourierApis` — sender information and credentials for courier providers.
- `Smtp` — SMTP configuration.

Typical configuration is done via `appsettings.json` / `appsettings.*.json` in the API and Server projects (connection strings, WMS/ERP endpoints, authentication settings, etc.).

## Repository notes

- `wwwroot/downloads` in `KontrolaPakowania.Server` contains packaged downloads (e.g., helper tools/agents) that are copied to output on build.

## Screenshots

Screenshots are stored in the `Screenshots/` folder.

### Logistic units

List of logistic units downloaded from WMS and ready for packing.

![Logistic units list](Screenshots/logistics-units.png)

### Packing view

Main packing screen used by warehouse operators.

![Packing view](Screenshots/packing-view.png)

### Pack item (scan)

Scanning/adding items to the current parcel.

![Pack item / scan](Screenshots/pack-item.png)

### Finish packing

Finalizing a parcel (closing packing, confirming dimensions/weight, etc.).

![Finish packing](Screenshots/finish-packing.png)

### Send package

Shipment step (label generation and sending/creating the parcel in courier/ERP systems).

![Send package](Screenshots/send-package.png)

## License

This project is **proprietary and confidential**.

It was developed for a client and is **not permitted to be shared, redistributed, or used** without explicit written permission from the owner.

See [LICENSE](LICENSE) for details.

---

© 2025-present [calKU0](https://github.com/calKU0)
