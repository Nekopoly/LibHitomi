using System;
using System.Collections.Generic;
using LibHitomi;
using LibHitomi.GalleryList;
using Xunit;
using Xunit.Abstractions;

namespace LibHitomi.Test
{
    public class GalleryInfoTest
    {
        private readonly ITestOutputHelper logger;
        public GalleryInfoTest(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        ListDownloader listDownloader;
        [Fact]
        public async void ListDownloadAndInfoTest()
        {
            // Downlad galleries
            listDownloader = new ListDownloader();
            listDownloader.ListDownloadProgress += InitializationTest_onProgress;
            List<Gallery> galleries = new List<Gallery>();
            galleries.AddRange(await listDownloader.Download());

            // Check whether list is downloaded
            Assert.True(galleries.Count > 456900);
            Assert.DoesNotContain(null, galleries);

            // Check whether gallery information is correct
            Gallery gallery = galleries.Find(v => v.Id == 1277807);
            Assert.Equal(GalleryCrawlMethod.Normal, gallery.GalleryCrawlMethod);
            Assert.Equal(1277807, gallery.Id);
            Assert.Equal("doujinshi", gallery.Type);
            Assert.Equal("Oidemase!! 2-jigen Fuuzoku Gakuen Dai 2 Kukaku", gallery.Name);
            Assert.Equal(new string[] { "nyuu" }, gallery.Artists);
            Assert.Equal(new string[] { "nyuu koubou" }, gallery.Groups);
            Assert.Equal(new string[] { "elf yamada", "haruhi suzumiya", "lyfa", "masamune izumi", "muramasa senju", "sagiri izumi", "shino asada", "suguha kirigaya" }.Sort(), gallery.Characters.Sort());
            Assert.Equal(new string[] { "eromanga sensei", "nier automata", "ranma 12", "sword art online", "the idolmaster", "the melancholy of haruhi suzumiya", "to love-ru", "urusei yatsura", "yuru camp" }.Sort(), gallery.Parodies.Sort());
            Assert.Equal(new string[] { "female:anal", "female:body writing", "female:bunny girl", "female:mind control", "female:piercing", "full color" }.Sort(), gallery.Tags.Sort());
            logger.WriteLine("Has correct gallery");
        }
        [Fact]
        public void GalleryBlockParsingTest()
        {
            Gallery.GetGalleryByParsingGalleryBlock(1841, out Gallery gallery);
            Assert.Equal(1841, gallery.Id);
            Assert.Equal("japanese", gallery.Language);
            Assert.Equal("Dear my RIN", gallery.Name);
            Assert.Equal("doujinshi", gallery.Type);
            Assert.Equal(new string[] { "rusty soul", "alto seneka" }.Sort(), gallery.Artists.Sort());
            Assert.Equal(new string[] { "kodomo no jikan" }, gallery.Parodies);
            Assert.Equal(new string[] { "female:loli", "female:bunny girl", "female:schoolgirl uniform", "female:stockings", "male:glasses", "male:teacher" }.Sort(), gallery.Tags.Sort());
        }
        private void InitializationTest_onProgress(ListDownloadProgressType progressType, int? data)
        {
            string line = "onProgress Event : " + progressType.ToString();
            if (data.HasValue) line += $" (data={data.Value})";
            logger.WriteLine(line);
        }
    }
}
