//
// AmpachePlayerFixture.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2012 John Moore
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Logic;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using System.IO;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	[TestFixture()]
	public class AmpachePlayerFixture
	{
		[Test()]
		public void AmpachePlayerSettingPlayerPositionMilliSecondUpdatesPercentPlayedOnModelTest ()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			var song = new AmpacheSong();
			song.TrackLength = TimeSpan.FromSeconds(60);
			model.PlayingSong = song;
			mock.SetPlayerPositionMilliSecond(15000);
			Assert.That(model.PercentPlayed, Is.EqualTo(25));
		}
		
		[Test]
		public void AmpachePlayerPlayPauseHasNoEffectWithoutPlaylistTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			model.Playlist = null;
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(model.PlayingSong, Is.Null);
		}
		
		[Test]
		public void AmpachePlayerPlayPauseResumesPlaybackWhenPasusedTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(true);
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.UnpauseCalls, Is.EqualTo(1));
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
		}
		
		[Test]
		public void AmpachePlayerPlayPausePausesPlaybackWhenPlayingTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			model.IsPlaying = true;
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(1));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.True);
			Assert.That(model.IsPlaying, Is.False);
		}
				
		[Test]
		public void AmpachePlayerPlayPauseStartsPlaybackAtFirstSongTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(first));
			Assert.That(mock.LastStartedSong, Is.SameAs(first));
		}
		
		[Test]
		public void AmpachePlayerPlayPauseRespectsSelectedSongWhenStartingPlaybackTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(second));
			Assert.That(mock.LastStartedSong, Is.SameAs(second));
		}
		
		[Test]
		public void AmpachePlayerPlayPauseErrorStartingPlaybackWithSelectedSongTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerErrorHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.False);
			Assert.That(model.PlayingSong, Is.SameAs(second));
			Assert.That(mock.LastStartedSong, Is.SameAs(second));
		}
		
		[Test]
		public void AmpachePlayerPlayPauseErrorStartingPlaybackWithOutSelectedSongTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerErrorHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayPauseRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.False);
			Assert.That(model.PlayingSong, Is.SameAs(first));
			Assert.That(mock.LastStartedSong, Is.SameAs(first));
		}
		
		[Test]
		public void AmpachePlayerNextNotShufflingNotPlayingTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = null;
			model.IsPlaying = false;
			model.NextRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(first));
			Assert.That(mock.LastStartedSong, Is.SameAs(first));
		}
		
		[Test]
		public void AmpachePlayerNextNotShufflingPlayingTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = first;
			model.IsPlaying = true;
			model.NextRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(second));
			Assert.That(mock.LastStartedSong, Is.SameAs(second));
		}
		
		[Test]
		public void AmpachePlayerNextNotShufflingPlayingRollsToStartTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.IsPlaying = true;
			model.PercentPlayed = 90;
			model.PercentDownloaded = 100;
			model.NextRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(model.PercentPlayed, Is.EqualTo(0));
			Assert.That(model.PercentDownloaded, Is.EqualTo(0));
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(first));
			Assert.That(mock.LastStartedSong, Is.SameAs(first));
		}
		[Test]
		public void AmpachePlayerNextShufflingTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = first;
			model.IsPlaying = true;
			model.Shuffling = true;
			model.NextRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			// TODO: find a way to test randomized shuffeling
		}
		[Test]
		public void AmpachePlayerPreviousAfterSomePlayTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			mock.SetPlayerPositionMilliSecond(10000);
			model.IsPlaying = true;
			model.PreviousRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(second));
			Assert.That(mock.LastStartedSong, Is.SameAs(second));
		}
		[Test]
		public void AmpachePlayerStopWhilePlayTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.IsPlaying = true;
			model.StopRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(1));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.False);
			Assert.That(model.PlayingSong, Is.Null);
		}
		[Test]
		public void AmpachePlayerStopNotWhilePlayTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.IsPlaying = false;
			model.StopRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.False);
			Assert.That(model.PlayingSong, Is.Null);
		}
		[Test]
		public void AmpachePlayerSeekingRespectsConfigurationTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.IsPlaying = true;
			model.Configuration = new UserConfiguration();
			model.Configuration.AllowSeeking = false;
			model.RequestedSeekToPercentage = 50;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.EqualTo(second));
		}
		[Test]
		public void AmpachePlayerSeekingTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			second.TrackLength = TimeSpan.FromMinutes(1);
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			model.IsPlaying = true;
			model.Configuration = new UserConfiguration();
			model.Configuration.AllowSeeking = true;
			model.RequestedSeekToPercentage = 50;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(0));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(1));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(second));
		}
		
		[Test]
		public void AmpachePlayerPreviousAfterNoPlayTest()
		{
			var model = new AmpacheModel();
			var mock = new PlayerHandle(model);
			
			mock.SetPauseState(false);
			var first = new AmpacheSong();
			var second = new AmpacheSong();
			model.Playlist = new List<AmpacheSong>{ first, second };
			model.PlayingSong = second;
			mock.SetPlayerPositionMilliSecond(100);
			model.IsPlaying = true;
			model.PreviousRequested = true;
			System.Threading.Thread.Sleep(100);
			Assert.That(model.PlayPauseRequested, Is.False);
			Assert.That(mock.PauseCalls, Is.EqualTo(0));
			Assert.That(mock.UnpauseCalls, Is.EqualTo(0));
			Assert.That(mock.PlaySongCalls, Is.EqualTo(1));
			Assert.That(mock.StopCalls, Is.EqualTo(0));
			Assert.That(mock.SeekToCalls, Is.EqualTo(0));
			Assert.That(mock.GetPauseState(), Is.False);
			Assert.That(model.IsPlaying, Is.True);
			Assert.That(model.PlayingSong, Is.SameAs(first));
			Assert.That(mock.LastStartedSong, Is.SameAs(first));
		}
		
		#region Helper Class
		
		/// <summary>
		/// Mocking the AmpachePlayer is a bad idea due to its use of protected members.
		/// </summary>
		private class PlayerHandle : AmpachePlayer
		{
			public int UnpauseCalls { get; private set; }
			public int PauseCalls { get; private set; }
			public int StopCalls { get; private set; }
			public int SeekToCalls { get; private set; }
			public AmpacheSong LastStartedSong { get; private set; }
			public int PlaySongCalls { get; private set; }
			
			public PlayerHandle (AmpacheModel model) : base(model)
			{
				UnpauseCalls = 0;
				PauseCalls = 0;
				StopCalls = 0;
				SeekToCalls = 0;
				PlaySongCalls = 0;
				LastStartedSong = null;
			}
			
			public void SetPauseState(bool p)
			{
				_isPaused = p;
			}
			public bool GetPauseState()
			{
				return _isPaused;
			}
			
			public void SetPlayerPositionMilliSecond(int val)
			{
				PlayerPositionMilliSecond = val;
			}
			
			#region implemented abstract members of JohnMoore.AmpacheNet.Logic.AmpachePlayer
			protected override void PlaySong (AmpacheSong song)
			{
				LastStartedSong = song;
				++PlaySongCalls;
			}

			protected override void StopPlay ()
			{
				++StopCalls;
			}

			protected override void Pause ()
			{
				++PauseCalls;
			}

			protected override void Unpause ()
			{
				++UnpauseCalls;
			}

			protected override void SeekTo (int millis)
			{
				++SeekToCalls;
			}
			#endregion
		}
		
		private class PlayerErrorHandle : PlayerHandle
		{
			public PlayerErrorHandle (AmpacheModel model) : base(model) {}
			
			protected override void Pause ()
			{
				base.Pause();
				throw new Exception();
			}
			protected override void PlaySong (AmpacheSong song)
			{
				base.PlaySong(song);
				throw new Exception();
			}
			protected override void SeekTo (int millis)
			{
				base.SeekTo(millis);
				throw new Exception();
			}
			protected override void StopPlay ()
			{
				base.StopPlay();
				throw new Exception();
			}
		}
		
		#endregion
	}
}

