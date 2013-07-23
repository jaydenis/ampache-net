//
// AlbumFactory.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2010 John Moore
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

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess
{
    internal class AlbumFactory : FactoryBaseRatable<AmpacheAlbum>, IEntityFactory<AmpacheAlbum>
    {
        public ICollection<AmpacheAlbum> Construct(ICollection<XElement> raw)
        {
            return new HashSet<AmpacheAlbum>(raw.Select(n=>Construct(n)).Where(n => n.TrackCount > 0));
        }

        public AmpacheAlbum Construct(XElement raw)
        {
			if(raw.Name.LocalName.ToLower() != "album")
			{
				throw new System.Xml.XmlException(string.Format("{0} can not be processed into an Album", raw.Name.LocalName));
			}
            var result = BuildBase(raw);
            if (raw.Descendants("artist").Any()) 
			{
                var elem = raw.Descendants("artist").First();
				result.ArtistId = int.Parse (elem.Attribute("id").Value);
                
				result.ArtistName = elem.Value;
            }
			else
			{
				result.ArtistId = 0;
				result.ArtistName = string.Empty;
			}
            if (raw.Descendants("name").Any()) 
			{
				result.Name = raw.Descendants ("name").First ().Value;
            }
            int yr = 1900;
			if(raw.Descendants("year").Any())
			{
            	int.TryParse(raw.Descendants("year").First().Value, out yr);
			}
            result.Year = yr;
			if(raw.Descendants("art").Any())
			{
            	result.ArtUrl = raw.Descendants("art").First().Value;
			}
			if(raw.Descendants("tracks").Any())
			{
                var tmp = raw.Descendants("tracks").First().Value;
                int tc = 0;
                int.TryParse(tmp, out tc);
                result.TrackCount = tc;
			}
            return result;
        }
    }
}
