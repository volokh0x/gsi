using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class JsonDiffCli : Command
    {
        public bool ShowHelp;
        public List<string> exclude = new List<string>();
        public List<string> include = new List<string>();
        public bool ignore_append = false;
        public bool html_output = false;
        public JsonDiffCli() : base("json-diff", "show changes between two files on disk")
        {
            Options = new OptionSet() {
                "use as: gsi json-diff <file1|hash1> <file2|hash2> [OPTIONS]",
                "",
                {"?|h|help",
                "get help",
                v => ShowHelp = v != null },

                {"H|HTML",
                "produce HTML output",
                v => html_output = v != null },
                {"i|include",
                "attributes to include" ,
                v => include.Add(v)
                },
                {"e|exclude",
                "attributes to exclude" ,
                v => exclude.Add(v)
                },
                {"a|ignore-append",
                "ignore appended elements",
                v => ignore_append = v != null },
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
                if (extra.Count != 2)
                {
                    throw new Exception("must specify two files to compare");
                }
                GitCommand.JsonDiffCmd(extra[0], extra[1], exclude, include, ignore_append, html_output);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"gsi json-diff: {e.Message}");
                Console.Error.WriteLine($"gsi json-diff: aborted ...");
                return 1;
            }
        }
    }
}