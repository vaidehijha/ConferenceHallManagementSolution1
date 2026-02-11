using Microsoft.AspNetCore.Components.Authorization;
using Models_ConferenceHallManagement.DTOs;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConferenceHallManagement.Web.Services
{
    /// <summary>
    /// Service to access user session data from Claims (no session storage)
    /// Optimized for Blazor Server components
    /// </summary>
    public class UserSessionService
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public UserSessionService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        /// <summary>
        /// Gets complete user session details from claims
        /// </summary>
        public async Task<UserSessionDetails?> GetUserSessionAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return ClaimsHelper.GetSessionFromClaims(authState.User);
        }

        /// <summary>
        /// Gets a specific claim value by key
        /// </summary>
        public async Task<string> GetClaimValue(string claimType)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return ClaimsHelper.GetClaimValue(authState.User, claimType);
        }

        /// <summary>
        /// Gets all user roles with region/location details
        /// </summary>
        public async Task<List<UserRoleInfo>> GetUserRolesAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return ClaimsHelper.GetUserRoles(authState.User);
        }

        /// <summary>
        /// Checks if user has a specific role
        /// </summary>
        public async Task<bool> HasRoleAsync(string roleName)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return ClaimsHelper.HasRole(authState.User, roleName);
        }

        /// <summary>
        /// Gets primary role of the user
        /// </summary>
        public async Task<string> GetPrimaryRoleAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return ClaimsHelper.GetPrimaryRole(authState.User);
        }

        // Convenience methods for common claims
        public async Task<string> GetEmpName() => await GetClaimValue(ClaimTypes.Name);
        public async Task<string> GetEmpId() => await GetClaimValue(ClaimTypes.NameIdentifier);
        public async Task<string> GetEmail() => await GetClaimValue(ClaimTypes.Email);
        public async Task<string> GetMobile() => await GetClaimValue(ClaimTypes.MobilePhone);
        public async Task<string> GetEmpImageGuid() => await GetClaimValue("EmpImage");
    }
}