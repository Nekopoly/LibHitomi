using System;
using System.Collections.Generic;
using LibHitomi;
using LibHitomi.GalleryList;
using LibHitomi.Search;
using Xunit;
using Xunit.Abstractions;

namespace LibHitomi.Test
{
    public class SearchTest : IClassFixture<GalleriesFixture>
    {
        private SimpleSearcher searcher;
        private List<Gallery> galleries;
        public SearchTest(GalleriesFixture galleriesFixture)
        {
            galleries = galleriesFixture.galleries;
            searcher = new SimpleSearcher();
        }
        [Fact]
        public void SearchWithLanguageQuery()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "language:korean"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => x.Language == "korean"));
        }
        [Fact]
        public void SearchWithTagQuery()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "tag:full_color"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => Array.IndexOf(x.Tags, "full color") >= 0));
        }
        [Fact]
        public void SearchWithTitleQuery()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "title:boku"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => x.Name.ToLower().Contains("boku")));
        }
        [Fact]
        public void SearchWithNATagQuery()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "tag:"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => x.Tags.Length == 0));
        }
        [Fact]
        public void SearchWithNALanguageQuery()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "language:"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => x.Language.Length == 0));
        }
        [Fact]
        public void SearchWithExclusiveQuery()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "-tag:full_color"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => Array.IndexOf(x.Tags, "full color") < 0));
        }
        [Fact]
        public void SearchWithMultipleQueries()
        {
            var Result = new List<Gallery>(searcher.Search(galleries, "language:korean tag:full_color tag:uncensored"));

            Assert.True(Result.Count > 0);
            Assert.True(Result.TrueForAll(x => Array.IndexOf(x.Tags, "full color") >= 0));
            Assert.True(Result.TrueForAll(x => Array.IndexOf(x.Tags, "uncensored") >= 0));
            Assert.True(Result.TrueForAll(x => x.Language == "korean"));
        }
    }
}
