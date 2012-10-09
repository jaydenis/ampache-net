using System;
using System.Collections.Generic;
using System.Text;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.Logic
{
	public static class SongHandshakeRemapper
	{
		/// <summary>
		/// 0: Server URL
		/// 1: Session Auth
		/// 2: song id
		/// </summary>
		private const string URL_FORMAT = "{0}/play/index.php?ssid={1}&oid={2}";
		private const string ART_URL = "{0}?id={1}&auth={2}&name=art.jpg";
		private const string UID = @"&uid=";
		private const string NAME = @"&name=";

		public static void MapToHandshake(this AmpacheSong song, Handshake handshake)
		{
			if(handshake == null){
				throw new ArgumentNullException("handshake");
			}
			var urlParts = (song.Url ?? string.Empty).Split('?');
			if(urlParts.Length == 2){
				var parameterMap = new Dictionary<string, string>();
				var strings = urlParts[1].Split('&');
				foreach (var str in strings) {
					var tup = str.Split('=');
					if(tup.Length == 2){
						parameterMap.Add(tup[0].ToLower(), tup[1]);
					}
				}
				var builder = new StringBuilder(string.Format(URL_FORMAT, handshake.Server, handshake.Passphrase, song.Id));
				if(parameterMap.ContainsKey("uid")){
					builder.Append(UID);
					builder.Append(parameterMap["uid"]);
				}
				if(parameterMap.ContainsKey("name")){
					builder.Append(NAME);
					builder.Append(parameterMap["name"]);
				}
				song.Url = builder.ToString();
			}
			urlParts = (song.ArtUrl ?? string.Empty).Split('?');
			if(urlParts.Length == 2){
				song.ArtUrl = string.Format(ART_URL, urlParts[0], song.ArtId, handshake.Passphrase);
			}
		}
		
	}
}

