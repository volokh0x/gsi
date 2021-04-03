using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class InitCli: Command 
	{
		public string Path;
		public bool IsBare;
		public bool ShowHelp;
		
		public InitCli () : base ("init", "init some repo")
		{
			Options = new OptionSet () {
				"usage: gsi init [dir] [OPTIONS]",
				"",
				{ "bare|b",
				"create a bare repo",
				b => IsBare = b != null },
				{"help|h|?",
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
				if (extra.Count==0)
					Path=Environment.CurrentDirectory;
				else if (extra.Count==1)
					Path=extra[0];
				else 
				{
					Console.Error.WriteLine("gsi init: to many arguments");
					return 1;
				}
				GitCommand.InitCmd(Path,IsBare);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi init: {e.ToString()}"); // e.Message
				return 1;
			}
		}

	}
}