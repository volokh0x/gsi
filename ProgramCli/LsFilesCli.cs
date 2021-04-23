using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class LsFilesCli: Command 
	{	
		public LsFilesCli () : base ("ls-files", "list staging area")
		{
			Options = new OptionSet () {
				"use as: gsi ls-files",
				""
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse(args);
				if (extra.Count!=0)
                {
					throw new Exception("too many arguments");
                }
				GitCommand.LsFilesCmd();
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi ls-files: {e.Message}"); 
				Console.Error.WriteLine ($"gsi ls-files: aborted ...");
				return 1;
			}
		}
	}
}