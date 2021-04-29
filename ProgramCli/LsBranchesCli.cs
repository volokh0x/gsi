using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class LsBranchesCli: Command 
	{	
		public bool ShowHelp;
		public LsBranchesCli () : base ("ls-branches", "list all branches")
		{
			Options = new OptionSet () {
				"use as: gsi ls-branches",
				"",
				{"?|h|help",
				"get help",
				v => ShowHelp = v != null },
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse(args);
				if (ShowHelp)
				{
					Options.WriteOptionDescriptions(CommandSet.Out);
					return 0;
				}
				if (extra.Count!=0)
                {
					throw new Exception("too many arguments");
                }
				GitCommand.LsBranchesCmd();
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi ls-branches: {e.Message}"); 
				Console.Error.WriteLine ($"gsi ls-branches: aborted ...");
				return 1;
			}
		}
	}
}