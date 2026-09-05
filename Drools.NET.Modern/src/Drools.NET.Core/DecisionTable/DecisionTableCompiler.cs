using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Drools.NET.Core.Implementation
{
    /// <summary>
    /// Compiles decision tables (Excel/CSV) to DRL format
    /// </summary>
    internal class DecisionTableCompiler
    {
        public string Compile(Stream inputStream, DecisionTableInputType inputType)
        {
            return inputType switch
            {
                DecisionTableInputType.Excel => CompileExcel(inputStream),
                DecisionTableInputType.CSV => CompileCsv(inputStream),
                _ => throw new ArgumentException($"Unsupported input type: {inputType}")
            };
        }

        private string CompileExcel(Stream inputStream)
        {
            // This would use a library like EPPlus or NPOI to read Excel files
            // For now, return a placeholder
            throw new NotImplementedException("Excel decision table compilation requires EPPlus or NPOI package");
        }

        private string CompileCsv(Stream inputStream)
        {
            using var reader = new StreamReader(inputStream);
            var lines = new List<string>();
            
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            if (lines.Count < 2)
            {
                throw new ArgumentException("CSV decision table must have at least header and one data row");
            }

            return CompileCsvToRules(lines);
        }

        private string CompileCsvToRules(List<string> csvLines)
        {
            // Parse CSV headers
            var headers = ParseCsvLine(csvLines[0]);
            var conditionHeaders = new List<(int Index, string Pattern, string Field)>();
            var actionHeaders = new List<(int Index, string Action)>();
            
            string? packageName = null;
            string? ruleName = "DecisionTableRule";

            // Analyze headers to identify conditions and actions
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i].Trim();
                
                if (header.StartsWith("CONDITION-") || header.StartsWith("C-"))
                {
                    // Extract pattern and field from header like "CONDITION-Person.age"
                    var conditionPart = header.Substring(header.IndexOf('-') + 1);
                    var parts = conditionPart.Split('.');
                    if (parts.Length >= 2)
                    {
                        conditionHeaders.Add((i, parts[0], parts[1]));
                    }
                }
                else if (header.StartsWith("ACTION-") || header.StartsWith("A-"))
                {
                    // Extract action from header like "ACTION-modify($person)"
                    var actionPart = header.Substring(header.IndexOf('-') + 1);
                    actionHeaders.Add((i, actionPart));
                }
                else if (header.Equals("PACKAGE", StringComparison.OrdinalIgnoreCase))
                {
                    if (csvLines.Count > 1)
                    {
                        var firstDataRow = ParseCsvLine(csvLines[1]);
                        if (i < firstDataRow.Length)
                        {
                            packageName = firstDataRow[i];
                        }
                    }
                }
                else if (header.Equals("RULE-NAME", StringComparison.OrdinalIgnoreCase))
                {
                    ruleName = "DecisionTableRule";
                }
            }

            // Generate DRL
            var drl = new StringBuilder();
            
            // Package declaration
            if (!string.IsNullOrWhiteSpace(packageName))
            {
                drl.AppendLine($"package {packageName};");
            }
            else
            {
                drl.AppendLine("package org.drools.decisiontable;");
            }
            
            drl.AppendLine();

            // Generate rules from data rows
            for (int rowIndex = 1; rowIndex < csvLines.Count; rowIndex++)
            {
                var dataRow = ParseCsvLine(csvLines[rowIndex]);
                var ruleNumber = rowIndex;
                
                drl.AppendLine($"rule \"{ruleName}_{ruleNumber}\"");
                drl.AppendLine("    when");
                
                // Generate conditions
                var patterns = new Dictionary<string, List<string>>();
                foreach (var (index, pattern, field) in conditionHeaders)
                {
                    if (index < dataRow.Length && !string.IsNullOrWhiteSpace(dataRow[index]))
                    {
                        var value = dataRow[index].Trim();
                        if (!patterns.ContainsKey(pattern))
                        {
                            patterns[pattern] = new List<string>();
                        }
                        patterns[pattern].Add($"{field} == \"{value}\"");
                    }
                }

                // Generate pattern conditions
                foreach (var (pattern, conditions) in patterns)
                {
                    var variable = pattern.ToLower();
                    drl.AppendLine($"        ${variable} : {pattern}( {string.Join(" && ", conditions)} )");
                }

                drl.AppendLine("    then");
                
                // Generate actions
                foreach (var (index, action) in actionHeaders)
                {
                    if (index < dataRow.Length && !string.IsNullOrWhiteSpace(dataRow[index]))
                    {
                        var actionValue = dataRow[index].Trim();
                        if (!string.IsNullOrWhiteSpace(actionValue))
                        {
                            // Replace placeholders in action template
                            var expandedAction = action.Replace("$value", actionValue);
                            drl.AppendLine($"        {expandedAction};");
                        }
                    }
                }
                
                drl.AppendLine("end");
                drl.AppendLine();
            }

            return drl.ToString();
        }

        private string[] ParseCsvLine(string csvLine)
        {
            // Simple CSV parsing - a real implementation would handle quoted fields, escapes, etc.
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            
            for (int i = 0; i < csvLine.Length; i++)
            {
                char c = csvLine[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            
            fields.Add(current.ToString().Trim());
            return fields.ToArray();
        }
    }
}