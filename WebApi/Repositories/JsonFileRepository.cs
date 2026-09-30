using System.Text.Json;

namespace WebApi.Repositories
{
    public class JsonFileRepository<T>
    {
        private readonly string _filePath;
        private readonly Func<T, int> _idSelector;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private List<T> _items = new();

        public JsonFileRepository(
            string filePath,
            Func<T, int> idSelector)
        {
            _filePath = filePath;
            _idSelector = idSelector;
        }

        public async Task InitializeAsync()
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await _semaphore.WaitAsync();

            try
            {
                if (!File.Exists(_filePath))
                {
                    _items = new List<T>();

                    await File.WriteAllTextAsync(
                        _filePath,
                        "[]");

                    return;
                }

                var json = await File.ReadAllTextAsync(_filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _items = new List<T>();
                    return;
                }

                try
                {
                    _items =
                        JsonSerializer.Deserialize<List<T>>(
                            json,
                            _jsonOptions)
                        ?? new List<T>();
                }
                catch (JsonException ex)
                {
                    throw new InvalidDataException(
                        $"Файл '{_filePath}' содержит некорректный JSON.",
                        ex);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                return _items.ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            await _semaphore.WaitAsync();

            try
            {
                return _items.FirstOrDefault(
                    item => _idSelector(item) == id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> AddAsync(Func<int, T> factory)
        {
            await _semaphore.WaitAsync();

            try
            {
                var nextId = _items.Count == 0
                    ? 1
                    : _items.Max(_idSelector) + 1;

                var item = factory(nextId);

                _items.Add(item);

                await SaveLockedAsync();

                return item;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> UpdateAsync(int id, T item)
        {
            await _semaphore.WaitAsync();

            try
            {
                var index = _items.FindIndex(
                    existing => _idSelector(existing) == id);

                if (index == -1)
                {
                    return false;
                }

                _items[index] = item;

                await SaveLockedAsync();

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _semaphore.WaitAsync();

            try
            {
                var item = _items.FirstOrDefault(
                    existing => _idSelector(existing) == id);

                if (item == null)
                {
                    return false;
                }

                _items.Remove(item);

                await SaveLockedAsync();

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SaveLockedAsync()
        {
            var json = JsonSerializer.Serialize(
                _items,
                _jsonOptions);

            await File.WriteAllTextAsync(
                _filePath,
                json);
        }
    }
}
