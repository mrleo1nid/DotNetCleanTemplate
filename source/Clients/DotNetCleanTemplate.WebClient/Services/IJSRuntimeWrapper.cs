using Microsoft.JSInterop;

namespace DotNetCleanTemplate.WebClient.Services;

public interface IJSRuntimeWrapper
{
    ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args);
    ValueTask InvokeVoidAsync(string identifier, params object?[]? args);
}

public class JSRuntimeWrapper : IJSRuntimeWrapper
{
    private readonly IJSRuntime _jsRuntime;

    public JSRuntimeWrapper(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
    {
        return _jsRuntime.InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
    {
        return _jsRuntime.InvokeVoidAsync(identifier, args);
    }
}
