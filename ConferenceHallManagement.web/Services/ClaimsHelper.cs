using Models_ConferenceHallManagement.DTOs;
using Models_ConferenceHallManagement.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace ConferenceHallManagement.Web.Services
{
    /// <summary>
    /// Service to convert between UserSessionDetails and Claims
    /// Optimized for minimal cookie size and security
    /// 
    /// Cookie Strategy:
    /// - Individual claims for identity data (EmpNo, Name, Email, Mobile, EmpImageGuid)
    /// - Individual ClaimTypes.Role claims for ASP.NET [Authorize(Roles="")] support
    /// - Single "UserRoles" JSON claim containing List&lt;UserRoleInfo&gt; for role details
    /// - No duplicate data - ~60% smaller cookies
    /// </summary>
    public class ClaimsHelper
    {
        private const string USER_ROLES_CLAIM = "UserRoles";
        private const string EMP_IMAGE_GUID_CLAIM = "EmpImageGuid";

        /// <summary>
        /// Converts UserSessionDetails to minimal Claims list for cookie authentication
        /// 
        /// Cookie Structure:
        /// - NameIdentifier (EmpNo) - for user identification
        /// - Name (UserName) - for display
        /// - Email - for contact/notifications
        /// - MobilePhone - for contact
        /// - EmpImageGuid - for profile picture
        /// - Role claims - for [Authorize(Roles="")] support
        /// - UserRoles JSON - SINGLE source of truth for role details (RegionId, LocationId, etc.)
        /// </summary>
        public static List<Claim> BuildClaimsFromSession(UserSessionDetails session)
        {
            var claims = new List<Claim>
            {
                // Essential identity claims
                new Claim(ClaimTypes.NameIdentifier, session.EmpNo),
                new Claim(ClaimTypes.Name, session.UserName),
                new Claim(ClaimTypes.Email, session.Email ?? string.Empty),
                new Claim(ClaimTypes.MobilePhone, session.Mobile ?? string.Empty),
                new Claim(EMP_IMAGE_GUID_CLAIM, session.EmpImageGuid ?? string.Empty)
            };

            // Add individual role claims for ASP.NET [Authorize(Roles="")] support
            // This allows [Authorize(Roles="Super Admin,Central Admin")] to work
            foreach (var role in session.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }

            // Serialize ONLY the Roles list (not entire UserSessionDetails)
            // This preserves role details (RoleId, RegionId, LocationId, etc.) needed for filtering
            // while avoiding duplicate data
            var rolesJson = JsonSerializer.Serialize(session.Roles, new JsonSerializerOptions 
            { 
                WriteIndented = false // Compact JSON for smaller cookie
            });
            claims.Add(new Claim(USER_ROLES_CLAIM, rolesJson));

            return claims;
        }

        /// <summary>
        /// Reconstructs UserSessionDetails from Claims Principal
        /// Combines individual identity claims with deserialized role data
        /// Performance: Fast reconstruction from structured claims
        /// No unnecessary data - only what's needed
        /// </summary>
        public static UserSessionDetails? GetSessionFromClaims(ClaimsPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return null;

            try
            {
                // Reconstruct UserSessionDetails from individual claims
                var session = new UserSessionDetails
                {
                    EmpNo = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                    UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                    Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                    Mobile = user.FindFirst(ClaimTypes.MobilePhone)?.Value ?? string.Empty,
                    EmpImageGuid = user.FindFirst(EMP_IMAGE_GUID_CLAIM)?.Value ?? string.Empty,
                    Roles = GetUserRolesFromClaims(user)
                };

                return session;
            }
            catch
            {
                // If claims are corrupt or missing, return null
                // Login will be required
                return null;
            }
        }

        /// <summary>
        /// Extracts and deserializes the Roles list from UserRoles claim
        /// This is the SINGLE source of truth for role details
        /// </summary>
        private static List<UserRoleInfo> GetUserRolesFromClaims(ClaimsPrincipal user)
        {
            var rolesJson = user?.FindFirst(USER_ROLES_CLAIM)?.Value;
            
            if (string.IsNullOrEmpty(rolesJson))
                return new List<UserRoleInfo>();

            try
            {
                var roles = JsonSerializer.Deserialize<List<UserRoleInfo>>(rolesJson);
                return roles ?? new List<UserRoleInfo>();
            }
            catch
            {
                // If JSON is corrupt, return empty list
                // User will have no roles (least privilege)
                return new List<UserRoleInfo>();
            }
        }

        /// <summary>
        /// Gets specific claim value by type
        /// </summary>
        public static string GetClaimValue(ClaimsPrincipal user, string claimType)
        {
            return user?.FindFirst(claimType)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets all user roles with region/location details
        /// Returns the deserialized Roles list from UserRoles claim
        /// </summary>
        public static List<UserRoleInfo> GetUserRoles(ClaimsPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return new List<UserRoleInfo>();

            return GetUserRolesFromClaims(user);
        }

        /// <summary>
        /// Checks if user has a specific roleeeeeeee
        /// Uses ClaimTypes.Role claims for fast lookup (no JSON deserialization)
        /// </summary>
        public static bool HasRole(ClaimsPrincipal user, string roleName)
        {
            return user?.FindAll(ClaimTypes.Role)
                ?.Any(c => c.Value.Equals(roleName, System.StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Gets the primary (highest priority) role name
        /// Uses extension method on reconstructed session
        /// </summary>
        public static string GetPrimaryRole(ClaimsPrincipal user)
        {
            var session = GetSessionFromClaims(user);
            return session?.GetPrimaryRoleName() ?? "Employee";
        }

        /// <summary>
        /// Gets the primary role ID
        /// Uses extension method on reconstructed session
        /// </summary>
        public static int GetPrimaryRoleId(ClaimsPrincipal user)
        {
            var session = GetSessionFromClaims(user);
            return session?.GetPrimaryRoleId() ?? 0;
        }

        /// <summary>
        /// Gets the primary UserRoleInfo object with full details (RegionId, LocationId, etc.)
        /// Uses extension method on reconstructed session
        /// </summary>
        public static UserRoleInfo? GetPrimaryRoleInfo(ClaimsPrincipal user)
        {
            var session = GetSessionFromClaims(user);
            return session?.GetPrimaryRole();
        }

        /// <summary>
        /// Gets the employee number (EmpNo) from NameIdentifier claim
        /// Fast access without session reconstruction
        /// </summary>
        public static string GetEmployeeNumber(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the user's display name from Name claim
        /// Fast access without session reconstruction
        /// </summary>
        public static string GetUserName(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the user's email from Email claim
        /// Fast access without session reconstruction
        /// </summary>
        public static string GetEmail(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }
}
