//
// SongFactory.cs
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
    internal class SongFactory : FactoryBaseRatable<AmpacheSong>, IEntityFactory<AmpacheSong>
    {
        public ICollection<AmpacheSong> Construct(ICollection<XElement> raw)
        {
            return new HashSet<AmpacheSong>(raw.Select(n=> Construct(n)));
        }

        public AmpacheSong Construct(XElement raw)
        {
			if(raw.Name.LocalName.ToLower() != "song")
			{
				throw new System.Xml.XmlException(string.Format("{0} can not be processed into an Song", raw.Name.LocalName));
			}
            var result = BuildBase(raw);
			if(!raw.Descendants("title").Any())
			{
				throw new System.Xml.XmlException(string.Format("Song id {0} has no name defined", result.Id)); 
			}
			if(!raw.Descendants("url").Any())
			{
				throw new System.Xml.XmlException(string.Format("Song id {0} has no url defined", result.Id)); 
			}
			result.Name = raw.Descendants("title").First().Value;
            result.Url =  raw.Descendants("url").First().Value;
			if(raw.Descendants("art").Any())
			{
				result.ArtUrl = raw.Descendants("art").First().Value;
			}
			if(raw.Descendants("album").Any())
			{
				result.AlbumName = raw.Descendants("album").First().Value;
				result.AlbumId = int.Parse(raw.Descendants("album").First().Attribute("id").Value);
			}
			if(raw.Descendants("artist").Any())
			{
				result.ArtistName = raw.Descendants("artist").First().Value;
            	result.ArtistId = int.Parse(raw.Descendants("artist").First().Attribute("id").Value);
			}
            int tmp = 0;
            int.TryParse((raw.Descendants("track").FirstOrDefault() ?? new XElement("empty", 0)).Value, out tmp);
            result.TrackNumber = tmp;
            tmp = 0;
            int.TryParse((raw.Descendants("time").FirstOrDefault() ?? new XElement("empty", 0)).Value, out tmp);
            result.TrackLength = TimeSpan.FromSeconds(tmp);
            return result;
        }
    }
}
