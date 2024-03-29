using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class CatFileCli : Command
    {
        public bool ShowHelp;
        public CatFileCli() : base("cat-file", "display object content by it's hash")
        {
            Options = new OptionSet() {
                "use as: gsi cat-file <hash>",
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
                    throw new Exception("must specify one object by hash to read from");
                }
                GitCommand.CatFileCmd(extra[0]);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"gsi cat-file: {e.Message}");
                Console.Error.WriteLine($"gsi cat-file: aborted ...");
                return 1;
            }
        }
    }
}