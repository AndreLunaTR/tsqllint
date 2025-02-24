using System.Collections.Generic;

namespace TSQLLint.Core.Interfaces
{
    public interface ICommandLineOptions
    {
        string[] Args { get; set; }

        string ConfigFile { get; set; }

        bool Force { get; set; }

        bool Help { get; set; }

        bool Init { get; set; }

        //List<string> LintPath { get; set; }
        List<string> LintSqlQueries { get; set; }

        bool ListPlugins { get; set; }

        bool PrintConfig { get; set; }

        bool Version { get; set; }

        string GetUsage();
    }
}
