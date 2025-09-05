using System.Threading;
using System.Threading.Channels;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;

namespace MyNotes.Core.Dao;
internal class NoteSearchDao(SearchService searchService)
{
  private readonly SearchService _searchService = searchService;

  public void AddSearchDocument(NoteSearchDto dto)
  {
    var doc = new Document()
      {
        new StringField("id", dto.Id.ToString(), Field.Store.YES),
        new TextField("title", dto.Title, Field.Store.YES),
        new TextField("body", dto.Body, Field.Store.YES)
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
            new TextField("body", dto.Body, Field.Store.YES)
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
        var indexSearcher = new IndexSearcher(indexReader);
        var parser = new QueryParser(_searchService.LuceneVersion, "body", _searchService.Analyzer);
        var searchQuery = parser.Parse(searchText);
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
            var doc = indexSearcher.Doc(scoreDoc.Doc);
            dtos.Add(new GetNoteSearchDto() { SearchText = searchText, Id = new Guid(doc.Get("id")), Body = doc.Get("body") });
          }

          currentDoc = scoreDocs.Last();
        }
      }
      catch (ParseException)
      {

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
        var indexSearcher = new IndexSearcher(indexReader);
        var parser = new QueryParser(_searchService.LuceneVersion, "body", _searchService.Analyzer);
        var searchQuery = parser.Parse(searchText);
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
            var doc = indexSearcher.Doc(scoreDoc.Doc);
            await channel.Writer.WriteAsync(new GetNoteSearchDto() { SearchText = searchText, Id = new Guid(doc.Get("id")), Body = doc.Get("body") });
          }

          currentDoc = scoreDocs.Last();
        }
      }
      catch (ParseException)
      {

      }
      finally
      {
        channel.Writer.Complete();
      }
    }, cancellationToken);

    return channel.Reader.ReadAllAsync(cancellationToken);
  }
}
