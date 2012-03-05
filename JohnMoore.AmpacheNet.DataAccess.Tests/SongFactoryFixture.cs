using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class SongFactoryFixture
	{
		[Test()]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void SongFactoryConstructBadXmlTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<albums>
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
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void PlaylistFactoryConstructMissingTitleTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.Fail();
		}
		
		[Test]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void PlaylistFactoryConstructMissingUrlTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.Fail();
		}
		
		[Test]
		public void PlaylistFactoryConstructNormalTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingArtistTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(0));
			Assert.That(actual.ArtistName, Is.Null);
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingAlbumTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(0));
			Assert.That(actual.AlbumName, Is.Null);
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(0));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingTagsTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(0));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingTrackTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(0));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingLengthTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.Zero));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingSizeTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingArtTest ()
		{
			var target = new SongFactory();
			var raw = XElement.Parse(@"
<song id=""3180"">
        <title>Hells Bells</title>
        <artist id=""129348"">AC/DC</artist>
        <album id=""2910"">Back in Black</album>
        <tag id=""2481"" count=""3"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <track>4</track>
        <time>234</time>
        <url>http://localhost/play/index.php?oid=123908...</url>
        <size>Song Filesize in Bytes</size>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</song>");
			var actual = target.Construct(raw);
			Assert.That(actual.Name, Is.EqualTo("Hells Bells"));
			Assert.That(actual.AlbumId, Is.EqualTo(2910));
			Assert.That(actual.AlbumName, Is.EqualTo("Back in Black"));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtUrl, Is.Null);
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			Assert.That(actual.TrackLength, Is.EqualTo(TimeSpan.FromSeconds(234)));
			Assert.That(actual.TrackNumber, Is.EqualTo(4));
			Assert.That(actual.Url, Is.EqualTo("http://localhost/play/index.php?oid=123908..."));
		}
		
	}
}

