using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class SwitchCli : Command
    {
        public bool ShowHelp;
        public SwitchCli() : base("switch", "switch to other branch")
        {
            Options = new OptionSet() {
                "use as: gsi switch [branch]",
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
                if (extra.Count != 1)
                {
                    throw new Exception("must specify one branch to checkout");
                }
                GitCommand.SwitchCmd(extra[0]);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"gsi switch: {e.Message}"); 
                Console.Error.WriteLine($"gsi switch: aborted ...");
                return 1;
            }
        }

    }
}