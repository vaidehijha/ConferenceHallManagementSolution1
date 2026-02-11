using Models_ConferenceHallManagement.DTOs;
using Models_ConferenceHallManagement.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Models_ConferenceHallManagement.Helpers
{
    /// <summary>
    /// Extension methods to apply role-based filtering on IQueryable
    /// </summary>
    public static class RoleBasedFilterExtensions
    {
        /// <summary>
        /// Filters query based on user role and region/location access
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">IQueryable to filter</param>
        /// <param name="session">User session details</param>
        /// <param name="empNoSelector">Expression to select EmpNo field</param>
        /// <param name="regionIdSelector">Expression to select RegionId field</param>
        /// <param name="locationIdSelector">Expression to select LocationId field</param>
        /// <param name="selectedRole">Specific role to use (if null, uses primary role)</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> ApplyRoleBasedFilter<T>(
            this IQueryable<T> query,
            UserSessionDetails session,
            Expression<Func<T, string>> empNoSelector,
            Expression<Func<T, int?>> regionIdSelector,
            Expression<Func<T, int?>> locationIdSelector,
            UserRoleInfo? selectedRole = null) where T : class
        {
            if (session == null) return query;

            // Use selected role or primary role
            var roleToUse = selectedRole ?? session.GetPrimaryRole();
            if (roleToUse == null) return query;

            // Super Admin - No filter
            if (roleToUse.RoleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            // Central Admin - No filter (can see all regions)
            if (roleToUse.RoleName.Equals("Central Admin", StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            // Regional Admin - Filter by RegionId
            if (roleToUse.RoleName.Equals("Regional Admin", StringComparison.OrdinalIgnoreCase) && roleToUse.RegionId.HasValue)
            {
                var regionId = roleToUse.RegionId.Value;
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = Expression.Invoke(regionIdSelector, parameter);
                var constant = Expression.Constant((int?)regionId, typeof(int?));
                var equals = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                
                return query.Where(lambda);
            }

            // Station Admin - Filter by LocationId
            if (roleToUse.RoleName.Equals("Station Admin", StringComparison.OrdinalIgnoreCase) && roleToUse.LocationId.HasValue)
            {
                var locationId = roleToUse.LocationId.Value;
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = Expression.Invoke(locationIdSelector, parameter);
                var constant = Expression.Constant((int?)locationId, typeof(int?));
                var equals = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                
                return query.Where(lambda);
            }

            // Regular User - Filter by EmpNo
            var empNo = session.EmpNo;
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = Expression.Invoke(empNoSelector, parameter);
                var constant = Expression.Constant(empNo);
                var equals = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                
                return query.Where(lambda);
            }
        }

        /// <summary>
        /// Simplified version for entities that have CreatedBy field
        /// </summary>
        public static IQueryable<T> ApplyRoleFilter<T>(
            this IQueryable<T> query,
            UserSessionDetails session,
            Expression<Func<T, string>> createdBySelector,
            Expression<Func<T, int?>> regionIdSelector = null,
            Expression<Func<T, int?>> locationIdSelector = null) where T : class
        {
            if (session == null) return query;

            var primaryRole = session.GetPrimaryRole();
            if (primaryRole == null) return query;

            // Super Admin or Central Admin - No filter
            if (primaryRole.RoleName.Equals("Super Admin", StringComparison.OrdinalIgnoreCase) ||
                primaryRole.RoleName.Equals("Central Admin", StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            // Regional Admin
            if (primaryRole.RoleName.Equals("Regional Admin", StringComparison.OrdinalIgnoreCase) && 
                primaryRole.RegionId.HasValue && 
                regionIdSelector != null)
            {
                var regionId = primaryRole.RegionId.Value;
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = Expression.Invoke(regionIdSelector, parameter);
                var constant = Expression.Constant((int?)regionId, typeof(int?));
                var equals = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                
                return query.Where(lambda);
            }

            // Station Admin
            if (primaryRole.RoleName.Equals("Station Admin", StringComparison.OrdinalIgnoreCase) && 
                primaryRole.LocationId.HasValue && 
                locationIdSelector != null)
            {
                var locationId = primaryRole.LocationId.Value;
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = Expression.Invoke(locationIdSelector, parameter);
                var constant = Expression.Constant((int?)locationId, typeof(int?));
                var equals = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                
                return query.Where(lambda);
            }

            // Regular User - own records only
            var empNo = session.EmpNo;
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = Expression.Invoke(createdBySelector, parameter);
                var constant = Expression.Constant(empNo);
                var equals = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                
                return query.Where(lambda);
            }
        }

        /// <summary>
        /// Checks if user has permission to view specific record
        /// </summary>
        public static bool CanAccessRecord(UserSessionDetails session, string recordEmpNo, int? recordRegionId = null, int? recordLocationId = null)
        {
            return session?.CanAccessRecord(recordEmpNo, recordRegionId, recordLocationId) ?? false;
        }

        /// <summary>
        /// Checks if user can perform admin operations
        /// </summary>
        public static bool IsAdmin(UserSessionDetails session)
        {
            return session?.IsAdmin() ?? false;
        }

        /// <summary>
        /// Gets the highest access level of the user
        /// </summary>
        public static AccessLevel GetAccessLevel(UserSessionDetails session)
        {
            return session?.GetAccessLevel() ?? AccessLevel.None;
        }
    }
}
