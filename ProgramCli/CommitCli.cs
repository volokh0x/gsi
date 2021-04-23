using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class CommitCli: Command 
	{
		public string Message;		
        public bool ShowHelp;
		
		public CommitCli () : base ("commit", "commit changes")
		{
			Options = new OptionSet () {
				"usage: gsi commit [OPTIONS]",
				"",
				{"message=|m=",
				"message",
				m => Message = m},
				{"help|h|?",
				"get help",
				v => ShowHelp = v != null},
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
				if (Message==null)
					throw new Exception("message was not passed");
				GitCommand.CommitCmd(Message);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi commit: {e.Message}"); 
				Console.Error.WriteLine ($"gsi commit: aborted ...");
				return 1;
			}
		}

	}
}