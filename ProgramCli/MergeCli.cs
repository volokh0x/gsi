using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class MergeCli: Command 
	{	
		public bool ShowHelp;
		public MergeCli () : base ("merge", "merge with some branch")
		{
			Options = new OptionSet () {
				"use as: gsi merge <branch>",
				"",
				{"?|h|help",
				"get help",
				v => ShowHelp = v != null },
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try 
			{
				var extra = Options.Parse (args);
				if (ShowHelp)
				{
					Options.WriteOptionDescriptions(CommandSet.Out);
					return 0;
				}
				if (extra.Count!=1)
                {
					throw new Exception("must specify one branch to merge in");
                }
				GitCommand.MergeCmd(extra[0]);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi merge: {e.Message}"); 
				Console.Error.WriteLine ($"gsi merge: aborted ...");
				return 1;
			}
		}

	}
}