using System.Text.Json;
using DotNetCleanTemplate.WebClient.Services;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Services;

public class LocalStorageServiceTests
{
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly LocalStorageService _localStorageService;

    public LocalStorageServiceTests()
    {
        _mockJSRuntime = new Mock<IJSRuntime>();
        _localStorageService = new LocalStorageService(_mockJSRuntime.Object);
    }

    [Fact]
    public async Task GetItemAsync_WhenItemExists_ReturnsDeserializedValue()
    {
        // Arrange
        var testObject = new TestObject { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(testObject);

        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        var result = await _localStorageService.GetItemAsync<TestObject>("testKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testObject.Id, result.Id);
        Assert.Equal(testObject.Name, result.Name);
    }

    [Fact]
    public async Task GetItemAsync_WhenItemDoesNotExist_ReturnsDefault()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _localStorageService.GetItemAsync<TestObject>("testKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemAsync_WhenEmptyString_ReturnsDefault()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _localStorageService.GetItemAsync<TestObject>("testKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemAsync_WhenJSRuntimeThrowsException_ReturnsDefault()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ThrowsAsync(new Exception("JS Error"));

        // Act
        var result = await _localStorageService.GetItemAsync<TestObject>("testKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetItemAsync_WhenValidObject_SetsItemSuccessfully()
    {
        // Arrange
        var testObject = new TestObject { Id = 1, Name = "Test" };
        var expectedJson = JsonSerializer.Serialize(testObject);

        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _localStorageService.SetItemAsync("testKey", testObject);

        // Assert
        _mockJSRuntime.Verify(
            x =>
                x.InvokeVoidAsync(
                    "localStorage.setItem",
                    It.Is<object[]>(args =>
                        args[0].ToString() == "testKey" && args[1].ToString() == expectedJson
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SetItemAsync_WhenJSRuntimeThrowsException_DoesNotThrow()
    {
        // Arrange
        var testObject = new TestObject { Id = 1, Name = "Test" };

        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
            .ThrowsAsync(new Exception("JS Error"));

        // Act & Assert
        await _localStorageService.SetItemAsync("testKey", testObject);
        // Не должно выбрасывать исключение
    }

    [Fact]
    public async Task RemoveItemAsync_WhenValidKey_RemovesItemSuccessfully()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.removeItem", It.IsAny<object[]>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _localStorageService.RemoveItemAsync("testKey");

        // Assert
        _mockJSRuntime.Verify(
            x =>
                x.InvokeVoidAsync(
                    "localStorage.removeItem",
                    It.Is<object[]>(args => args[0].ToString() == "testKey")
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveItemAsync_WhenJSRuntimeThrowsException_DoesNotThrow()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.removeItem", It.IsAny<object[]>()))
            .ThrowsAsync(new Exception("JS Error"));

        // Act & Assert
        await _localStorageService.RemoveItemAsync("testKey");
        // Не должно выбрасывать исключение
    }

    [Fact]
    public void GetItem_WhenItemExists_ReturnsDeserializedValue()
    {
        // Arrange
        var testObject = new TestObject { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(testObject);

        var mockTask = new Mock<IJSObjectReference>();
        var valueTask = ValueTask.FromResult(json);

        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .Returns(valueTask);

        // Act
        var result = _localStorageService.GetItem<TestObject>("testKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testObject.Id, result.Id);
        Assert.Equal(testObject.Name, result.Name);
    }

    [Fact]
    public void GetItem_WhenItemDoesNotExist_ReturnsDefault()
    {
        // Arrange
        var valueTask = ValueTask.FromResult<string?>(null);

        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .Returns(valueTask);

        // Act
        var result = _localStorageService.GetItem<TestObject>("testKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetItem_WhenEmptyString_ReturnsDefault()
    {
        // Arrange
        var valueTask = ValueTask.FromResult(string.Empty);

        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .Returns(valueTask);

        // Act
        var result = _localStorageService.GetItem<TestObject>("testKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetItem_WhenJSRuntimeThrowsException_ReturnsDefault()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .Throws(new Exception("JS Error"));

        // Act
        var result = _localStorageService.GetItem<TestObject>("testKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetItem_WhenValidObject_SetsItemSuccessfully()
    {
        // Arrange
        var testObject = new TestObject { Id = 1, Name = "Test" };
        var expectedJson = JsonSerializer.Serialize(testObject);

        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        _localStorageService.SetItem("testKey", testObject);

        // Assert
        _mockJSRuntime.Verify(
            x =>
                x.InvokeVoidAsync(
                    "localStorage.setItem",
                    It.Is<object[]>(args =>
                        args[0].ToString() == "testKey" && args[1].ToString() == expectedJson
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public void SetItem_WhenJSRuntimeThrowsException_DoesNotThrow()
    {
        // Arrange
        var testObject = new TestObject { Id = 1, Name = "Test" };

        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.setItem", It.IsAny<object[]>()))
            .Throws(new Exception("JS Error"));

        // Act & Assert
        _localStorageService.SetItem("testKey", testObject);
        // Не должно выбрасывать исключение
    }

    [Fact]
    public void RemoveItem_WhenValidKey_RemovesItemSuccessfully()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.removeItem", It.IsAny<object[]>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        _localStorageService.RemoveItem("testKey");

        // Assert
        _mockJSRuntime.Verify(
            x =>
                x.InvokeVoidAsync(
                    "localStorage.removeItem",
                    It.Is<object[]>(args => args[0].ToString() == "testKey")
                ),
            Times.Once
        );
    }

    [Fact]
    public void RemoveItem_WhenJSRuntimeThrowsException_DoesNotThrow()
    {
        // Arrange
        _mockJSRuntime
            .Setup(x => x.InvokeVoidAsync("localStorage.removeItem", It.IsAny<object[]>()))
            .Throws(new Exception("JS Error"));

        // Act & Assert
        _localStorageService.RemoveItem("testKey");
        // Не должно выбрасывать исключение
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
