using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using TSQLLint.Common;
using TSQLLint.Core.DTO;
using TSQLLint.Core.Interfaces;
using TSQLLint.Core.Interfaces.Config.Contracts;
using TSQLLint.Core.UseCases.Console;
using TSQLLint.Infrastructure.CommandLineOptions;
using TSQLLint.Infrastructure.Configuration;
using TSQLLint.Infrastructure.Parser;
using TSQLLint.Infrastructure.Plugins;
using TSQLLint.Infrastructure.Reporters;

namespace TSQLLint
{
    public class Application
    {
        private readonly IRequestHandler<CommandLineRequestMessage, HandlerResponseMessage> commandLineOptionHandler;
        private readonly ICommandLineOptions commandLineOptions;
        private readonly IConfigReader configReader;
        private readonly IReporter reporter;
        private readonly IConsoleTimer timer;

        private IPluginHandler pluginHandler;
        private ISqlFileProcessor fileProcessor;

        public Application(string[] args, IReporter reporter)
        {
            timer = new ConsoleTimer();
            timer.Start();

            this.reporter = reporter;
            commandLineOptions = new CommandLineOptions(args);
            configReader = new ConfigReader(reporter);
            commandLineOptionHandler = new CommandLineOptionHandler(
                new ConfigFileGenerator(),
                configReader,
                reporter,
                new FileSystemWrapper());
        }

        public void Run()
        {
            // configReader.LoadConfig(commandLineOptions.ConfigFile);
            configReader.LoadConfig("C:\\Users\\C282466\\.tsqllintrc");
            var fragmentBuilder = new FragmentBuilder(configReader.CompatabilityLevel);
            var ruleVisitorBuilder = new RuleVisitorBuilder(configReader, this.reporter);
            var ruleVisitor = new SqlRuleVisitor(ruleVisitorBuilder, fragmentBuilder, reporter);
            pluginHandler = new PluginHandler(reporter);
            fileProcessor = new SqlFileProcessor(ruleVisitor, pluginHandler, reporter, new FileSystem());

            pluginHandler.ProcessPaths(configReader.GetPlugins());
            var response = commandLineOptionHandler.Handle(new CommandLineRequestMessage(commandLineOptions));

            if (response.ShouldLint)
            {
                fileProcessor.ProcessList(commandLineOptions.LintSqlQueries);
            }

            if (fileProcessor.FileCount > 0)
            {
                reporter.ReportResults(timer.Stop(), fileProcessor.FileCount);
            }

            if (!response.Success)
            {
                 Environment.ExitCode = 1;
            }
        }
    }
}
