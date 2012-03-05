using System;
using System.IO;

namespace JohnMoore.AmpacheNet.Entities
{
	public class AlbumArt : IEntity
	{
		public Stream ArtStream { get; set; }
		
		public int AlbumId { get; set; }

		#region IEntity implementation
		public int Id { get { return AlbumId; } set {} }
	
		public string Name { get; set; }
		#endregion
	}
}

