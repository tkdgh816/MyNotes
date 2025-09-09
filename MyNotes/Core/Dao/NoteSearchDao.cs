using System.Threading;
using System.Threading.Channels;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.Dao;
internal class NoteSearchDao
{
  private readonly SearchService _searchService;
  private readonly FieldType _bodyFieldType = new()
  {
    IsIndexed = true,
    IsStored = false,
    IsTokenized = true,
    StoreTermVectors = true,
    StoreTermVectorPositions = true,
    StoreTermVectorOffsets = true,
  };

  public NoteSearchDao(SearchService searchService)
  {
    _searchService = searchService;
    _bodyFieldType.Freeze();
  }

  public void AddSearchDocument(NoteSearchDto dto)
  {
    var doc = new Document()
      {
        new StringField("id", dto.Id.ToString(), Field.Store.YES),
        new TextField("title", dto.Title, Field.Store.YES),
        new Field("body", dto.Body, _bodyFieldType)
      };

    _searchService.Writer.AddDocument(doc);
    _searchService.Writer.Commit();
  }

  public void DeleteSearchDocument(DeleteNoteDto dto)
  {
    var term = new Term("id", dto.Id.ToString());
    _searchService.Writer.DeleteDocuments(term);
    _searchService.Writer.Commit();
  }

  public void UpdateNoteSearchDocument(NoteSearchDto dto)
  {
    var indexReader = DirectoryReader.Open(_searchService.Writer, true);
    var indexSearcher = new IndexSearcher(indexReader);
    var hits = indexSearcher.Search(new TermQuery(new Term("id", dto.Id.ToString())), 1).ScoreDocs;

    if (hits.Length > 0)
    {
      var doc = indexSearcher.Doc(hits[0].Doc);

      var newDoc = new Document()
          {
            new StringField("id", dto.Id.ToString(), Field.Store.YES),
            new TextField("title", dto.Title, Field.Store.YES),
            new Field("body", dto.Body, _bodyFieldType)
          };

      _searchService.Writer.UpdateDocument(new Term("id", dto.Id.ToString()), newDoc);
      _searchService.Writer.Commit();
    }
  }

  private readonly int _pageSize = 100;

  public Task<IEnumerable<GetNoteSearchDto>> GetNoteSearchIdsBatch(string searchText, CancellationToken cancellationToken = default) =>
    Task.Run(() =>
    {
      List<GetNoteSearchDto> dtos = new();

      try
      {
        using DirectoryReader indexReader = _searchService.Writer.GetReader(true);
        IndexSearcher indexSearcher = new(indexReader);
        QueryParser parser = new(_searchService.LuceneVersion, "body", new MaxGramAnalyzer(SearchSettings.MaxGram));
        var searchQuery = parser.Parse(searchText);

        List<string> tokens = GetTokens(searchText, SearchSettings.MaxGram);

        ScoreDoc? currentDoc = null;

        while (true)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var topDocs = indexSearcher.SearchAfter(currentDoc, searchQuery, _pageSize);
          var scoreDocs = topDocs.ScoreDocs;

          if (scoreDocs.Length == 0)
            break;

          foreach (var scoreDoc in scoreDocs)
          {
            cancellationToken.ThrowIfCancellationRequested();
            var docId = scoreDoc.Doc;
            var doc = indexSearcher.Doc(docId);
            var matches = GetDocPositionAndOffsets(indexReader, docId, tokens);

            if (matches is not null && matches.Count > 0)
              dtos.Add(new GetNoteSearchDto() { SearchText = searchText, Id = new Guid(doc.Get("id")), Matches = matches });
          }
          currentDoc = scoreDocs.Last();
        }
      }
      catch (ParseException parseException)
      {
        Debug.WriteLine(parseException.Message);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }
      
      return (IEnumerable<GetNoteSearchDto>)dtos;
    }, cancellationToken);

  public IAsyncEnumerable<GetNoteSearchDto> GetNoteSearchIdsStream(string searchText, CancellationToken cancellationToken = default)
  {
    var channel = Channel.CreateUnbounded<GetNoteSearchDto>();
    Task.Run(async () =>
    {
      try
      {
        using DirectoryReader indexReader = _searchService.Writer.GetReader(true);
        IndexSearcher indexSearcher = new(indexReader);
        QueryParser parser = new(_searchService.LuceneVersion, "body", new MaxGramAnalyzer(SearchSettings.MaxGram));
        var searchQuery = parser.Parse(searchText);

        List<string> tokens = GetTokens(searchText, SearchSettings.MaxGram);

        ScoreDoc? currentDoc = null;

        while (true)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var topDocs = indexSearcher.SearchAfter(currentDoc, searchQuery, _pageSize);
          var scoreDocs = topDocs.ScoreDocs;

          if (scoreDocs.Length == 0)
            break;

          foreach (var scoreDoc in scoreDocs)
          {
            cancellationToken.ThrowIfCancellationRequested();
            var docId = scoreDoc.Doc;
            var doc = indexSearcher.Doc(docId);
            var matches = GetDocPositionAndOffsets(indexReader, docId, tokens);

            if (matches is not null && matches.Count > 0)
              await channel.Writer.WriteAsync(new GetNoteSearchDto() { SearchText = searchText, Id = new Guid(doc.Get("id")), Matches = matches });
          }
          currentDoc = scoreDocs.Last();
        }
      }
      catch (ParseException parseException)
      {
        Debug.WriteLine(parseException.Message);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }
      finally
      {
        channel.Writer.Complete();
      }
    }, cancellationToken);

    return channel.Reader.ReadAllAsync(cancellationToken);
  }

  private List<string> GetTokens(string word, int gramSize)
  {
    word = word.ToLowerInvariant();
    int length = word.Length;

    List<string> tokens = new();
    if (length <= gramSize)
      tokens.Add(word);
    else
      for (int index = 0; index <= length - gramSize; index++)
        tokens.Add(word[index..(index + gramSize)]);

    return tokens;
  }

  private Dictionary<int, Range>? GetDocPositionAndOffsets(IndexReader indexReader, int docId, List<string> tokens)
  {
    var termsEnum = indexReader.GetTermVector(docId, "body").GetEnumerator();
    // TermsEnum: ScoreDoc의 특정 필드에서 발생한 모든 Term 나열

    Dictionary<int, Range>? matches = null;
    foreach (string token in tokens)
    {
      if (termsEnum.SeekExact(new BytesRef(token)))
      {
        var docsEnum = termsEnum.DocsAndPositions(null, null);

        if (docsEnum is null)
        {
          matches = null;
          break;
        }

        Dictionary<int, Range> currentMatches = new();

        while (docsEnum.NextDoc() != DocIdSetIterator.NO_MORE_DOCS)
        {
          for (int i = 0; i < docsEnum.Freq; i++)
            currentMatches.Add(docsEnum.NextPosition(), new Range(docsEnum.StartOffset, docsEnum.EndOffset));
        }

        matches = matches is null
          ? currentMatches
          : matches.Where(match => currentMatches.ContainsKey(match.Key)).ToDictionary();
      }
      else
      {
        matches = null;
        break;
      }
    }

    return matches;
  }
}
