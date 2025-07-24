using System.Text.Json;
using Microsoft.JSInterop;

namespace DotNetCleanTemplate.WebClient.Services;

public class LocalStorageService : ILocalStorageService
{
    private readonly JSRuntimeWrapper _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = new JSRuntimeWrapper(jsRuntime);
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // Игнорируем ошибки при работе с localStorage
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Игнорируем ошибки при работе с localStorage
        }
    }

    public T? GetItem<T>(string key)
    {
        try
        {
            var task = _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (!task.IsCompletedSuccessfully)
            {
                return default;
            }

            var json = task.Result;
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public void SetItem<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var task = _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            if (!task.IsCompleted)
            {
                return;
            }
        }
        catch
        {
            // Игнорируем ошибки при работе с localStorage
        }
    }

    public void RemoveItem(string key)
    {
        try
        {
            var task = _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            if (!task.IsCompleted)
            {
                return;
            }
        }
        catch
        {
            // Игнорируем ошибки при работе с localStorage
        }
    }
}
