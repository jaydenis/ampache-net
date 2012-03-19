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
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Linq;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Logic;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using System.IO;
using NSubstitute;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	[TestFixture()]
	public class BackgroundFixture
	{
		[Test()]
		public void BackgroundStartInitializesModelTest ()
		{
			var target = new BackgroundHandle(null, null);
			var initial = target.Model;
			target.Start(new MemoryStream());
			var after = target.Model;
			Assert.That(after, Is.Not.Null);
			Assert.That(initial, Is.Not.SameAs(after));
		}
		
		[Test()]
		public void BackgroundStartPopulatesConfigurationWhenLoadReturnNullTest ()
		{
			var target = new BackgroundHandle(null, null);
			target.Start(new MemoryStream());
			var exp = target.Model.Configuration;
			Assert.That(exp, Is.Not.Null);
			// Verifies initial state matches documentation
			Assert.That(exp.AllowSeeking, Is.True);
			Assert.That(exp.CacheArt, Is.True);
			Assert.That(exp.Password, Is.EqualTo(string.Empty));
			Assert.That(exp.ServerUrl, Is.EqualTo(string.Empty));
			Assert.That(exp.User, Is.EqualTo(string.Empty));
		}
		
		[Test()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void BackgroundStartFailsWithNullParameterTest ()
		{
			var target = new BackgroundHandle(null, null);
			target.Start(null);
			Assert.Fail();
		}
		
		[Test()]
		public void BackgroundStartPopulatesArtCacheLocationToDefaultTest ()
		{
			var target = new BackgroundHandle(null, null);
			target.Start(new MemoryStream());
			Assert.That(AmpacheSelectionFactory.ArtLocalDirectory, Is.EqualTo("Art"));
		}
		
		[Test()]
		public void BackgroundStartPopulatesArtLoaderTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			var exp = target.Loader;
			Assert.That(exp, Is.Not.Null);
		}
		
		[Test()]
		public void BackgroundStartPopulatesModelFactoryWhenNoUserConfigExistsTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			var exp = target.Model.Factory;
			Assert.That(exp, Is.Not.Null);
		}
		
		[Test()]
		public void BackgroundStartBeginsAutoShutOffTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			Assert.That(target.AutoShutOffCallCount, Is.EqualTo(1));
		}
		
		[Test()]
		public void BackgroundStartPopulatesModelFactoryWhenUserConfigExistsTest ()
		{
			string path = "myartpath";
			var config = new UserConfiguration();
			config.ServerUrl = "test";
			config.User = "test";
			config.Password = "test";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			factory.AuthenticateToServer(config).Returns(x => (Authenticate)null);
			var target = new BackgroundHandle(config, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			var exp = target.Model.Factory;
			Assert.That(exp, Is.Not.Null);
			var userMessage = target.Model.UserMessage;
			Assert.That(userMessage, Is.Not.Null);
			Assert.That(userMessage, Is.EqualTo(BackgroundHandle.SUCCESS_MESSAGE));
			Assert.That(target.LoadSongsCalled, Is.True);
		}
		
		[Test()]
		public void BackgroundStartPopulatesModelFactoryWhenUserConfigExistsFailsToConnectToServerTest ()
		{
			string path = "myartpath";
			var config = new UserConfiguration();
			config.ServerUrl = "test";
			config.User = "test";
			config.Password = "test";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			string message = "ERROR";
			factory.When(f => f.AuthenticateToServer(config)).Do(delegate { throw new Exception(message); });
			var target = new BackgroundHandle(config, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			var exp = target.Model.Factory;
			Assert.That(exp, Is.Not.Null);
			var userMessage = target.Model.UserMessage;
			Assert.That(userMessage, Is.Not.Null);
			Assert.That(userMessage, Is.EqualTo(message));
			Assert.That(target.LoadSongsCalled, Is.False);
		}
		
		[Test()]
		public void BackgroundStartPopulatesModelPlaylistWhenUserConfigExistsTest ()
		{
			string path = "myartpath";
			var config = new UserConfiguration();
			config.ServerUrl = "test";
			config.User = "test";
			config.Password = "test";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			factory.AuthenticateToServer(config).Returns(x => (Authenticate)null);
			var target = new BackgroundHandle(config, path, factory);
			target.Start(new MemoryStream());
			var exp = target.Model.Playlist;
			Assert.That(exp, Is.Not.Null);
		}
		
		[Test()]
		public void BackgroundListensForModelDispositionTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			target.Model.Dispose();
			Assert.That(target.FinalizedCalled, Is.True);
		}
		
		[Test()]
		public void BackgroundListensForConfigChangesTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.Configuration = new UserConfiguration();
			Assert.That(target.SavedConfigCalled, Is.True);
		}
		
		[Test()]
		public void BackgroundListensForPlaylistChangesTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.Playlist = new List<AmpacheSong>();
			Assert.That(target.SavedSongsCalled, Is.True);
		}
		
		[Test()]
		public void BackgroundListensForPlayingChangesAndCancelsShutOffTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.IsPlaying = true;
			Assert.That(target.StopShutOffCallCount, Is.EqualTo(1));
		}
		
		[Test()]
		public void BackgroundListensForPlayingChangesAndStartsShutOffTest ()
		{
			string path = "myartpath";
			var target = new BackgroundHandle(null, path);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.IsPlaying = true;
			Assert.That(target.StopShutOffCallCount, Is.EqualTo(1));
			target.Model.IsPlaying = false;
			Assert.That(target.AutoShutOffCallCount, Is.EqualTo(2));
		}
		
		private class BackgroundHandle : Background
		{
			private readonly UserConfiguration _config;
			private readonly AmpacheSelectionFactory _factory;
			public const string SUCCESS_MESSAGE = "success";
			public bool FinalizedCalled { get; private set;}
			public bool SavedConfigCalled { get; private set;}
			public bool SavedSongsCalled { get; private set;}
			public bool LoadSongsCalled { get; private set;}
			public int AutoShutOffCallCount { get; private set; }
			public int StopShutOffCallCount { get; private set; }
			
			public BackgroundHandle (UserConfiguration config, string art)
			{
				_successConnectionMessage = SUCCESS_MESSAGE;
				_config = config;
				_artCachePath = art;
				FinalizedCalled = false;
				SavedConfigCalled = false;
				SavedSongsCalled = false;
				LoadSongsCalled = false;
				AutoShutOffCallCount = 0;
				StopShutOffCallCount = 0;
			}
			
			public BackgroundHandle (UserConfiguration config, string art, AmpacheSelectionFactory factory) : this(config, art)
			{
				_factory = factory;
			}
			
			public AmpacheModel Model { get { return _model; } }
			public AlbumArtLoader Loader { get { return _loader; } }
			
			public override UserConfiguration LoadPersistedConfiguration ()
			{
				return _config;
			}
			
			public override List<AmpacheSong> LoadPersistedSongs ()
			{
				LoadSongsCalled = true;
				return new List<AmpacheSong>();
			}
			public override AmpacheSelectionFactory CreateFactory ()
			{
				return _factory ?? base.CreateFactory();
			}
			
			public override void PlatformFinalize ()
			{
				FinalizedCalled = true;
			}
			public override void PersistUserConfig (UserConfiguration config)
			{
				SavedConfigCalled = true;
			}
			public override void PersistSongs (IList<AmpacheSong> songs)
			{
				SavedSongsCalled = true;
			}
			public override void StartAutoShutOff ()
			{
				++AutoShutOffCallCount;
			}
			public override void StopAutoShutOff ()
			{
				++StopShutOffCallCount;
			}
		}
	}
}

