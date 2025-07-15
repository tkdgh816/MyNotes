using MyNotes.Core.Dto;

namespace MyNotes.Core.Dao;
internal class NoteFileDao
{
  private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private readonly StorageFolder _noteFolder;

  public NoteFileDao()
  {
    _noteFolder = _localFolder.CreateFolderAsync("notes", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
  }

  public Task<StorageFile> GetFile(NoteFileDto dto)
  => _noteFolder.GetFileAsync(dto.FileName).AsTask();

  public Task<StorageFile> CreateFile(NoteFileDto dto)
    => _noteFolder.CreateFileAsync(dto.FileName, CreationCollisionOption.OpenIfExists).AsTask();

  public async Task DeleteFile(NoteFileDto dto)
  {
    try
    {
      await (await CreateFile(dto)).DeleteAsync(StorageDeleteOption.PermanentDelete);
    }
    catch (Exception)
    { }
  }
}
