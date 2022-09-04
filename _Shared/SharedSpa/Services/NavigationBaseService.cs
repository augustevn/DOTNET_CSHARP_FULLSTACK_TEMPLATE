using Microsoft.AspNetCore.Components;
using SharedSpa.Interfaces;

namespace SharedSpa.Services;

public class NavigationBaseService : ISpaNavigationBaseService
{
    private readonly NavigationManager _navigationManager;
    public NavigationBaseService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
    
    // TODO: add regions

    public void NavigateToIndex()
    {
        _navigationManager.NavigateTo("/");
    }
}