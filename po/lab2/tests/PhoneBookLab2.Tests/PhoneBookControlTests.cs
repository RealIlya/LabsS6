using PhoneBookLab2.Core;

namespace PhoneBookLab2.Tests;

public class PhoneBookControlTests
{
    [Fact]
    public void AddFindDeleteRecord_WorksAsExpected()
    {
        using var control = new PhoneBookControl();

        control.AddRecord("Ivanov", "100");
        control.AddRecord("Petrov", "200");

        Assert.Equal(2, control.RecordsInBook());
        Assert.Equal(0, control.FindRecord("Ivanov", "100"));
        Assert.True(control.DeleteRecord("Ivanov", "100"));
        Assert.Equal(1, control.RecordsInBook());
    }

    [Fact]
    public void DeleteSelectedRecord_RemovesByIndex()
    {
        using var control = new PhoneBookControl();
        control.AddRecord("Ivanov", "100");
        control.AddRecord("Petrov", "200");

        control.DeleteSelectedRecord(0);
        Assert.Equal(1, control.RecordsInBook());
        Assert.Equal("Petrov", control.ReadName(0));
    }

    [Fact]
    public void UpdateSelectedRecord_ChangesDataAndKeepsSorting()
    {
        using var control = new PhoneBookControl();
        control.AddRecord("Ivanov", "100");
        control.AddRecord("Petrov", "200");

        control.UpdateSelectedRecord(1, "Alekseev", "900");

        Assert.Equal(2, control.RecordsInBook());
        Assert.Equal("Alekseev", control.ReadName(0));
        Assert.Equal("900", control.ReadNumber(0));
    }

    [Fact]
    public void SaveLoad_RestoresRecordsFromFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"phonebook_{Guid.NewGuid():N}.pbk");
        try
        {
            using (var writerControl = new PhoneBookControl())
            {
                writerControl.CreateFile(filePath);
                writerControl.AddRecord("Ivanov", "100");
                writerControl.AddRecord("Petrov", "200");
                writerControl.SaveBookToFile();
            }

            using (var readerControl = new PhoneBookControl())
            {
                readerControl.CreateFile(filePath);
                readerControl.LoadBookFromFile();

                Assert.Equal(2, readerControl.RecordsInBook());
                Assert.Equal("Ivanov", readerControl.ReadName(0));
                Assert.Equal("200", readerControl.ReadNumber(1));
            }
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void ClearBook_RemovesAllRecords()
    {
        using var control = new PhoneBookControl();
        control.AddRecord("Ivanov", "100");
        control.ClearBook();
        Assert.Equal(0, control.RecordsInBook());
    }
}
