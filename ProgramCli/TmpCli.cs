using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class TmpCli : Command
    {
        public bool ShowHelp;
        public TmpCli() : base("tmp", "mark files as temporary by git")
        {
            Options = new OptionSet() {
                "use as: gsi tmp <path1> [path2...]",
                "",
                {"?|h|help",
                "get help",
                v => ShowHelp = v != null },
            };
        }
        public override int Invoke(IEnumerable<string> args)
        {
            try
            {
                var extra = Options.Parse(args);
                if (ShowHelp)
                {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return 0;
                }
                if (extra.Count == 0)
                {
                    throw new Exception("nothing to mark temporary");
                }
                GitCommand.TmpCmd(extra);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"gsi tmp: {e.Message}");
                Console.Error.WriteLine($"gsi tmp: aborted ...");
                return 1;
            }
        }
    }
}