using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSQLLint.Common;
using TSQLLint.Core.Interfaces;
using TSQLLint.Infrastructure.Plugins;
using TSQLLint.Infrastructure.Rules.RuleExceptions;

namespace TSQLLint.Infrastructure.Parser
{
    public class SqlFileProcessor : ISqlFileProcessor
    {
        private readonly IRuleVisitor ruleVisitor;

        private readonly IReporter reporter;

        private readonly IFileSystem fileSystem;

        private readonly IPluginHandler pluginHandler;

        private readonly IRuleExceptionFinder ruleExceptionFinder;

        private ConcurrentDictionary<string, Stream> queryStreams = new ConcurrentDictionary<string, Stream>();

        public SqlFileProcessor(IRuleVisitor ruleVisitor, IPluginHandler pluginHandler, IReporter reporter, IFileSystem fileSystem)
        {
            this.ruleVisitor = ruleVisitor;
            this.pluginHandler = pluginHandler;
            this.reporter = reporter;
            this.fileSystem = fileSystem;
            ruleExceptionFinder = new RuleExceptionFinder();
        }

        private int _fileCount;
        public int FileCount
        {
            get
            {
                return _fileCount;
            }
        }

        public void ProcessList(List<string> sqlQueries)
        {
            Parallel.ForEach(sqlQueries, (query) =>
            {                
                processSqlQuery(query);
            });            

            foreach (var query in queryStreams)
            {
                HandleProcessing(query.Key, query.Value);
                query.Value.Dispose();
            }
        }

        public void ProcessSqlQuery(string query)
        {
            processSqlQuery(query);            

            foreach (var sqlQuery in queryStreams)
            {
                HandleProcessing(sqlQuery.Key, sqlQuery.Value);
                sqlQuery.Value.Dispose();
            }
        }
        
        private void processSqlQuery(string query)
        {           
            var queriesList = query.Split('|');
            
            Parallel.ForEach(queriesList, (query) =>
            {               
                ProcessQuery(query);               
            });
        }       
        private void ProcessQuery(string query)
        {            
            var queryStream = GetQueryContentAsStream(query);           
            AddToProcessing(query, queryStream);
            
            Interlocked.Increment(ref _fileCount);
        }

        private bool IsWholeFileIgnored(string filePath, IEnumerable<IExtendedRuleException> ignoredRules)
        {
            var ignoredRulesEnum = ignoredRules.ToArray();
            if (!ignoredRulesEnum.Any())
            {
                return false;
            }
            
            var lineOneRuleIgnores = ignoredRulesEnum.OfType<GlobalRuleException>().Where(x => 1 == x.StartLine).ToArray();
            if (!lineOneRuleIgnores.Any())
            {
                return false;
            }

            var lineCount = 0;
            using (var reader = new StreamReader(GetQueryContentAsStream(filePath)))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }

            return lineOneRuleIgnores.Any(x => x.EndLine == lineCount);
        }
        
        private void AddToProcessing(string query, Stream queryStream)
        {            
            queryStreams.TryAdd(query, queryStream);
        }

        private void HandleProcessing(string filePath, Stream fileStream)
        {           

            var ignoredRules = ruleExceptionFinder.GetIgnoredRuleList(fileStream).ToList();
            if (IsWholeFileIgnored(filePath, ignoredRules))
            {
                return;
            }
            ProcessRules(fileStream, ignoredRules, filePath);
            ProcessPlugins(fileStream, ignoredRules, filePath);
        }       

        private void ProcessRules(Stream fileStream, IEnumerable<IRuleException> ignoredRules, string filePath)
        {
            ruleVisitor.VisitRules(filePath, ignoredRules, fileStream);
        }

        private void ProcessPlugins(Stream fileStream, IEnumerable<IRuleException> ignoredRules, string filePath)
        {
            TextReader textReader = new StreamReader(fileStream);
            pluginHandler.ActivatePlugins(new PluginContext(filePath, ignoredRules, textReader));
        }

        private Stream GetQueryContentAsStream(string query)
        {            
            return new MemoryStream(Encoding.ASCII.GetBytes(query));
        }
    }
}
