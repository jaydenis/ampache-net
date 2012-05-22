using NUnit.Framework;
using System;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class AlbumFactoryFixture
	{
		[Test()]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void AlbumFactoryConstructBadXmlTest ()
		{
			var target = new AlbumFactory();
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
		[Test()]
		public void AlbumFactoryConstructNormalAlbumTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
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
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3)); // NOTE: not sure that is correct, but it will work
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingArtistTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(0));
			Assert.That(actual.ArtistName, Is.EqualTo(string.Empty));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3)); // NOTE: not sure that is correct, but it will work
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingYearTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1900));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingTracksTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3)); // NOTE: not sure that is correct, but it will work
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(0));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingDiskTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3)); 
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingTagsTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(0)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingArtTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.Null);
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingRatingTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
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
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
			Assert.That(actual.Rating, Is.EqualTo(0));
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
		
		[Test()]
		public void AlbumFactoryConstructMissingPreciseRatingTest ()
		{
			var target = new AlbumFactory();
			var raw = XElement.Parse(@"
<album id=""2910"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <rating>3</rating>
</album>");
			var actual = target.Construct(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(2910));
			Assert.That(actual.ArtId, Is.EqualTo(2910));
			Assert.That(actual.ArtistId, Is.EqualTo(129348));
			Assert.That(actual.ArtistName, Is.EqualTo("AC/DC"));
			Assert.That(actual.Name, Is.EqualTo("Back in Black"));
			Assert.That(actual.PerciseRating, Is.EqualTo(0));
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.Tags.Count, Is.EqualTo(3)); // NOTE: test the tag factory in a different fixture
			Assert.That(actual.TrackCount, Is.EqualTo(12));
			Assert.That(actual.Year, Is.EqualTo(1984));
			Assert.That(actual.ArtUrl, Is.EqualTo("http://localhost/image.php?id=129348"));
		}
	}
}

