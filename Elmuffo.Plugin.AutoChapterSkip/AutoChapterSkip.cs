using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;

namespace Elmuffo.Plugin.AutoChapterSkip
{
    /// <summary>
    /// Automatically skip chapters matching regex.
    /// Commands clients to seek to the end of matched chapters as soon as they start playing them.
    /// </summary>
    public class AutoChapterSkip : IServerEntryPoint
    {
        private readonly object _currentPositionsLock = new();
        private readonly Dictionary<string, long?> _currentPositions;
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoChapterSkip"/> class.
        /// </summary>
        /// <param name="sessionManager">Session manager.</param>
        public AutoChapterSkip(
            ISessionManager sessionManager)
        {
            _currentPositions = new Dictionary<string, long?>();
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Set it up.
        /// </summary>
        /// <returns>Task.</returns>
        public Task RunAsync()
        {
            _sessionManager.PlaybackStopped += SessionManager_PlaybackStopped;
            _sessionManager.PlaybackProgress += SessionManager_PlaybackProgress;
            return Task.CompletedTask;
        }

        private void SessionManager_PlaybackProgress(object? sender, PlaybackProgressEventArgs e)
        {
            var match = Plugin.Instance!.Configuration.Match;

            if (string.IsNullOrEmpty(match))
            {
                return;
            }

            var chapters = e.Session.NowPlayingItem.Chapters;
            var regex = new Regex(match);
            var chapter = chapters.LastOrDefault(c => c.StartPositionTicks < e.PlaybackPositionTicks);

            if (chapter == null || !regex.IsMatch(chapter.Name))
            {
                return;
            }

            var send = (PlaystateCommand command, long? ticks) =>
            {
                Lock(() => _currentPositions[e.Session.Id] = ticks);

                _sessionManager.SendPlaystateCommand(
                   e.Session.Id,
                   e.Session.Id,
                   new PlaystateRequest
                   {
                       Command = command,
                       ControllingUserId = e.Session.UserId.ToString("N"),
                       SeekPositionTicks = ticks
                   },
                   CancellationToken.None);
            };

            var remainingChapters = chapters.Skip(chapters.IndexOf(chapter) + 1);
            var nextChapter = remainingChapters.FirstOrDefault(c => !regex.IsMatch(c.Name));
            var nextChapterTicks = nextChapter?.StartPositionTicks;

            if (nextChapterTicks == null)
            {
                if (!remainingChapters.Any())
                {
                    send(PlaystateCommand.Stop, null);
                }

                return;
            }

            long? previousChapterTicks = null;

            Lock(() => _currentPositions.TryGetValue(e.Session.Id, out previousChapterTicks));

            if (e.PlaybackPositionTicks <= previousChapterTicks)
            {
                return;
            }

            send(PlaystateCommand.Seek, nextChapterTicks);
        }

        private void SessionManager_PlaybackStopped(object? sender, PlaybackStopEventArgs e)
        {
            Lock(() => _currentPositions.Remove(e.Session.Id));
        }

        private void Lock(Action work)
        {
            lock (_currentPositionsLock)
            {
                work();
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose.
        /// </summary>
        /// <param name="disposing">Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _sessionManager.PlaybackStopped -= SessionManager_PlaybackStopped;
            _sessionManager.PlaybackProgress -= SessionManager_PlaybackProgress;
        }
    }
}
