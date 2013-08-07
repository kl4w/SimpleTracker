using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using MonoTorrent.Common;
using MonoTorrent.TorrentWatcher;
using MonoTorrent.Tracker;
using MonoTorrent.Tracker.Listeners;

namespace SimpleTracker
{
	public class SimpleTracker
	{
		public readonly Tracker Tracker;
		public readonly TorrentFolderWatcher Watcher;
		public readonly ConcurrentDictionary<string, InfoHashTrackable> Mappings;

		public SimpleTracker (string announcementEndpoint, string torrentsDirectoryPath)
		{
			// Make the listner.
			var listener = new HttpListener(announcementEndpoint);

			// Make the tracker.
			Tracker = new Tracker ();
			Tracker.AllowUnregisteredTorrents = true;
			Tracker.RegisterListener (listener);

			// Make mappings.
			Mappings = new ConcurrentDictionary<string, InfoHashTrackable> ();

			// Make watcher.
			Watcher = new TorrentFolderWatcher (torrentsDirectoryPath, "*.torrent");
			Watcher.TorrentFound += (sender, e) =>
			{
				try
				{
					// Wait for file to finish copying.
					System.Threading.Thread.Sleep (500);

					// Make InfoHashTrackable from torrent.
					var torrent = Torrent.Load (e.TorrentPath);
					var trackable = new InfoHashTrackable (torrent);

					// Add to tracker.
					lock (Tracker)
						Tracker.Add (trackable);

					// Save to mappings.
					Mappings[e.TorrentPath] = trackable;
				}
				catch (Exception exception)
				{
					Debug.WriteLine ("Error loading torrent from disk: {0}", exception.Message);
					Debug.WriteLine ("Stacktrace: {0}", exception.ToString ());
				}
			};
			Watcher.TorrentLost += (sender, e) =>
			{
				try
				{
					// Get from mappings.
					var trackable = Mappings[e.TorrentPath];

					// Remove from tracker.
					lock(Tracker)
						Tracker.Remove(trackable);

					// Remove from mappings.
					Mappings.TryRemove(e.TorrentPath, out trackable);
				}
				catch(Exception exception)
				{
					Debug.WriteLine ("Error uploading torrent from disk: {0}", exception.Message);
					Debug.WriteLine ("Stacktrace: {0}", exception.ToString ());
				}
			};

			// Register close events.
			AppDomain.CurrentDomain.ProcessExit += (sender, e) => Tracker.Dispose ();

			// Run.
			listener.Start();
			Watcher.Start();
			Watcher.ForceScan();
		}
	}
}