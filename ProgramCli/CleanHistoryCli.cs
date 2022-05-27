using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace gsi
{
    class CleanHistoryCli : Command
    {
        public bool ShowHelp;
        bool all;
        public List<string> paths = new List<string>();
        public CleanHistoryCli() : base("clean-history", "remove temporary files from history")
        {
            Options = new OptionSet() {
                "use as: gsi clean-history <path1> [path2...] | --all",
                "",
                {"?|h|help",
                "get help",
                v => ShowHelp = v != null },
                {"a|all",
                "clean all temporary files",
                a => all = a != null },
            };
        }
        public override int Invoke(IEnumerable<string> args)
        {
            try
            {
                var paths = Options.Parse(args);
                if (ShowHelp)
                {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return 0;
                }
                if (!all && paths.Count == 0)
                {
                    throw new Exception("nothing to clean");
                }
                GitCommand.CleanHistoryCmd(all, paths);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"gsi clean-history: {e}");  // e.Message
                Console.Error.WriteLine($"gsi clean-history: aborted ...");
                return 1;
            }
        }
    }
}