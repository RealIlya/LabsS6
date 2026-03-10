using System.Text.Json;

namespace PhoneBookLab2.Core;

public sealed class PhoneBookControl : IDisposable
{
    private readonly AbonentList list = new();
    private FileStream? fileStream;
    private string? filePath;

    public int RecordsInBook()
    {
        return list.RecordsCount;
    }

    public string ReadRecord(int index)
    {
        return list.ReadRecord(index).ToString();
    }

    public string ReadName(int index)
    {
        return list.ReadRecord(index).Name;
    }

    public string ReadNumber(int index)
    {
        return list.ReadRecord(index).Number;
    }

    public void AddRecord(string name, string number)
    {
        list.AddRecord(new Abonent(new AbonentRecord(name, number)));
    }

    public bool DeleteRecord(string name, string number)
    {
        return list.RemoveRecord(new Abonent(new AbonentRecord(name, number)));
    }

    public void DeleteSelectedRecord(int index)
    {
        list.RemoveSelectedRecord(index);
    }

    public void UpdateSelectedRecord(int index, string name, string number)
    {
        var updated = new Abonent(new AbonentRecord(name, number));
        list.RemoveSelectedRecord(index);
        list.AddRecord(updated);
    }

    public void ClearBook()
    {
        list.ClearList();
    }

    public int FindRecord(string name, string number)
    {
        return list.FindRecord(new Abonent(new AbonentRecord(name, number)));
    }

    public void CreateFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Путь к файлу не должен быть пустым.", nameof(path));
        }

        DisposeStream();

        filePath = path;
        fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
    }

    public void SaveBookToFile()
    {
        EnsureFileCreated();

        fileStream!.SetLength(0);
        fileStream.Position = 0;

        var records = list.Snapshot();
        JsonSerializer.Serialize(fileStream, records);
        fileStream.Flush();
        fileStream.Position = 0;
    }

    public void LoadBookFromFile()
    {
        EnsureFileCreated();

        fileStream!.Position = 0;
        if (fileStream.Length == 0)
        {
            list.ClearList();
            return;
        }

        var records = JsonSerializer.Deserialize<List<AbonentRecord>>(fileStream);
        list.ReplaceAll(records ?? []);
        fileStream.Position = 0;
    }

    public string? CurrentFilePath()
    {
        return filePath;
    }

    public void Dispose()
    {
        DisposeStream();
        GC.SuppressFinalize(this);
    }

    private void DisposeStream()
    {
        fileStream?.Dispose();
        fileStream = null;
    }

    private void EnsureFileCreated()
    {
        if (fileStream is null)
        {
            throw new InvalidOperationException("Файл не создан и не открыт.");
        }
    }
}

