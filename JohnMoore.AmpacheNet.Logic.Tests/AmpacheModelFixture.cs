using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Logic;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	/// <summary>
	/// Ampache model fixture.
	/// Verifies that changing properties correctly triggers the change notification
	/// </summary>
	[TestFixture()]
	public class AmpacheModelFixture
	{
		[Test()]
		public void AmapcheModelFactoyNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.FACTORY));
			target.Factory = new JohnMoore.AmpacheNet.DataAccess.AmpacheSelectionFactory(null);
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelPlayingSongNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PLAYING_SONG));
			target.PlayingSong = new AmpacheSong();
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelPlayListNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PLAYLIST));
			target.Playlist = new List<AmpacheSong>();
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelShufflingNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.SHUFFELING));
			target.Shuffling = true;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelConfigurationNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.CONFIGURATION));
			target.Configuration = new UserConfiguration();
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelAlbumArtStreamNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.ALBUM_ART_STREAM));
			target.AlbumArtStream = new System.IO.MemoryStream();
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelPlayPauseRequestedNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PLAY_PAUSE_REQUESTED));
			target.PlayPauseRequested = true;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelNextRequestedNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.NEXT_REQUESTED));
			target.NextRequested = true;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelPerviousRequestedNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PREVIOUS_REQUESTED));
			target.PreviousRequested = true;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelIsPlayingNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.IS_PLAYING));
			target.IsPlaying = true;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelStopRequestedNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.STOP_REQUESTED));
			target.StopRequested = true;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelPercentDownloadNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PERCENT_DOWNLOADED));
			target.PercentDownloaded = 1000;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelPercentPlayedNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PERCENT_PLAYED));
			target.PercentPlayed = 1000;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelRequestSeekToPercentageNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.REQUESTED_SEEK_TO_PERCENTAGE));
			target.RequestedSeekToPercentage = .50;
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelUserMessageNotifiesOnChange ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.USER_MESSAGE));
			target.UserMessage = "message";
			Assert.That(timesNotified, Is.EqualTo(1));
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelIsNotDisposedAtDefaultTest ()
		{
			var target = new AmpacheModel();
			Assert.That(target.IsDisposed, Is.False);
		}
		[Test()]
		public void AmapcheModelIsDisposedAfterDisposalTest ()
		{
			var target = new AmpacheModel();
			Assert.That(target.IsDisposed, Is.False);
			target.Dispose();
			Assert.That(target.IsDisposed, Is.True);
		}
		[Test()]
		public void AmapcheModelDisposalDisablesListenersTest ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.Dispose();
			timesNotified = 0;
			target.UserMessage = "message";
			Assert.That(timesNotified, Is.EqualTo(0));
		}
		[Test()]
		public void AmapcheModelDisposalClearsPlaylistTest ()
		{
			int timesNotified = 0;
			var target = new AmpacheModel();
			target.PropertyChanged += (sender, e) => ++timesNotified;
			target.PropertyChanged += (sender, e) => Assert.That(e.PropertyName, Is.EqualTo(AmpacheModel.PLAYLIST));
			target.Dispose();
			Assert.That(timesNotified, Is.EqualTo(1));
		}
	}
}

