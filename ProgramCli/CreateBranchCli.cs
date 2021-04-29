using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class CreateBranchCli: Command 
	{	
		public bool ShowHelp;
		public CreateBranchCli () : base ("create-branch", "create a new line of development from current one")
		{
			Options = new OptionSet () {
				"usage: gsi create-branch [name]",
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
				if (extra.Count!=1)
					throw new Exception("must specify one branch name to create");
				GitCommand.CreateBranchCmd(extra[0]);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi create-branch: {e.Message}");
				Console.Error.WriteLine ($"gsi create-branch: aborted ...");
				return 1;
			}
		}
	}
}