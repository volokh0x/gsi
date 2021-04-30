using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace gsi
{
    class RestoreCli: Command 
	{
		public string Hash_prefix;
        public string file;		
        public bool ShowHelp;
		
		public RestoreCli () : base ("restore", "restore file by it's hash")
		{
			Options = new OptionSet () {
				"use as: gsi restore [OPTIONS]",
				"",
				{"hash=",
				"stored file's hash",
				v => Hash_prefix = v},
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
				if (extra.Count()!=1)
					throw new Exception("file path not provided");
                if (Hash_prefix==null)
                    throw new Exception("hash not provided");
				GitCommand.RestoreCmd(extra[0],Hash_prefix);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi restore: {e.Message}"); 
				Console.Error.WriteLine ($"gsi restore: aborted ...");
				return 1;
			}
		}

	}
}