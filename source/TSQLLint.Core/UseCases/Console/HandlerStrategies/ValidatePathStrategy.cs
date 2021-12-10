using System.Linq;
using TSQLLint.Common;
using TSQLLint.Core.DTO;
using TSQLLint.Core.Interfaces;

namespace TSQLLint.Core.UseCases.Console.HandlerStrategies
{
    public class ValidatePathStrategy : IHandlingStrategy
    {
        private readonly IBaseReporter reporter;
        private readonly IFileSystemWrapper fileSystem;

        public ValidatePathStrategy(IBaseReporter reporter, IFileSystemWrapper fileSystem)
        {
            this.reporter = reporter;
            this.fileSystem = fileSystem;
        }

        public HandlerResponseMessage HandleCommandLineOptions(ICommandLineOptions commandLineOptions)
        {
            //var invalidPaths = commandLineOptions.LintPath.Count(path => !fileSystem.PathIsValidForLint(path));
            //var invalidPath = commandLineOptions.LintSqlQueries.Count(query => !fileSystem.PathIsValidForLint());


            //if (invalidPaths < commandLineOptions.LintSqlQueries.Count)
            //{
                // if there are valid paths then signal success and lint
                return new HandlerResponseMessage(true, true);
            //}

            //reporter.Report("No valid file paths provided");
            //return new HandlerResponseMessage(false, false);

            //reporter.Report("No valid file paths provided");
            //return new HandlerResponseMessage(true, true);
        }
    }
}
