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
using NSubstitute;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	[TestFixture()]
	public class BackgroundFixture
	{
		[Test()]
		public void BackgroundStartInitializesModelTest ()
		{
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, null, factory);
			var initial = target.Model;
			target.Start(new MemoryStream());
			var after = target.Model;
			Assert.That(after, Is.Not.Null);
			Assert.That(initial, Is.Not.SameAs(after));
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
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, null, factory);
			target.Start(new MemoryStream());
			Assert.That(AmpacheSelectionFactory.ArtLocalDirectory, Is.EqualTo("Art"));
		}
		
		[Test()]
		public void BackgroundStartPopulatesArtLoaderTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			var exp = target.Loader;
			Assert.That(exp, Is.Not.Null);
		}
		
		[Test()]
		public void BackgroundStartPopulatesModelFactoryWhenNoUserConfigExistsTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			var exp = target.Model.Factory;
			Assert.That(exp, Is.Not.Null);
		}
		
		[Test()]
		public void BackgroundStartBeginsAutoShutOffTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, path, factory);
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
			var persister = Substitute.For<IPersister<AmpacheSong>>();
			factory.GetPersistorFor<AmpacheSong>().Returns(persister);
			var sngs = new List<AmpacheSong>();
			sngs.Add(new AmpacheSong(){Url = "http://test"});
			sngs.Add(new AmpacheSong(){Url = "http://test"});
			persister.SelectAll().Returns(sngs);
			factory.AuthenticateToServer(config).Returns(x => (Authenticate)null);
			var mockHs = new MockHandShake();
			mockHs.Setup("test", "test");
			factory.Handshake.Returns(mockHs);
			var target = new BackgroundHandle(config, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			var exp = target.Model.Factory;
			Assert.That(exp, Is.Not.Null);
			var userMessage = target.Model.UserMessage;
			Assert.That(userMessage, Is.Not.Null);
			Assert.That(userMessage, Is.EqualTo(BackgroundHandle.SUCCESS_MESSAGE));
			Assert.That(target.Model.Playlist, Is.EquivalentTo(sngs));
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
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(config);
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
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			persister.SelectBy(Arg.Any<int>()).Returns(config);
			var factory = Substitute.For<AmpacheSelectionFactory>();
			factory.AuthenticateToServer(config).Returns(x => (Authenticate)null);
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			var target = new BackgroundHandle(config, path, factory);
			target.Start(new MemoryStream());
			var exp = target.Model.Playlist;
			Assert.That(exp, Is.Not.Null);
		}
		
		[Test()]
		public void BackgroundListensForModelDispositionTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			target.Model.Dispose();
			Assert.That(target.FinalizedCalled, Is.True);
		}
		
		[Test()]
		public void BackgroundListensForConfigChangesTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			bool persisted = false;
			var config = new UserConfiguration();
			persister.When(x => x.Persist(config)).Do(p => persisted = true);
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.Configuration = config;
			Assert.That(persisted, Is.True);
		}
		
		[Test()]
		public void BackgroundListensForPlaylistChangesTest ()
		{
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<AmpacheSong>>();
			var sng = new AmpacheSong();
			factory.GetPersistorFor<AmpacheSong>().Returns(persister);
			bool persisted = false;
			persister.When(p => p.Persist(sng)).Do(p => persisted = true);
			string path = "myartpath";
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			var sngs = new List<AmpacheSong>();
			sngs.Add(sng);
			target.Model.Playlist = sngs;
			Assert.That(persisted, Is.True);
		}
		
		[Test()]
		public void BackgroundListensForPlayingChangesAndCancelsShutOffTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.IsPlaying = true;
			Assert.That(target.StopShutOffCallCount, Is.EqualTo(1));
		}
		
		[Test()]
		public void BackgroundListensForPlayingChangesAndStartsShutOffTest ()
		{
			string path = "myartpath";
			var factory = Substitute.For<AmpacheSelectionFactory>();
			var persister = Substitute.For<IPersister<UserConfiguration>>();
			factory.GetPersistorFor<UserConfiguration>().Returns(persister);
			persister.SelectBy(Arg.Any<int>()).Returns(new UserConfiguration { ServerUrl = string.Empty });
			var target = new BackgroundHandle(null, path, factory);
			target.Start(new MemoryStream());
			System.Threading.Thread.Sleep(100);
			target.Model.IsPlaying = true;
			Assert.That(target.StopShutOffCallCount, Is.EqualTo(1));
			target.Model.IsPlaying = false;
			Assert.That(target.AutoShutOffCallCount, Is.EqualTo(2));
		}

		private class MockHandShake : Handshake
		{
			public void Setup (string test, string test2)
			{
				Passphrase = test;
				Server = test2;
			}
		}

		private class BackgroundHandle : Background
		{
			private readonly UserConfiguration _config;
			private readonly AmpacheSelectionFactory _factory;
			public const string SUCCESS_MESSAGE = "success";
			public bool FinalizedCalled { get; private set;}
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

			public override AmpacheSelectionFactory CreateFactory ()
			{
				return _factory ?? base.CreateFactory();
			}
			
			public override void PlatformFinalize ()
			{
				FinalizedCalled = true;
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

