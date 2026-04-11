# SmartHal

**Open-source, local-first IoT management platform for smart home infrastructure**

SmartHal provides a unified control layer for managing devices across heterogeneous smart home systems. It focuses on installation, maintenance, migration, and security — not on replacing existing platforms like Home Assistant or openHAB, but on managing the infrastructure beneath them.

## Features

- **Unified Device Management** — Single model for devices from different manufacturers and protocols
- **Declarative YAML Configuration** — One file per device, Git-friendly, human-readable
- **Backup & Restore** — Snapshots with dry-run preview, selective restore, and automatic rollback protection
- **Multi-Protocol Support** — HomeMatic (Phase 1), with EVCC, MQTT, and Matter planned
- **Secrets Management** — OS-integrated credential storage (Keychain, Credential Manager, Secret Service) plus encrypted file fallback
- **Extensible Adapter System** — Plugin architecture supporting multiple instances per adapter type
- **Cross-Platform** — Runs on Linux, macOS, and Windows (Raspberry Pi 4 compatible)

## Architecture

SmartHal follows a **library-first** design. All business logic lives in reusable core libraries, with the CLI as the first presentation layer.

```
┌─────────────────────────────────────┐
│  CLI (shal)                         │  ← Presentation
├─────────────────────────────────────┤
│  Core Libraries                     │
│  Config · Backup · Devices ·        │
│  Adapters · Secrets                 │  ← Business Logic
├─────────────────────────────────────┤
│  Adapter Plugins                    │
│  HomeMatic · EVCC · MQTT · Matter   │  ← Protocol Integration
├─────────────────────────────────────┤
│  YAML Files                         │  ← Persistence
└─────────────────────────────────────┘
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (v10.0.100 or later)

## Getting Started

### Build

```bash
dotnet build SmartHal.slnx
```

### Run Tests

```bash
dotnet test SmartHal.slnx
```

### Initialize a Configuration

```bash
dotnet run --project src/Shells/SmartHal.CLI -- config init --config ./my-config
```

This creates the directory structure:

```
my-config/
├── meta.yaml          # Schema version, secrets provider config
├── rooms.yaml         # Room and group definitions
├── adapters/          # Adapter instance configurations
├── devices/           # Device YAML files (one per device)
├── filters/           # Parameter filter profiles
├── scopes/            # Reusable backup/restore scopes
└── snapshots/         # Backup snapshots
```

## CLI Reference

SmartHal's CLI follows a consistent `shal <resource> <action> [options]` pattern.

| Command Group | Description |
|---|---|
| `shal device` | Discover, list, show, set, and replace devices |
| `shal config` | Initialize, validate, diff, and apply configurations |
| `shal backup` | Create, restore, list, show, delete, and cleanup snapshots |
| `shal adapter` | Configure and list adapter instances |
| `shal instance` | Add, remove, and list adapter instances |
| `shal secret` | Manage secrets (set, get, list, delete) |

### Examples

```bash
# List all devices
shal device list

# Discover devices from an adapter
shal device discover --adapter-id ccu-main

# Create a backup snapshot
shal backup create --description "Before migration"

# Preview a restore without applying
shal backup restore <snapshot-id> --diff

# Validate configuration
shal config validate
```

> [!TIP]
> Use `--output json` or `--output yaml` on any command for machine-readable output.

## Project Structure

```
src/
├── Core/
│   ├── SmartHal.Core.Shared        # Exceptions, logging, utilities
│   ├── SmartHal.Core.Devices       # Device model and types
│   ├── SmartHal.Core.Adapters      # Adapter interfaces and factory
│   ├── SmartHal.Core.Secrets       # Secrets provider implementations
│   ├── SmartHal.Core.Config        # YAML config engine (read, write, diff, validate)
│   └── SmartHal.Core.Backup        # Snapshot management and restore orchestration
├── Adapters/
│   └── SmartHal.Adapter.HomeMatic  # HomeMatic XML-RPC protocol adapter
└── Shells/
    └── SmartHal.CLI                # Command-line interface

tests/                              # Matching test projects for each component
```

## Adapter System

Adapters implement `ISmartHalAdapter` with optional capability interfaces:

| Interface | Capability |
|---|---|
| `IDeviceDiscovery` | Discover devices on the network |
| `IDeviceReader` | Read current device state |
| `IDeviceWriter` | Write parameters to devices |
| `IRelationManager` | Manage device-to-device relations |
| `IBackupRestore` | Native adapter-level backup/restore |

Adapters support **multi-instance** configurations — run multiple instances of the same adapter type (e.g., two HomeMatic CCUs) with independent settings.

## Secrets Management

SmartHal never stores secrets in YAML configuration files. Instead, adapter settings reference secrets by key, and the configured provider handles secure storage.

| Provider | Platform |
|---|---|
| Windows Credential Manager | Windows |
| macOS Keychain | macOS |
| Secret Service (D-Bus) | Linux |
| Encrypted File (AES-256-GCM) | Cross-platform |
| Environment Variables | CI/CD |

## Roadmap

| Phase | Focus | Status |
|---|---|---|
| **Phase 1** | CLI & Core — Device management, HomeMatic adapter, YAML config, backup/restore | 🔨 In Progress |
| **Phase 2** | REST API (ASP.NET Core) & Angular Web UI | Planned |
| **Phase 3** | Multi-location management, cloud backup (E2E encrypted), automation | Planned |

## Tech Stack

- **.NET 10** with C# 14
- **YamlDotNet** for configuration serialization
- **Serilog** for structured logging
- **Spectre.Console** for rich CLI output
- **xUnit** + **FakeItEasy** + **AwesomeAssertions** for testing
