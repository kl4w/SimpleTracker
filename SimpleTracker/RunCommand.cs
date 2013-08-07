using System;
using System.IO;
using System.Threading;
using ManyConsole;

namespace SimpleTracker
{
	public class RunCommand : ConsoleCommand
	{
		public string AnnouncementEndpoint;
		public string TorrentsDirectoryPath;

		public RunCommand()
		{
			IsCommand("run", "Run the application.");
			HasOption ("a|announcementEndpoint=", "Endpoing to announce (i.e. http;//example.com:80/).", o => AnnouncementEndpoint = o);
			HasRequiredOption ("t|torrentsDirectoryPath=", "Directory holding the trackers torrents.", o => TorrentsDirectoryPath = Path.GetFullPath (o));
			SkipsCommandSummaryBeforeRunning();
		}

		public override int Run (string[] remainingArguments)
		{
			// Make the tracker.
			new SimpleTracker (AnnouncementEndpoint, TorrentsDirectoryPath);

			// Never die.
			Thread.Sleep (Timeout.Infinite);
			return 0;
		}

		public static void Main (string[] args)
		{
			ConsoleCommandDispatcher.DispatchCommand (ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs (typeof(SimpleTracker)), args, Console.Out);
		}
	}
}

