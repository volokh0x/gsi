using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class BranchCli: Command 
	{	
		public BranchCli () : base ("branch", "branch to other branch")
		{
			Options = new OptionSet () {
				"usage: gsi branch [name]",
				"",
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse(args);
				if (extra.Count!=1)
				{
					throw new Exception("must specify one branch name to create");
				}
				GitCommand.BranchCmd(extra[0]);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi branch: {e.Message}");
				Console.Error.WriteLine ($"gsi branch: aborted ...");
				return 1;
			}
		}
	}
}