using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Xunit;
using LibHitomi;
using System.Linq;

namespace LibHitomi.Test
{
    public class DownloadTest : IClassFixture<GalleriesFixture>
    {
        private Gallery gallery;
        private Gallery anime;
        private Downloader.Downloader downloader;
        private string tempDir = Path.GetTempPath();
        private string[] sha256sums = new string[] {"1c6687e6872eb63ce6ed8c29875dda5abfa4c412ac8a6c2e74274852d97c9a63",
                                                    "c70bdc2f4c0a286c850c562ec027e0a28e6e13c12bc93bfa2730e40c6b1c07bf",
                                                    "c2c040adc282522a1b531fcf514187eb07e5269db364906dbcf7b0f4dbfb309a",
                                                    "4afaafd6956c62ec8f0677befb26c7ff9087fdfd3e3a95901d28f54bef7871e2",
                                                    "47a0e3bd0c88b48476b876445131880ed73552003fd7933bea1b09f8f92074c0",
                                                    "1154060029f69dc9190eb0f3837bb7a1162b69a63daddfc939a6758503ff2156",
                                                    "d2afd7c76efcb5fcec11410ea3741a0a14d28ba05b6394da899a0a4dcece33f2",
                                                    "60e2a126c7d89941d63dec328b4d2890866ab7a0954f914f70842d3db88172bd",
                                                    "dd5657209915394731c62efc87310c5835bab4f79764170e49285a5f99a8fe75",
                                                    "97c9530a7b988b41fbca0491a95e8754e5b10d8cdd3f585664682d7d27536b0b",
                                                    "43751c4174e1249134a6b252d2e12fd85556d078127f103091c66cb04f9cfbe1",
                                                    "6747ef0fc3fef7a44067aa107f3cb5607ef7951c0059b3759cdb620f33241f32",
                                                    "f1ccce88f7666e357bacd1d123c92aca5825207453bbb4d8f77b1b0a37979f43",
                                                    "c81956294bc1ca6f66855e24ab72e05c186cdefad6e451aacb4f8cf71db016e0",
                                                    "ff9c992364b6ad5dedd1bebd929817181510f63c44a78b49d9e2a85b49474b95",
                                                    "5078478743c1b0552a056c726a81a04555d89cedec471c3399938ad845d3b43e",
                                                    "c358fd147ec71be5f3bf14626329fc2dd8d8eb3855150dc6a8a4e9006d5b02a6",
                                                    "df52e60c257ea07916f7c4bb4a2afad0cd7995e473bd87f27dd8d9e8866f0ed5",
                                                    "1758888cf211da9a794b126b04684a177b9ba8f8010bf877ce182e15cd31156f",
                                                    "37b605721fb6776c64c9d2c6d764ca9b832ed23e6888ac23fd2edae447cbc916",
                                                    "6ba551a5639d6f6807edeed6ca38f65d8fc385e6a443a69944e38b13308b49c4",
                                                    "c9d2ba76885812f17af92c5a86a1932587338519e96711c7d2226875a4eca3c4",
                                                    "2447b0925d83864ce9e340f22b8fd8ffe6aca0993525e3dcd9edfa5ac7538941",
                                                    "0589ac23c7d45783535bddd7e0af57fae73ff292a34c3e899f743feaf96009e5",
                                                    "d8df50afb952cf44ffa9544f5c62081a5739df1b5e089a8d5c8aed53e3b46102",
                                                    "7fe55b2a35ffb0693cc6cea91dbf92303e2b6f1d4d11bab04209eb3481b204f6",
                                                    "d4f7d0dea4c8ad05a9f18651a7b1619b8203068bca68e7faf438d4feffc1ee34",
                                                    "3780b88cf3336d46603478809550289e42b11c77085b436e0cd90c4bf2fbc3d8",
                                                    "f34643fb8498e07e0ea58183866622c8801acfe54da817722846ba9cc7225a4f",
                                                    "a5f40c19907d6df643b9511d3c32148ac9ab124da7521c67cf464a83f439f3a1"
        };
        public DownloadTest(GalleriesFixture galleries)
        {
            System.Diagnostics.Debug.WriteLine($"Temp dir : {tempDir}");
            gallery = galleries.galleries.Find(x => x.Id == 1099244);
            anime = galleries.galleries.Find(x => x.Id == 1352);
            downloader = new LibHitomi.Downloader.Downloader(g => Path.Combine(tempDir, g.Id.ToString()), (g, of, i) => g.Type == "anime" ? of.ToLower() : $"{i}.{of.Split('.').Last().ToLower()}");
        }
        [Fact]
        public async void DownloadGallery()
        {
            await downloader.DownloadAsync(gallery);
            string baseDir = Path.Combine(tempDir, "1099244");
            SHA256 sha256 = SHA256.Create();
            for (int i = 0; i < 30; i++)
            {
                string filepath = Path.Combine(baseDir, i + ".jpg");
                using (FileStream fstr = new FileStream(filepath, FileMode.Open,FileAccess.Read, FileShare.Read))
                {
                    Assert.Equal(BitConverter.ToString(sha256.ComputeHash(fstr)).Replace("-", "").ToLower(), sha256sums[i]);
                }
            }
        }
        // 184MB라 시간 오래걸려서 테스트 작성 안 함.
        // 
        //[Fact]
        //public async void DownloadAnime()
        //{
        //}
    }
}
