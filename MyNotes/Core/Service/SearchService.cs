using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

using MyNotes.Core.Shared;

namespace MyNotes.Core.Service;

internal class SearchService : IDisposable
{
  private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private readonly FSDirectory _indexDir;
  public Analyzer WriterAnalyzer { get; }
  public IndexWriter Writer { get; }

  public SearchService()
  {
    var searchFolder = _localFolder.CreateFolderAsync("data", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
    var indexPath = Path.Combine(searchFolder.Path, "search");
    _indexDir = FSDirectory.Open(indexPath);
    WriterAnalyzer = new NGramAnalyzer(SearchSettings.MinGram, SearchSettings.MaxGram);
    var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, WriterAnalyzer) { OpenMode = OpenMode.CREATE_OR_APPEND };
    Writer = new IndexWriter(_indexDir, indexConfig);
  }

  public void Dispose()
  {
    Writer.Dispose();
    _indexDir.Dispose();
  }
}

public class NGramAnalyzer : Analyzer
{
  private readonly int _minGram;
  private readonly int _maxGram;
  private readonly CharArraySet _stopWords = new(LuceneVersion.LUCENE_48,
    [
      "a"," an"," and"," are"," as"," at"," be"," but"," by"," for"," if"," in"," into"," is"," it"," no"," not",
      "of"," on"," or"," such"," that"," the"," their"," then"," there"," these"," they"," this"," to",
      "was"," will"," with"," can"," do"," does"," did"," from"," has"," have"," had"," he"," she"," him",
      "her"," his"," we"," you"," your"," I"," me"," my"," ours"," ours"," them"," us"," what"," which"," who",
      "whom"," whose"," where"," when"," why"," how"," all"," any"," both"," each"," few"," more"," most",
      "other"," some"," such"," only"," own"," same"," so"," than"," too", "very"
    ],
    true);

  public NGramAnalyzer(int minGram, int maxGram)
  {
    _minGram = minGram;
    _maxGram = maxGram;
  }

  protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
  {
    StandardTokenizer tokenizer = new(LuceneVersion.LUCENE_48, reader);
    TokenStream tokenStream = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);
    tokenStream = new NGramTokenFilter(LuceneVersion.LUCENE_48, tokenStream, _minGram, _maxGram);
    //tokenStream = new StopFilter(LuceneVersion.LUCENE_48, tokenStream, _stopWords);

    return new TokenStreamComponents(tokenizer, tokenStream);
  }
}

internal class MaxGramAnalyzer : Analyzer
{
  private readonly int _maxGram;

  public MaxGramAnalyzer(int maxGram)
  {
    _maxGram = maxGram;
  }

  protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
  {
    MaxGramTokenizer tokenizer = new(reader, _maxGram);
    TokenStream tokenStream = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer);

    return new TokenStreamComponents(tokenizer, tokenStream);
  }
}

internal sealed class MaxGramTokenizer : Tokenizer
{
  private readonly int _maxGram;
  private readonly ICharTermAttribute _termAttr;
  private readonly IOffsetAttribute _offsetAttr;

  private string _text = "";
  private int _index = 0;

  public MaxGramTokenizer(TextReader input, int maxGram) : base(input)
  {
    _maxGram = maxGram;
    _termAttr = AddAttribute<ICharTermAttribute>();
    _offsetAttr = AddAttribute<IOffsetAttribute>();
  }

  public override bool IncrementToken()
  {
    if (_index == 0 && string.IsNullOrEmpty(_text))
      _text = m_input.ReadToEnd();

    if (_text.Length < _maxGram)
    {
      if (_index > 0)
        return false;

      ClearAttributes();
      _termAttr.SetEmpty().Append(_text);
      _offsetAttr.SetOffset(CorrectOffset(0), CorrectOffset(_text.Length));
      _index++;
      return true;
    }

    if (_index > _text.Length - _maxGram)
      return false;

    ClearAttributes();

    string token = _text.Substring(_index, _maxGram);
    _termAttr.SetEmpty().Append(token);
    _offsetAttr.SetOffset(CorrectOffset(_index), CorrectOffset(_index + _maxGram));

    _index++;
    return true;
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
  }
}