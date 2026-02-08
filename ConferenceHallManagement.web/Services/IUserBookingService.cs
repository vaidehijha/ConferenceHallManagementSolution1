using ConferenceHallManagement.web.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConferenceHallManagement.web.Services
{
    public interface IUserBookingService
    {
        // 1. Availability Check
        Task<List<BookingDayVM>> CheckAvailabilityAsync(int hallId, DateTime startDate, DateTime endDate);

        // 2. Save New Booking
        Task<bool> SaveBookingAsync(int hallId, string programName, int attendees, string remarks, string userName, string phone, List<BookingDayVM> days);

        // 3. Get User Bookings (Sirf Logged-in User ka data) -- YE MISSING THA
        Task<List<BookingListVM>> GetMyBookingsAsync(string username);

        // 4. Get ALL Bookings (Admin Dashboard ke liye)
        Task<List<BookingListVM>> GetAllBookingsAsync();

        // 5. Cancel Session (User & Admin both)
        Task<bool> CancelSessionAsync(int sessionRecordId);

        // 6. ADMIN STATUS UPDATE (Approve/Reject)
        Task<bool> UpdateBookingStatusAsync(int bookingId, int newStatus);

        // 7. ADMIN: Reject Individual Session
        Task<bool> RejectSessionByAdminAsync(int sessionRecordId);

        // 8. ADMIN: Approve Booking (only pending sessions)
        Task<bool> ApproveBookingAsync(int bookingId);

        // 9. ADMIN: Reject Booking and all pending sessions
        Task<bool> RejectBookingAsync(int bookingId);

        // 10. AUTO-APPROVAL: Check and auto-approve pending bookings
        Task RunAutoApprovalCheckAsync();

        // 11. Get Bookings by Hall ID
        Task<List<BookingListVM>> GetBookingsByHallIdAsync(int hallId);

        // 12. Cancel Entire Booking (User)
        Task<bool> CancelBookingAsync(int bookingId);

        // 13. Update Individual Session Status
        Task<bool> UpdateSessionStatusAsync(int sessionId, int status);
    }
}