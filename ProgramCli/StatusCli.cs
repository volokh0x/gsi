using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class StatusCli: Command 
	{	
		public StatusCli () : base ("status", "get status of files")
		{
			Options = new OptionSet () {
				"use as: gsi status",
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
				GitCommand.SatusCmd();
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi status: {e.Message}"); 
				Console.Error.WriteLine ($"gsi status: aborted ...");
				return 1;
			}
		}
	}
}