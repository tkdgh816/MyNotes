using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace MyNotes.Core.Service;

internal class SearchService : IDisposable
{
  public readonly LuceneVersion LuceneVersion = LuceneVersion.LUCENE_48;
  private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private readonly FSDirectory _indexDir;
  public Analyzer Analyzer { get; }
  public IndexWriter Writer { get; }

  public SearchService()
  {
    var searchFolder = _localFolder.CreateFolderAsync("data", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
    var indexPath = Path.Combine(searchFolder.Path, "search");
    _indexDir = FSDirectory.Open(indexPath);
    Analyzer = new StandardAnalyzer(LuceneVersion);
    var indexConfig = new IndexWriterConfig(LuceneVersion, Analyzer) { OpenMode = OpenMode.CREATE_OR_APPEND };
    Writer = new IndexWriter(_indexDir, indexConfig);

//    var perFieldAnalyzer = new PerFieldAnalyzerWrapper(Analyzer, new Dictionary<string, Analyzer>
//{
//    { "title", new EdgeNGramTokenizer(LuceneVersion.LUCENE_48, 1, 10) },    // 제목: 접두사 검색
//    { "content_ngram", new NGramTokenizer(LuceneVersion.LUCENE_48, 3, 10) } // 본문 부분검색
//});
  }

  public void Dispose()
  {
    Writer.Dispose();
    _indexDir.Dispose();
  }
}
