using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace ConferenceHallManagement.Web.Services
{
    public class CookieAuthenticationStateProvider : ServerAuthenticationStateProvider
    {
        public event Action? OnAuthStateChanged;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var authState = base.GetAuthenticationStateAsync();
            NotifyAuthChanged();
            return authState;
        }

        private void NotifyAuthChanged()
        {
            OnAuthStateChanged?.Invoke();
        }

        public void NotifyUserAuthentication()
        {
            NotifyAuthChanged();
        }

        public void NotifyUserLogout()
        {
            NotifyAuthChanged();
        }
    }
}
