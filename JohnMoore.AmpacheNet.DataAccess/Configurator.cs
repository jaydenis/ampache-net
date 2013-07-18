using System;
using JohnMoore.AmpacheNet.Entities;
using System.IO;
using System.Data.SQLite;
using Athena.IoC;

namespace JohnMoore.AmpacheNet.DataAccess
{
    public static class Configurator
    {
        public static string ArtLocalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".AmpacheNet");
        public static string DatabaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string DbConnString { get { return string.Format("Data Source={0}", Path.Combine(DatabaseDirectory, "ampachenet.db3")); } }

        public static void Configure(Container container)
        {
            container.Register<IEntityFactory<AmpacheArtist>>().To<ArtistFactory>().AsSingleton();
            container.Register<IAmpacheSelector<AmpacheArtist>>().To<ArtistSelector>();

            container.Register<IEntityFactory<AmpacheAlbum>>().To<AlbumFactory>().AsSingleton();
            container.Register<IAmpacheSelector<AmpacheAlbum>>().To<AlbumSelector>();

            container.Register<IEntityFactory<AmpacheSong>>().To<SongFactory>().AsSingleton();
            container.Register<IAmpacheSelector<AmpacheSong>>().To<SongSelector>();
            container.Register<IPersister<AmpacheSong>>().To<SongPersister>();

            container.Register<IEntityFactory<AmpachePlaylist>>().To<PlaylistFactory>().AsSingleton();
            container.Register<IAmpacheSelector<AmpachePlaylist>>().To<PlaylistSelector>();

            container.Register<IAmpacheSelector<AlbumArt>>().To<AlbumArtRepository>().ConstructAs(j => new AlbumArtRepository(ArtLocalDirectory));
            container.Register<IPersister<AlbumArt>>().To<AlbumArtRepository>().ConstructAs(j => new AlbumArtRepository(ArtLocalDirectory));

            container.Register<System.Data.IDbConnection>().To<SQLiteConnection>().ConstructAs(j => new SQLiteConnection(DbConnString));

            container.Register<IPersister<UserConfiguration>>().To<UserConfigurationPersister>();
        }
    }
}
