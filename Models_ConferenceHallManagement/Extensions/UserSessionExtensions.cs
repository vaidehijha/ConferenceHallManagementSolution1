using Models_ConferenceHallManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models_ConferenceHallManagement.Extensions
{
    /// <summary>
    /// Extension methods for UserSessionDetails providing role-based access logic
    /// Single source of truth: List<UserRoleInfo> Roles
    /// </summary>
    public static class UserSessionExtensions
    {
        /// <summary>
        /// Role hierarchy for priority determination
        /// SuperAdmin(1) > CentralAdmin(2) > RegionalAdmin(3) > StationAdmin(4) > Employee(0)
        /// </summary>
        private static readonly Dictionary<string, int> RoleHierarchy = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Super Admin", 5 },
            { "Central Admin", 4 },
            { "Regional Admin", 3 },
            { "Station Admin", 2 },
            { "Employee", 1 }
        };

        /// <summary>
        /// Gets the primary (highest priority) role based on hierarchy
        /// Returns the role with the highest priority level
        /// </summary>
        public static UserRoleInfo? GetPrimaryRole(this UserSessionDetails session)
        {
            if (session?.Roles == null || !session.Roles.Any())
                return null;

            return session.Roles
                .OrderByDescending(r => GetRolePriority(r.RoleName))
                .ThenBy(r => r.RoleId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the primary role ID (0 if no roles)
        /// </summary>
        public static int GetPrimaryRoleId(this UserSessionDetails session)
        {
            return session?.GetPrimaryRole()?.RoleId ?? 0;
        }

        /// <summary>
        /// Gets the primary role name ("Employee" if no roles)
        /// </summary>
        public static string GetPrimaryRoleName(this UserSessionDetails session)
        {
            return session?.GetPrimaryRole()?.RoleName ?? "Employee";
        }

        /// <summary>
        /// Checks if user has a specific role by role ID
        /// </summary>
        public static bool HasRole(this UserSessionDetails session, int roleId)
        {
            return session?.Roles?.Any(r => r.RoleId == roleId) ?? false;
        }

        /// <summary>
        /// Checks if user has a specific role by role name (case-insensitive)
        /// </summary>
        public static bool HasRole(this UserSessionDetails session, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return false;

            return session?.Roles?.Any(r => 
                r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Checks if user has any admin role
        /// </summary>
        public static bool IsAdmin(this UserSessionDetails session)
        {
            return session?.Roles?.Any(r => 
                r.RoleName.Contains("Admin", StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Gets all role names the user has
        /// </summary>
        public static List<string> GetRoleNames(this UserSessionDetails session)
        {
            return session?.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets the access level based on primary role
        /// </summary>
        public static AccessLevel GetAccessLevel(this UserSessionDetails session)
        {
            var primaryRole = session?.GetPrimaryRole();
            if (primaryRole == null)
                return AccessLevel.None;

            return primaryRole.RoleId switch
            {
                1 => AccessLevel.SuperAdmin,
                2 => AccessLevel.CentralAdmin,
                3 => AccessLevel.RegionalAdmin,
                4 => AccessLevel.StationAdmin,
                0 => AccessLevel.User,
                _ => AccessLevel.User
            };
        }

        /// <summary>
        /// Gets the role priority for hierarchy comparison
        /// Higher number = higher priority
        /// </summary>
        private static int GetRolePriority(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return 0;

            return RoleHierarchy.TryGetValue(roleName, out int priority) ? priority : 0;
        }

        /// <summary>
        /// Checks if user can access a specific record based on role hierarchy
        /// </summary>
        public static bool CanAccessRecord(this UserSessionDetails session, string recordEmpNo, 
            int? recordRegionId = null, int? recordLocationId = null)
        {
            if (session == null) return false;

            var primaryRole = session.GetPrimaryRole();
            if (primaryRole == null) return false;

            // Super Admin and Central Admin can access everything
            if (primaryRole.RoleId == 1 || primaryRole.RoleId == 2)
                return true;

            // Regional Admin - check region
            if (primaryRole.RoleId == 3)
                return primaryRole.RegionId.HasValue && recordRegionId.HasValue && 
                       primaryRole.RegionId.Value == recordRegionId.Value;

            // Station Admin - check location
            if (primaryRole.RoleId == 4)
                return primaryRole.LocationId.HasValue && recordLocationId.HasValue && 
                       primaryRole.LocationId.Value == recordLocationId.Value;

            // Regular User - check if own record
            return session.EmpNo.Equals(recordEmpNo, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Access level enumeration for role-based authorization
    /// </summary>
    public enum AccessLevel
    {
        None = 0,
        User = 1,
        StationAdmin = 2,
        RegionalAdmin = 3,
        CentralAdmin = 4,
        SuperAdmin = 5
    }
}
