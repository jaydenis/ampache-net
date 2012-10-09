//
// AlbumArtLoaderFixture.cs
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
	public class AlbumArtLoaderFixture
	{
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultWhenInitializedTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			Assert.That(model.AlbumArtStream, Is.Not.SameAs(defaultStream));
			var target = new AlbumArtLoader(model, defaultStream);
			Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream));
		}
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultArtWhenNoArtAvailableTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = string.Empty;
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream));
		}
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultArtWhenLoadingArtTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersister<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IEntity>()).Returns(false);
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			int timesCalled = 0;
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(x => { ++timesCalled; Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream)); return Enumerable.Empty<AlbumArt>();});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(timesCalled, Is.EqualTo(1));
		}
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultArtWhenErrorLoadingArtTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersister<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IEntity>()).Returns(false);
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			int timesCalled = 0;
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(x => { ++timesCalled; return Enumerable.Empty<AlbumArt>();});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(timesCalled, Is.EqualTo(1));
			Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream)); 
		}
		
		[Test()]
		public void AlbumArtLoaderUsesLoadedArtAfterLoadingTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersister<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IEntity>()).Returns(false);
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			var art = new AlbumArt();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = false;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
		}
		
		[Test()]
		public void AlbumArtLoaderRespectsCachingPerferencesTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersister<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IEntity>()).Returns(false);
			int timesCalled = 0;
			persistor.When(x => x.Persist(Arg.Any<AlbumArt>())).Do( x => { ++timesCalled; });
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			var art = new AlbumArt();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = false;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
			Assert.That(timesCalled, Is.EqualTo(0));
		}
		
		[Test()]
		public void AlbumArtLoaderIgnoresCachingPersistedItemTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersister<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IEntity>()).Returns(true);
			var art = new AlbumArt();
			persistor.IsPersisted(Arg.Is(art)).Returns(true);
			int timesCalled = 0;
			persistor.When(x => x.Persist(Arg.Any<AlbumArt>())).Do( x => { ++timesCalled; });
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = true;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
			Assert.That(timesCalled, Is.EqualTo(0));
		}
		
		[Test()]
		public void AlbumArtLoaderCachesNewArtTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersister<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IEntity>()).Returns(false);
			var art = new AlbumArt();
			persistor.IsPersisted(Arg.Is(art)).Returns(false);
			int timesCalled = 0;
			persistor.When(x => x.Persist(Arg.Any<AlbumArt>())).Do( x => { ++timesCalled; });
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			art.ArtStream = new MemoryStream(new byte[8]);
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = true;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
			Assert.That(timesCalled, Is.EqualTo(1));
		}
	}
}

