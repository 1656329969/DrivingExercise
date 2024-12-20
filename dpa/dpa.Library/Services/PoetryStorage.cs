using System.Linq.Expressions;
using dpa.Library.Helpers;
using dpa.Library.Models;
using SQLite;

namespace dpa.Library.Services;

public class PoetryStorage : IPoetryStorage {
    public const int NumberPoetry = 30;

    public const string DbName = "poetrydb.sqlite3";

    public static readonly string PoetryDbPath =
        PathHelper.GetLocalFilePath(DbName);

    private SQLiteAsyncConnection _connection;

    private SQLiteAsyncConnection Connection =>
        _connection ??= new SQLiteAsyncConnection(PoetryDbPath);

    private readonly IPreferenceStorage _preferenceStorage;
    
    public PoetryStorage(IPreferenceStorage preferenceStorage) {
        _preferenceStorage = preferenceStorage;
    }

    public bool IsInitialized =>
        _preferenceStorage.Get(PoetryStorageConstant.VersionKey,
            default(int)) == PoetryStorageConstant.Version;

    public async Task InitializeAsync() {
        await using var dbFileStream =
            new FileStream(PoetryDbPath, FileMode.OpenOrCreate);
        await using var dbAssetStream =
            typeof(PoetryStorage).Assembly.GetManifestResourceStream(DbName);
        await dbAssetStream.CopyToAsync(dbFileStream);
        
        _preferenceStorage.Set(PoetryStorageConstant.VersionKey,
            PoetryStorageConstant.Version);

        await Connection.CloseAsync();
    }

    public async Task<Poetry> GetPoetryAsync(int id) =>
        await Connection.Table<Poetry>().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IList<Poetry>> GetPoetriesAsync(
        Expression<Func<Poetry, bool>> where, int skip, int take) =>
        await Connection.Table<Poetry>().Where(where).Skip(skip).Take(take)
            .ToListAsync();
    
    public async Task CloseAsync() => await Connection.CloseAsync();
}

public static class PoetryStorageConstant {
    public const int Version = 1;

    public const string VersionKey =
        nameof(PoetryStorageConstant) + "." + nameof(Version);
    // PoetryStorageConstant.Version
}