using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class UnTrackCli: Command 
	{	
		public bool ShowHelp;
		public UnTrackCli () : base ("untrack", "mark files as untracked by git")
		{
			Options = new OptionSet () {
				"use as: gsi untrack <path1> [path2...]",
				"",
				{"?|h|help",
				"get help",
				v => ShowHelp = v != null },
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse (args);
				if (ShowHelp)
				{
					Options.WriteOptionDescriptions(CommandSet.Out);
					return 0;
				}
				if (extra.Count==0)
                {
					throw new Exception("nothing to untrack");
                }
				GitCommand.UnTrackCmd(extra);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi untrack: {e.Message}"); 
				Console.Error.WriteLine ($"gsi untrack: aborted ...");
				return 1;
			}
		}
	}
}