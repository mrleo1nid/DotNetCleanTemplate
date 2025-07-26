using System.ComponentModel;
using DotNetCleanTemplate.Client.State;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.State;

public class AuthenticationStateTests
{
    private const string TestEmail = "test@example.com";
    private readonly AuthenticationState _authState;

    public AuthenticationStateTests()
    {
        _authState = new AuthenticationState();
    }

    [Fact]
    public void IsAuthenticated_WhenSetToTrue_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.IsAuthenticated))
                propertyChangedRaised = true;
        };

        // Act
        _authState.IsAuthenticated = true;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.True(_authState.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_WhenSetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        _authState.IsAuthenticated = true;
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.IsAuthenticated))
                propertyChangedRaised = true;
        };

        // Act
        _authState.IsAuthenticated = true;

        // Assert
        Assert.False(propertyChangedRaised);
        Assert.True(_authState.IsAuthenticated);
    }

    [Fact]
    public void UserEmail_WhenSetToNewValue_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.UserEmail))
                propertyChangedRaised = true;
        };

        // Act
        _authState.UserEmail = TestEmail;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(TestEmail, _authState.UserEmail);
    }

    [Fact]
    public void UserEmail_WhenSetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        _authState.UserEmail = TestEmail;
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.UserEmail))
                propertyChangedRaised = true;
        };

        // Act
        _authState.UserEmail = TestEmail;

        // Assert
        Assert.False(propertyChangedRaised);
        Assert.Equal(TestEmail, _authState.UserEmail);
    }

    [Fact]
    public void UserEmail_WhenSetToNull_RaisesPropertyChanged()
    {
        // Arrange
        _authState.UserEmail = TestEmail;
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.UserEmail))
                propertyChangedRaised = true;
        };

        // Act
        _authState.UserEmail = null;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Null(_authState.UserEmail);
    }

    [Fact]
    public void IsLoading_WhenSetToTrue_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.IsLoading))
                propertyChangedRaised = true;
        };

        // Act
        _authState.IsLoading = true;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.True(_authState.IsLoading);
    }

    [Fact]
    public void IsLoading_WhenSetToSameValue_DoesNotRaisePropertyChanged()
    {
        // Arrange
        _authState.IsLoading = true;
        var propertyChangedRaised = false;
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AuthenticationState.IsLoading))
                propertyChangedRaised = true;
        };

        // Act
        _authState.IsLoading = true;

        // Assert
        Assert.False(propertyChangedRaised);
        Assert.True(_authState.IsLoading);
    }

    [Fact]
    public void SetAuthenticated_WhenCalled_SetsIsAuthenticatedToTrueAndUserEmail()
    {
        // Arrange
        var email = TestEmail;
        var propertyChangedEvents = new List<string>();
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        _authState.SetAuthenticated(email, "Test User");

        // Assert
        Assert.True(_authState.IsAuthenticated);
        Assert.Equal(email, _authState.UserEmail);
        Assert.Contains(nameof(AuthenticationState.IsAuthenticated), propertyChangedEvents);
        Assert.Contains(nameof(AuthenticationState.UserEmail), propertyChangedEvents);
    }

    [Fact]
    public void SetUnauthenticated_WhenCalled_SetsIsAuthenticatedToFalseAndUserEmailToNull()
    {
        // Arrange
        _authState.IsAuthenticated = true;
        _authState.UserEmail = TestEmail;
        var propertyChangedEvents = new List<string>();
        _authState.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        _authState.SetUnauthenticated();

        // Assert
        Assert.False(_authState.IsAuthenticated);
        Assert.Null(_authState.UserEmail);
        Assert.Contains(nameof(AuthenticationState.IsAuthenticated), propertyChangedEvents);
        Assert.Contains(nameof(AuthenticationState.UserEmail), propertyChangedEvents);
    }

    [Fact]
    public void AuthenticationState_ImplementsINotifyPropertyChanged()
    {
        // Assert
        Assert.IsType<INotifyPropertyChanged>(_authState, exactMatch: false);
    }

    [Fact]
    public void AuthenticationState_InitialState_IsCorrect()
    {
        // Assert
        Assert.False(_authState.IsAuthenticated);
        Assert.Null(_authState.UserEmail);
        Assert.False(_authState.IsLoading);
    }

    [Fact]
    public void OnPropertyChanged_WhenCalledWithNullPropertyName_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            _authState
                .GetType()
                .GetMethod(
                    "OnPropertyChanged",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                ?.Invoke(_authState, new object?[] { null })
        );

        Assert.Null(exception);
    }
}
