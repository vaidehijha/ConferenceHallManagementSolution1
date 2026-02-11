using Models_ConferenceHallManagement.DTOs;
using Models_ConferenceHallManagement.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ConferenceHallManagement.Web.Services
{
    public class BookingAccessFilter
    {
        public static List<T> FilterBookingsByRole<T>(
            List<T> allBookings, 
            UserSessionDetails userSession,
            System.Func<T, int?> getRegionId,
            System.Func<T, int?> getLocationId,
            System.Func<T, string> getEmpNo) where T : class
        {
            if (userSession == null || !userSession.Roles.Any())
                return new List<T>();

            var primaryRole = userSession.GetPrimaryRole();
            if (primaryRole == null)
                return new List<T>();

            // Super Admin (RoleId = 1) - See ALL bookings
            if (primaryRole.RoleId == 1)
            {
                return allBookings;
            }

            // Central Admin (RoleId = 2) - See ALL bookings
            if (primaryRole.RoleId == 2)
            {
                return allBookings;
            }

            // Regional Admin (RoleId = 3) - See bookings in their region only
            if (primaryRole.RoleId == 3 && primaryRole.RegionId.HasValue)
            {
                return allBookings
                    .Where(b => getRegionId(b).HasValue && getRegionId(b).Value == primaryRole.RegionId.Value)
                    .ToList();
            }

            // Station Admin (RoleId = 4) - See bookings in their location only
            if (primaryRole.RoleId == 4 && primaryRole.LocationId.HasValue)
            {
                return allBookings
                    .Where(b => getLocationId(b).HasValue && getLocationId(b).Value == primaryRole.LocationId.Value)
                    .ToList();
            }

            // Normal Employee (RoleId = 0) - See own bookings only
            return allBookings
                .Where(b => getEmpNo(b) == userSession.EmpNo)
                .ToList();
        }

        public static bool CanApproveReject(UserSessionDetails? userSession, int? bookingLocationId)
        {
            if (userSession == null || !userSession.Roles.Any())
                return false;

            var primaryRole = userSession.GetPrimaryRole();
            if (primaryRole == null)
                return false;

            // Super Admin (1) - can approve all
            if (primaryRole.RoleId == 1)
                return true;

            // Station Admin (4) - can approve only their location's bookings
            if (primaryRole.RoleId == 4 && primaryRole.LocationId.HasValue && bookingLocationId.HasValue)
                return primaryRole.LocationId.Value == bookingLocationId.Value;

            // Central Admin (2) and Regional Admin (3) - cannot approve (view only)
            return false;
        }

        public static string GetAccessDescription(UserSessionDetails userSession)
        {
            if (userSession == null || !userSession.Roles.Any())
                return "Employee Access";

            var primaryRole = userSession.GetPrimaryRole();
            if (primaryRole == null)
                return "Employee Access";

            return primaryRole.RoleId switch
            {
                1 => "Full Access - All Bookings",
                2 => "Full Access - All Bookings (View Only)",
                3 => $"Regional Access - {primaryRole.RegionName} Region Only (View Only)",
                4 => $"Location Access - {primaryRole.LocationName} Only (Can Approve)",
                _ => "Employee Access - Own Bookings Only"
            };
        }
    }
}
