using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TSQLLint.Common;

namespace TSQLLint.Infrastructure.Reporters
{
    public class ConsoleReporter : IReporter
    {
        private int warningCount;
        private int errorCount;

        [ExcludeFromCodeCoverage]
        public virtual void Report(string message)
        {
            NonBlockingConsole.WriteLine(message);
        }

        public void ReportResults(TimeSpan timespan, int fileCount)
        {
            // Report($"\nLinted {fileCount} files in {timespan.TotalSeconds} seconds\n\n{errorCount} Errors.\n{warningCount} Warnings");
            Report($"\nLinted {fileCount} SQL Sections in {timespan.TotalSeconds} seconds\n\n{errorCount} Errors.\n{warningCount} Warnings");
        }

        public void ReportFileResults()
        {
        }

        public void ReportViolation(IRuleViolation violation)
        {
            switch (violation.Severity)
            {
                case RuleViolationSeverity.Warning:
                    warningCount++;
                    break;
                case RuleViolationSeverity.Error:
                    errorCount++;
                    break;
                default:
                    return;
            }

            ReportViolation(
                violation.FileName,
                violation.Line.ToString(),
                violation.Column.ToString(),
                violation.Severity.ToString().ToLowerInvariant(),
                violation.RuleName,
                violation.Text);
        }

        public void ReportViolation(string fileName, string line, string column, string severity, string ruleName, string violationText)
        {
            Report($"{fileName}({line},{column}): {severity} {ruleName} : {violationText}.");
        }
    }
}
