using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ConferenceHallManagement.Web.Services
{
    public class UserSessionService
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public UserSessionService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        // --COMMON FUNCTION TO GET VALUE BY KEY ---
        public async Task<string> GetValue(string key)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                // Claims se value nikalna (Key-Value Store)
                var claim = user.Claims.FirstOrDefault(c => c.Type == key);
                return claim?.Value ?? "";
            }

            return "";
        }

        // Shortcuts (Taaki baar baar key na likhni pade)
        public async Task<string> GetEmpName() => await GetValue(ClaimTypes.Name);
        public async Task<string> GetEmpId() => await GetValue(ClaimTypes.NameIdentifier); // EmpNo
        public async Task<string> GetRole() => await GetValue(ClaimTypes.Role);
        public async Task<string> GetMobile() => await GetValue(ClaimTypes.MobilePhone);
        public async Task<string> GetEmail() => await GetValue(ClaimTypes.Email);
    }
}