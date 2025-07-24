using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace DotNetCleanTemplate.WebClient.State;

public class AuthenticationState : INotifyPropertyChanged
{
    private readonly ILogger<AuthenticationState>? _logger;
    private bool _isAuthenticated;
    private string? _userEmail;
    private string? _userName;
    private bool _isLoading;

    public AuthenticationState(ILogger<AuthenticationState>? logger = null)
    {
        _logger = logger;
    }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set
        {
            if (_isAuthenticated != value)
            {
                _isAuthenticated = value;
                OnPropertyChanged();
            }
        }
    }

    public string? UserEmail
    {
        get => _userEmail;
        set
        {
            if (_userEmail != value)
            {
                _userEmail = value;
                OnPropertyChanged();
            }
        }
    }
    public string? UserName
    {
        get => _userName;
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetAuthenticated(string email, string name)
    {
        _logger?.LogDebug("Устанавливаем аутентификацию: Email={Email}, Name={Name}", email, name);
        IsAuthenticated = true;
        UserEmail = email;
        UserName = name;
    }

    public void SetUnauthenticated()
    {
        _logger?.LogDebug("Сбрасываем аутентификацию");
        IsAuthenticated = false;
        UserEmail = null;
        UserName = null;
    }
}
