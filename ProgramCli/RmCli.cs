using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class RmCli: Command 
	{	
        public bool f;
        public bool r;
		public RmCli () : base ("rm", "remove files from taging area")
		{
			Options = new OptionSet () {
				"usage: gsi rm [files]",
				"",
                { "force|f",
				"force deletion",
				x => f=x!=null},
                { "recursive|r",
				"recursive deletion",
				x => r=x!=null},
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse (args);
				if (extra.Count==0)
                {
                    Console.WriteLine("gsi rm: nothing to remove");
                    Console.WriteLine("gsi rm: aborted ...");
					return 0;
                }
				GitCommand.RmCmd(extra,f,r);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi rm: {e.Message}"); 
				return 1;
			}
		}
	}
}