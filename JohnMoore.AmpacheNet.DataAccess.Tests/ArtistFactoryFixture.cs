using NUnit.Framework;
using System;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class ArtistFactoryFixture
	{
		[Test()]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void BadAlbumXmlTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""123"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</albums>");
			var actual = target.Construct(raw);
			Assert.Fail();
		}
		[Test]
		public void NormalArtistConstructTest()
		{
			var target = new ArtistFactory();
			var raw = XElement.Parse(@"
<artist id=""12039"">
        <name>Metallica</name>
        <albums>5</albums>
        <songs>67</songs>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</artist>");
			var actual = target.Construct(raw);
			Assert.That(actual.AlbumCount, Is.EqualTo(5));
			Assert.That(actual.SongCount, Is.EqualTo(67));
			Assert.That(actual.Name, Is.EqualTo("Metallica"));
			Assert.That(actual.Tags, Is.Not.Null);
		}
		[Test]
		public void NormalArtistConstructNoAlbumsTest()
		{
			var target = new ArtistFactory();
			var raw = XElement.Parse(@"
<artist id=""12039"">
        <name>Metallica</name>
        <songs>67</songs>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</artist>");
			var actual = target.Construct(raw);
			Assert.That(actual.AlbumCount, Is.EqualTo(0));
			Assert.That(actual.SongCount, Is.EqualTo(67));
			Assert.That(actual.Name, Is.EqualTo("Metallica"));
			Assert.That(actual.Tags, Is.Not.Null);
		}
		[Test]
		public void NormalArtistConstructNoSongsTest()
		{
			var target = new ArtistFactory();
			var raw = XElement.Parse(@"
<artist id=""12039"">
        <name>Metallica</name>
        <albums>5</albums>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</artist>");
			var actual = target.Construct(raw);
			Assert.That(actual.AlbumCount, Is.EqualTo(5));
			Assert.That(actual.SongCount, Is.EqualTo(0));
			Assert.That(actual.Name, Is.EqualTo("Metallica"));
			Assert.That(actual.Tags, Is.Not.Null);
		}
	}
}

