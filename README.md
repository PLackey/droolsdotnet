# Drools.NET - .NET Core 8 Port

A Rete-based Business Rule Engine for .NET, upgraded from .NET Framework to .NET Core 8.

This is a fork of https://github.com/codehaus/droolsdotnet

## Overview

This is a .NET port of the Drools rule engine, providing a powerful business rules management system for .NET applications. The engine uses the Rete algorithm for efficient pattern matching and rule evaluation.

## Recent Updates (v2.0.0)

- **Upgraded to .NET Core 8**: Migrated from legacy .NET Framework to modern .NET Core 8
- **Modern Project Format**: Converted to SDK-style project files for better tooling support
- **Package References**: Updated to use modern NuGet package references where possible
- **Nullable Reference Types**: Enabled nullable reference types for better code safety
- **NuGet Package**: Now configured to generate NuGet packages automatically
- **Updated Testing Framework**: Upgraded from NUnit 2.x to NUnit 4.x with modern Assert syntax
- **Binary Compatibility**: Maintained compatibility with existing IKVM-based JAR dependencies
- **Modern .NET Features**: Updated to use latest C# language features and .NET 8 capabilities

## Migration Summary

### What Was Changed
- **Project Format**: Converted from old MSBuild format to modern SDK-style projects
- **Target Framework**: Changed from .NET Framework to `net8.0`
- **Package References**: Added modern NuGet packages (`System.CodeDom`, `System.Configuration.ConfigurationManager`)
- **Test Framework**: Updated NUnit from 2.x to 4.x with new Assert API
- **Assembly Version**: Bumped from 1.0.0.0 to 2.0.0.0
- **Build Configuration**: Added modern build features like nullable reference types and package generation

### What Was Preserved
- **API Compatibility**: All public APIs remain unchanged for backward compatibility
- **IKVM Dependencies**: Existing JAR-based dependencies continue to work
- **Functionality**: All business rules engine functionality preserved
- **Examples**: All example projects and test cases continue to work

## Building

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or later, or any editor that supports .NET development

### Build Commands
```bash
# Restore packages and build solution
dotnet restore
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Create NuGet package
dotnet pack --configuration Release --output .
```

## Projects

- **drools.dotnet**: Core rule engine library
- **drools.dotnet.examples**: Example applications and benchmarks

## Dependencies

The project still depends on some legacy JAR-based dependencies via IKVM:
- `drools-3.0.dll`
- `drools-dep.dll`
- `IKVM.GNU.Classpath.dll`
- `IKVM.Runtime.dll`

These are located in the `lib/` directory and are referenced directly.

## Usage

```csharp
using org.drools.dotnet;

// Create a rule base
RuleBaseFactory rbf = RuleBaseFactory.newRuleBaseFactory();
RuleBase ruleBase = rbf.newRuleBase();

// Create working memory
WorkingMemory workingMemory = ruleBase.newWorkingMemory();

// Add facts and fire rules
workingMemory.assertObject(myFact);
workingMemory.fireAllRules();
```

## Migration Notes

If you're upgrading from the previous version:

1. **Target Framework**: Now targets .NET 8.0 instead of .NET Framework
2. **Assembly Version**: Updated to 2.0.0.0
3. **Package Generation**: Automatically generates NuGet packages during build
4. **Project References**: Uses modern project reference format
5. **NUnit**: If using the examples/tests, note that NUnit API has been updated to 4.x

## NuGet Package

The project now automatically generates a NuGet package:
- **Package ID**: Drools.NET
- **Version**: 2.0.0
- **Target Framework**: .NET 8.0

Install via Package Manager:
```
Install-Package Drools.NET
```

Or via .NET CLI:
```
dotnet add package Drools.NET
```

## License

Copyright © 2007-2024 Sahi Technologies Pvt Ltd