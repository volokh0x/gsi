using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class TrackCli: Command
	{
		public bool ShowHelp;
		public TrackCli () : base ("track", "mark files as tracked by git")
		{
			Options = new OptionSet () {
				"use as: gsi track <path1> [path2...]",
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
					throw new Exception("nothing to track");
                }
				GitCommand.TrackCmd(extra);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi track: {e.Message}");
				Console.Error.WriteLine ($"gsi track: aborted ...");
				return 1;
			}
		}
	}
}