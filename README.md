# ERP_AI Desktop Pro

A modern .NET 8 WPF desktop application for ERP and accounting, featuring MVVM architecture, offline-first SQLite database, cloud sync, and advanced UI with ModernWpfUI.

## Solution Structure
- **ERP_AI.Desktop**: WPF main application (MVVM, DI, navigation, dialogs, logging, config, ModernWpfUI, responsive UI, dark/light theme)
- **ERP_AI.Core**: Business logic, entities, validation, concurrency, audit, soft delete
- **ERP_AI.Data**: Entity Framework Core (SQLite), repositories, migrations, backup/restore, performance, full-text search, seed data
- **ERP_AI.Services**: Business services, sync services, caching, background ops
- **ERP_AI.CloudSync**: Cloud sync logic, conflict resolution, mapping, queue, settings
- **ERP_AI.Tests**: Unit tests

## Features
- MVVM base classes, ViewModelLocator, RelayCommand/AsyncRelayCommand
- Navigation & dialog services
- Configuration & settings management
- Logging setup (Serilog)
- ModernWpfUI theme, custom window chrome, responsive layout, dark/light theme
- Application lifecycle & shutdown handling
- SQLite (WAL mode), EF Core DbContext, migrations, auto-create, backup/restore
- Core entities (with sync metadata), sync-related entities
- Soft delete, audit trail, concurrency, validation, indexes, full-text search
- Seed chart of accounts, performance optimization
- Data services (repository, unit of work, caching, background ops)
- UI services (dialogs, notifications, progress, file dialogs)
- Error handling (global, user-friendly, logging, retry, offline)
- Validation (FluentValidation, real-time, form-level, custom rules, error display)

## NuGet Packages
- Entity Framework Core (SQLite)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging
- CommunityToolkit.Mvvm
- ModernWpfUI
- Newtonsoft.Json
- RestSharp
- Serilog

## Getting Started
1. Open the solution in Visual Studio or VS Code.
2. Restore NuGet packages.
3. Build the solution.
4. Run ERP_AI.Desktop to launch the application.

## Documentation
- See copilot-instructions.md in .github for development workflow and guidelines.
