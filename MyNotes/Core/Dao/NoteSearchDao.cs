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

  private ScoreDoc? currentDoc;
  public IEnumerable<GetNoteDto> GetNoteSearchIds(string searchText, int limit, int offset)
  {
    if (offset == 0)
      currentDoc = null;

    using DirectoryReader indexReader = _searchService.Writer.GetReader(true);
    var indexSearcher = new IndexSearcher(indexReader);
    List<GetNoteDto> dtos = new();

    var parser = new QueryParser(_searchService.LuceneVersion, "body", _searchService.Analyzer);
    try
    {
      var searchQuery = parser.Parse(searchText);


      var topDocs = indexSearcher.SearchAfter(currentDoc, searchQuery, limit);


      foreach (var scoreDoc in topDocs.ScoreDocs)
      {
        var doc = indexSearcher.Doc(scoreDoc.Doc);
        dtos.Add(new GetNoteDto() { Id = new Guid(doc.Get("id")) });
      }

      if (topDocs.ScoreDocs.Length > 0)
        currentDoc = topDocs.ScoreDocs[^1];
    }
    catch (ParseException)
    {

    }

    return dtos;
  }
}
