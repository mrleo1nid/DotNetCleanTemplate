using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotNetCleanTemplate.WebClient.State;

public class AuthenticationState : INotifyPropertyChanged
{
    private bool _isAuthenticated;
    private string? _userEmail;
    private bool _isLoading;

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

    public void SetAuthenticated(string email)
    {
        IsAuthenticated = true;
        UserEmail = email;
    }

    public void SetUnauthenticated()
    {
        IsAuthenticated = false;
        UserEmail = null;
    }
}
