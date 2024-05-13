using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace LedOperationSample.Helplers;

public class FileHelper
{
    private readonly string _directoryPath;

    public FileHelper(string directoryName)
    {
        _directoryPath = Path.Combine(Directory.GetCurrentDirectory(), directoryName);

        if (!Directory.Exists(_directoryPath))
        {
            Directory.CreateDirectory(_directoryPath);
        }
    }

    public void Save<T>(T data, string fileName)
    {
        string jsonString = JsonSerializer.Serialize(data);
        string filePath = Path.Combine(_directoryPath, fileName);
        File.WriteAllText(filePath, jsonString);
    }

    public async Task SaveAsync<T>(T data, string fileName)
    {
        string jsonString = JsonSerializer.Serialize(data);
        string filePath = Path.Combine(_directoryPath, fileName);
        await File.WriteAllTextAsync(filePath, jsonString);
    }

    public List<T> ReadAll<T>()
    {
        List<T> dataList = [];

        string[] filePaths = Directory.GetFiles(_directoryPath);

        foreach (string filePath in filePaths)
        {
            string fileContent = File.ReadAllText(filePath);

            try
            {
                var jsonObject = JsonSerializer.Deserialize<T>(fileContent);
                dataList.Add(jsonObject);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing file: {filePath}");
                Debug.WriteLine(ex.Message);
            }
        }

        return dataList;
    }

    public async Task<List<T>> ReadAllAsync<T>()
    {
        List<T> dataList = [];

        string[] filePaths = Directory.GetFiles(_directoryPath);

        foreach (string filePath in filePaths)
        {
            string fileContent = await File.ReadAllTextAsync(filePath);

            try
            {
                var jsonObject = JsonSerializer.Deserialize<T>(fileContent);
                dataList.Add(jsonObject);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing file: {filePath}");
                Debug.WriteLine(ex.Message);
            }
        }

        return dataList;
    }

    public void DeleteAllFiles()
    {
        string[] filePaths = Directory.GetFiles(_directoryPath);

        foreach (string filePath in filePaths)
        {
            File.Delete(filePath);
        }
    }
}
