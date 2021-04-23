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
				"use as: gsi rm <file1> [file2...]",
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
                    throw new Exception("nothing to remove");
                }
				GitCommand.RmCmd(extra,f,r);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi rm: {e.Message}"); 
				Console.Error.WriteLine ($"gsi rm: aborted ...");
				return 1;
			}
		}
	}
}