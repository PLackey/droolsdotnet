# Drools.NET Modern - Pure .NET 8 Rule Engine

A complete rewrite of the Drools rule engine for .NET 8, eliminating IKVM dependencies and providing native .NET performance and compatibility.

## Features

- ✅ **Pure .NET 8** - No IKVM dependencies
- ✅ **Modern C#** - Uses latest language features and patterns  
- ✅ **High Performance** - Native .NET implementation
- ✅ **Full Compatibility** - API-compatible with Drools.NET 2.x
- ✅ **Cross Platform** - Works on Windows, Linux, and macOS
- ✅ **Modern Tooling** - Full debugging and IntelliSense support

## Quick Start

```csharp
using Drools.NET.Core;

// Create rule base
var ruleBase = RuleBaseFactory.CreateRuleBase();

// Build package from DRL
var packageBuilder = new PackageBuilder();
await packageBuilder.AddPackageFromDrlAsync("rules.drl", drlContent);
var package = packageBuilder.GetPackage();
ruleBase.AddPackage(package);

// Execute rules
var workingMemory = ruleBase.CreateWorkingMemory();
workingMemory.AssertObject(new MyFact());
workingMemory.FireAllRules();
```

## Installation

```bash
dotnet add package Drools.NET.Modern
```

## Documentation

See [Migration Guide](docs/migration-guide.md) for upgrading from legacy Drools.NET.