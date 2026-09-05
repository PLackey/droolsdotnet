using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Simple DRL (Drools Rule Language) parser
    /// This is a basic implementation - a production version would use ANTLR or similar
    /// </summary>
    internal class DrlParser
    {
        private readonly IPackageBuilderConfiguration _configuration;
        private readonly List<ICompilationError> _errors = new();

        public DrlParser(IPackageBuilderConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IReadOnlyList<ICompilationError> Errors => _errors.AsReadOnly();

        public PackageDescriptor? Parse(string drlContent, string resourceName)
        {
            _errors.Clear();
            
            if (string.IsNullOrWhiteSpace(drlContent))
            {
                AddError("DRL content is empty", resourceName);
                return null;
            }

            try
            {
                var lines = drlContent.Split('\n')
                    .Select((line, index) => new LineInfo { Line = line.Trim(), Number = index + 1 })
                    .Where(x => !string.IsNullOrEmpty(x.Line) && !x.Line.StartsWith("//"))
                    .ToList();

                var packageDescr = new PackageDescriptor();
                
                // Parse package declaration
                ParsePackageDeclaration(lines, packageDescr, resourceName);
                
                // Parse imports
                ParseImports(lines, packageDescr, resourceName);
                
                // Parse globals
                ParseGlobals(lines, packageDescr, resourceName);
                
                // Parse functions  
                ParseFunctions(lines, packageDescr, resourceName);
                
                // Parse rules
                ParseRules(lines, packageDescr, resourceName);

                return packageDescr;
            }
            catch (Exception ex)
            {
                AddError($"Failed to parse DRL: {ex.Message}", resourceName, exception: ex);
                return null;
            }
        }

        private class LineInfo
        {
            public string Line { get; set; } = "";
            public int Number { get; set; }
        }

        private void ParsePackageDeclaration(List<LineInfo> lines, PackageDescriptor packageDescr, string resourceName)
        {
            var packageLine = lines.FirstOrDefault(x => x.Line.StartsWith("package "));
            if (packageLine != null)
            {
                var packageName = packageLine.Line.Substring(8).Trim().TrimEnd(';');
                packageDescr.Name = packageName;
            }
            else
            {
                packageDescr.Name = "default";
            }
        }

        private void ParseImports(List<LineInfo> lines, PackageDescriptor packageDescr, string resourceName)
        {
            var importPattern = new Regex(@"^import\s+([^;]+);?$", RegexOptions.Compiled);
            
            foreach (var line in lines)
            {
                var match = importPattern.Match(line.Line);
                if (match.Success)
                {
                    var importName = match.Groups[1].Value.Trim();
                    packageDescr.Imports.Add(importName);
                }
            }
        }

        private void ParseGlobals(List<LineInfo> lines, PackageDescriptor packageDescr, string resourceName)
        {
            var globalPattern = new Regex(@"^global\s+(\w+(?:\.\w+)*)\s+(\w+);?$", RegexOptions.Compiled);
            
            foreach (var line in lines)
            {
                var match = globalPattern.Match(line.Line);
                if (match.Success)
                {
                    var typeName = match.Groups[1].Value.Trim();
                    var variableName = match.Groups[2].Value.Trim();
                    
                    try
                    {
                        var type = ResolveType(typeName);
                        packageDescr.Globals.Add(new GlobalDescriptor(variableName, type));
                    }
                    catch (Exception ex)
                    {
                        AddError($"Failed to resolve type '{typeName}' for global '{variableName}': {ex.Message}", 
                               resourceName, line.Number);
                    }
                }
            }
        }

        private void ParseFunctions(List<LineInfo> lines, PackageDescriptor packageDescr, string resourceName)
        {
            // Basic function parsing - would be more sophisticated in a real implementation
            var functionPattern = new Regex(@"^function\s+(\w+)\s+(\w+)\s*\(([^)]*)\)", RegexOptions.Compiled);
            
            foreach (var line in lines)
            {
                var match = functionPattern.Match(line.Line);
                if (match.Success)
                {
                    var returnType = match.Groups[1].Value.Trim();
                    var functionName = match.Groups[2].Value.Trim();
                    var parameters = match.Groups[3].Value.Trim();
                    
                    var functionDescr = new FunctionDescriptor(functionName, returnType, parameters);
                    packageDescr.Functions.Add(functionDescr);
                }
            }
        }

        private void ParseRules(List<LineInfo> lines, PackageDescriptor packageDescr, string resourceName)
        {
            var ruleStart = -1;
            var currentRule = new List<string>();
            var inRule = false;
            var braceCount = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var lineContent = line.Line;

                if (lineContent.StartsWith("rule ") && !inRule)
                {
                    ruleStart = line.Number;
                    inRule = true;
                    currentRule.Clear();
                    braceCount = 0;
                }

                if (inRule)
                {
                    currentRule.Add(lineContent);
                    
                    // Count braces to determine rule end
                    braceCount += lineContent.Count(c => c == '{') - lineContent.Count(c => c == '}');
                    
                    if (lineContent.Trim() == "end" || (braceCount == 0 && currentRule.Count > 1))
                    {
                        // End of rule
                        try
                        {
                            var ruleDescr = ParseSingleRule(currentRule, ruleStart, resourceName);
                            if (ruleDescr != null)
                            {
                                packageDescr.Rules.Add(ruleDescr);
                            }
                        }
                        catch (Exception ex)
                        {
                            AddError($"Failed to parse rule starting at line {ruleStart}: {ex.Message}", 
                                   resourceName, ruleStart, exception: ex);
                        }
                        
                        inRule = false;
                        currentRule.Clear();
                    }
                }
            }

            if (inRule)
            {
                AddError($"Unclosed rule starting at line {ruleStart}", resourceName, ruleStart);
            }
        }

        private RuleDescriptor? ParseSingleRule(List<string> ruleLines, int startLine, string resourceName)
        {
            if (ruleLines.Count == 0) return null;

            var ruleContent = string.Join(" ", ruleLines);
            
            // Extract rule name
            var nameMatch = Regex.Match(ruleLines[0], @"rule\s+""([^""]+)""");
            if (!nameMatch.Success)
            {
                nameMatch = Regex.Match(ruleLines[0], @"rule\s+(\w+)");
            }
            
            if (!nameMatch.Success)
            {
                AddError("Rule name not found", resourceName, startLine);
                return null;
            }

            var ruleName = nameMatch.Groups[1].Value;
            
            // Find when/then sections
            var whenIndex = FindSectionIndex(ruleLines, "when");
            var thenIndex = FindSectionIndex(ruleLines, "then");
            
            if (whenIndex == -1 || thenIndex == -1 || thenIndex <= whenIndex)
            {
                AddError($"Rule '{ruleName}' must have 'when' and 'then' sections", resourceName, startLine);
                return null;
            }

            // Extract LHS (when section)
            var lhsLines = ruleLines.Skip(whenIndex + 1).Take(thenIndex - whenIndex - 1).ToList();
            var lhs = new LeftHandSideDescriptor(string.Join(" ", lhsLines));

            // Extract RHS (then section)
            var rhsLines = ruleLines.Skip(thenIndex + 1).ToList();
            // Remove 'end' if present
            if (rhsLines.LastOrDefault()?.Trim() == "end")
            {
                rhsLines.RemoveAt(rhsLines.Count - 1);
            }
            var rhs = new RightHandSideDescriptor(string.Join(" ", rhsLines));

            return new RuleDescriptor
            {
                Name = ruleName,
                Lhs = lhs,
                Rhs = rhs,
                Source = ruleContent,
                Salience = 0, // Would extract from rule attributes
                Enabled = true,
                NoLoop = false,
                LockOnActive = false,
                AutoFocus = false
            };
        }

        private int FindSectionIndex(List<string> lines, string section)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith(section))
                {
                    return i;
                }
            }
            return -1;
        }

        private Type ResolveType(string typeName)
        {
            // Simple type resolution - a real implementation would be more comprehensive
            return typeName switch
            {
                "String" or "string" => typeof(string),
                "int" or "Integer" => typeof(int),
                "long" or "Long" => typeof(long),
                "double" or "Double" => typeof(double),
                "float" or "Float" => typeof(float),
                "boolean" or "bool" or "Boolean" => typeof(bool),
                "Object" or "object" => typeof(object),
                _ => Type.GetType(typeName) ?? typeof(object)
            };
        }

        private void AddError(string message, string resource, int line = 0, Exception? exception = null)
        {
            _errors.Add(new CompilationError(message, true, resource, line, 0, null, exception));
        }
    }
}