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
				"usage: gsi rstatus",
				""
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse(args);
				if (extra.Count!=0)
                {
                    Console.WriteLine("gsi status: wrong usage");
                    Console.WriteLine("gsi stats: aborted ...");
					return 0;
                }
				GitCommand.SatusCmd();
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi status: {e.Message}"); 
				return 1;
			}
		}
	}
}