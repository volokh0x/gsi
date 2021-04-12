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
				GitCommand.BranchCmd(extra.Count!=0?extra[0]:null);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi branch: {e.Message}");
				return 1;
			}
		}
	}
}