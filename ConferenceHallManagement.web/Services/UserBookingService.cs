using ConferenceHallManagement.web.ViewModels;
using Repository_ConferenceHallManagement.AppDataRepositoy;
using Models_ConferenceHallManagement.AppDbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ConferenceHallManagement.web.Services
{
    public class UserBookingService : IUserBookingService
    {
        private readonly ICHSessionDataRepository _sessionRepo;
        private readonly ICHBookingSessionsDataRepository _bookingSessionRepo;
        private readonly IConferenceHallBookingDataRepository _mainBookingRepo;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserBookingService(
            ICHSessionDataRepository sessionRepo,
            ICHBookingSessionsDataRepository bookingSessionRepo,
            IConferenceHallBookingDataRepository mainBookingRepo,
            AuthenticationStateProvider authStateProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _sessionRepo = sessionRepo;
            _bookingSessionRepo = bookingSessionRepo;
            _mainBookingRepo = mainBookingRepo;
            _authStateProvider = authStateProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        // --- HELPER METHOD: MAP SESSION ID TO TIME RANGE ---
        private string GetSessionTimeRange(int sessionId, string fallbackText)
        {
            return sessionId switch
            {
                1 => "09:00 AM - 01:00 PM",
                2 => "02:00 PM - 05:00 PM",
                _ => fallbackText ?? "Unknown Slot"
            };
        }

        // --- HELPER METHOD: GET CURRENT USER DETAILS ---
        private async Task<(string UserId, string IpAddress)> GetCurrentUserDetails()
        {
            string userId = "System";
            string ipAddress = "Unknown";

            try
            {
                // Get User ID from Claims
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var nameIdentifierClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                    if (nameIdentifierClaim != null && !string.IsNullOrEmpty(nameIdentifierClaim.Value))
                    {
                        userId = nameIdentifierClaim.Value;
                    }
                }

                // Get IP Address
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var remoteIp = httpContext.Connection.RemoteIpAddress;
                    if (remoteIp != null)
                    {
                        ipAddress = remoteIp.ToString();
                        if (ipAddress == "::1")
                        {
                            ipAddress = "Localhost";
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If any error occurs, keep default values
            }

            return (userId, ipAddress);
        }

        // --- 1. CHECK AVAILABILITY ---
        public async Task<List<BookingDayVM>> CheckAvailabilityAsync(int hallId, DateTime startDate, DateTime endDate)
        {
            var resultList = new List<BookingDayVM>();
            var dbSessions = await _sessionRepo.GetConferenceHallSessionByHallId(hallId);

            var masterSessions = dbSessions.Select(s => new BookingSessionVM
            {
                SessionId = s.SessionId,
                SessionName = s.SessionEn
            }).ToList();

            for (var currentDate = startDate.Date; currentDate <= endDate.Date; currentDate = currentDate.AddDays(1))
            {
                var dayModel = new BookingDayVM { Date = currentDate };

                foreach (var session in masterSessions)
                {
                    var existingBooking = await _bookingSessionRepo.GetConferenceHallSessionBookingDetails(hallId, session.SessionId, currentDate);

                    bool isTaken = existingBooking != null;
                    string bookedBy = "";
                    string contact = "";

                    if (isTaken && existingBooking.Booking != null)
                    {
                        bookedBy = existingBooking.Booking.CreatedBy ?? "Unknown";
                        contact = "9876543210";
                    }

                    dayModel.Sessions.Add(new BookingSessionVM
                    {
                        SessionId = session.SessionId,
                        SessionName = session.SessionName,
                        IsBooked = isTaken,
                        IsSelected = false,
                        BookedByInfo = bookedBy,
                        ContactInfo = contact
                    });
                }
                resultList.Add(dayModel);
            }
            return resultList;
        }

        // --- 2. SAVE BOOKING (UPDATED WITH PENDING STATUS) ---
        public async Task<bool> SaveBookingAsync(int hallId, string programName, int attendees, string remarks, string userName, string phone, List<BookingDayVM> days)
        {
            try
            {
                // Get current user details for audit trailing
                var (userId, ipAddress) = await GetCurrentUserDetails();

                string finalRemarks = $"Phone: {phone} | {remarks}";

                var newBooking = new ConferenceHallBooking
                {
                    HallId = hallId,
                    ProgramName = programName,
                    NoOfAttendees = attendees,
                    Remarks = finalRemarks,
                    Status = 1, // CRITICAL: Set to 1 (Pending for Approval)
                    RoomTypeId = 1,
                    StartDate = days.First().Date,
                    EndDate = days.Last().Date,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    CreatedFrom = $"Web (IP: {ipAddress})",
                    UpdatedBy = userId,
                    UpdatedOn = DateTime.Now,
                    UpdatedFrom = "Web"
                };

                _mainBookingRepo.Add(newBooking);
                await _mainBookingRepo.GetContext().SaveChangesAsync();

                foreach (var day in days)
                {
                    foreach (var session in day.Sessions)
                    {
                        if (session.IsSelected)
                        {
                            var sessionRecord = new ConferenceHallBookingSession
                            {
                                BookingId = newBooking.BookingId,
                                HallId = hallId,
                                SessionId = session.SessionId,
                                BookingDate = day.Date,
                                Status = 1, // CRITICAL: Session also starts as Pending
                                CreatedBy = userId,
                                CreatedOn = DateTime.Now,
                                CreatedFrom = "Web",
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.Now,
                                UpdatedFrom = "Web"
                            };
                            _bookingSessionRepo.Add(sessionRecord);
                        }
                    }
                }
                await _bookingSessionRepo.GetContext().SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- NEW: AUTO-APPROVAL CHECK METHOD ---
        public async Task RunAutoApprovalCheckAsync()
        {
            try
            {
                var context = _mainBookingRepo.GetContext();
                var today = DateTime.Today;

                // Find all bookings that are:
                // 1. Status = 1 (Pending)
                // 2. StartDate <= Today (Booking date has arrived or passed)
                var pendingBookingsToAutoApprove = await context.ConferenceHallBookings
                    .Where(b => b.Status == 1 && b.StartDate <= today)
                    .ToListAsync();

                if (pendingBookingsToAutoApprove.Any())
                {
                    foreach (var booking in pendingBookingsToAutoApprove)
                    {
                        // Update booking status to 5 (Auto Approved)
                        booking.Status = 5;
                        booking.UpdatedOn = DateTime.Now;
                        booking.UpdatedBy = "System";
                        booking.Remarks = (booking.Remarks ?? "") + " [Auto-Approved by System]";
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Log error if needed, but don't throw to prevent dashboard crash
            }
        }

        // --- 3. GET MY BOOKINGS (USER SPECIFIC) ---
        public async Task<List<BookingListVM>> GetMyBookingsAsync(string empName)
        {
            try
            {
                Console.WriteLine($"[GetMyBookingsAsync] Starting with empName: '{empName}'");
                
                if (string.IsNullOrWhiteSpace(empName))
                {
                    Console.WriteLine("[GetMyBookingsAsync] empName is null or empty, returning empty list");
                    return new List<BookingListVM>();
                }
                
                // Reuse the main logic, but filter by username
                var allBookings = await FetchBookingsInternalAsync(usernameFilter: empName);
                Console.WriteLine($"[GetMyBookingsAsync] Fetched {allBookings.Count} bookings for user: {empName}");
                return allBookings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMyBookingsAsync] Exception: {ex.Message}");
                Console.WriteLine($"[GetMyBookingsAsync] StackTrace: {ex.StackTrace}");
                throw; // Re-throw so the UI can catch and display
            }
        }

        // --- 4. GET ALL BOOKINGS (ADMIN VIEW) ---
        public async Task<List<BookingListVM>> GetAllBookingsAsync()
        {
            // Just fetch data without auto-approval
            // Auto-approval will be explicitly triggered from Admin Dashboard
            return await FetchBookingsInternalAsync(usernameFilter: null);
        }

        // --- 5. ADMIN ACTION: APPROVE / REJECT ---
        public async Task<bool> UpdateBookingStatusAsync(int bookingId, int newStatus)
        {
            try
            {
                var context = _mainBookingRepo.GetContext();
                var booking = await context.ConferenceHallBookings.FindAsync(bookingId);

                if (booking == null) return false;

                booking.Status = newStatus; // 2 = Approved, 4 = Rejected
                booking.UpdatedOn = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // --- 6. CANCEL SESSION LOGIC ---
        public async Task<bool> CancelSessionAsync(int sessionRecordId)
        {
            try
            {
                var context = _bookingSessionRepo.GetContext();
                var sessionRecord = await context.ConferenceHallBookingSessions
                    .FirstOrDefaultAsync(x => x.Id == sessionRecordId);

                if (sessionRecord == null) return false;

                sessionRecord.Status = 3; // Cancelled
                sessionRecord.UpdatedOn = DateTime.Now;

                int parentBookingId = sessionRecord.BookingId;

                // Check if any other session is active in this booking
                bool hasOtherActiveSessions = await context.ConferenceHallBookingSessions
                    .AnyAsync(x => x.BookingId == parentBookingId && x.Id != sessionRecordId && x.Status != 3);

                if (!hasOtherActiveSessions)
                {
                    var parentBooking = await context.ConferenceHallBookings
                        .FirstOrDefaultAsync(x => x.BookingId == parentBookingId);

                    if (parentBooking != null)
                    {
                        parentBooking.Status = 3; // Entire booking cancelled
                    }
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- 7. ADMIN: REJECT INDIVIDUAL SESSION ---
        public async Task<bool> RejectSessionByAdminAsync(int sessionRecordId)
        {
            try
            {
                // Get current user details for audit trailing
                var (userId, ipAddress) = await GetCurrentUserDetails();

                var context = _bookingSessionRepo.GetContext();
                var sessionRecord = await context.ConferenceHallBookingSessions
                    .FirstOrDefaultAsync(x => x.Id == sessionRecordId);

                if (sessionRecord == null) return false;

                sessionRecord.Status = 4; // Rejected by Admin
                sessionRecord.UpdatedBy = userId;
                sessionRecord.UpdatedOn = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- 8. ADMIN: APPROVE BOOKING (Only Pending Sessions) ---
        public async Task<bool> ApproveBookingAsync(int bookingId)
        {
            try
            {
                // Get current user details for audit trailing
                var (userId, ipAddress) = await GetCurrentUserDetails();

                var context = _mainBookingRepo.GetContext();

                // Find the main booking
                var booking = await context.ConferenceHallBookings.FindAsync(bookingId);
                if (booking == null) return false;

                // Update main booking status to Approved
                booking.Status = 2; // Approved
                booking.UpdatedBy = userId;
                booking.UpdatedOn = DateTime.Now;

                // Get all sessions for this booking that are still pending (Status = 1)
                var pendingSessions = await context.ConferenceHallBookingSessions
                    .Where(s => s.BookingId == bookingId && s.Status == 1)
                    .ToListAsync();

                // Update ONLY pending sessions to Approved
                // Do NOT change rejected (4) or cancelled (3) sessions
                foreach (var session in pendingSessions)
                {
                    session.Status = 2; // Approved
                    session.UpdatedBy = userId;
                    session.UpdatedOn = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- 9. ADMIN: REJECT BOOKING (And All Pending Sessions) ---
        public async Task<bool> RejectBookingAsync(int bookingId)
        {
            try
            {
                // Get current user details for audit trailing
                var (userId, ipAddress) = await GetCurrentUserDetails();

                var context = _mainBookingRepo.GetContext();

                // Find the main booking
                var booking = await context.ConferenceHallBookings.FindAsync(bookingId);
                if (booking == null) return false;

                // Update main booking status to Rejected
                booking.Status = 4; // Rejected
                booking.UpdatedBy = userId;
                booking.UpdatedOn = DateTime.Now;

                // Get all sessions for this booking that are still pending (Status = 1)
                var pendingSessions = await context.ConferenceHallBookingSessions
                    .Where(s => s.BookingId == bookingId && s.Status == 1)
                    .ToListAsync();

                // Update ONLY pending sessions to Rejected
                // Do NOT change already approved (2), rejected (4), or cancelled (3) sessions
                foreach (var session in pendingSessions)
                {
                    session.Status = 4; // Rejected
                    session.UpdatedBy = userId;
                    session.UpdatedOn = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- HELPER: COMMON FETCH LOGIC ---
        private async Task<List<BookingListVM>> FetchBookingsInternalAsync(string? usernameFilter)
        {
            try
            {
                Console.WriteLine($"[FetchBookingsInternalAsync] Starting with usernameFilter: '{usernameFilter}'");
                
                var context = _mainBookingRepo.GetContext();

                var query = context.ConferenceHallBookings
                    .Include(b => b.Hall)
                    .Include(b => b.RoomType)
                    .Include(b => b.ConferenceHallBookingSessions)
                    .ThenInclude(bs => bs.Session)
                    .Where(b => b.Status != 3); // Exclude deleted

                // Apply filter if username is provided (User View), otherwise fetch all (Admin View)
                if (!string.IsNullOrEmpty(usernameFilter))
                {
                    query = query.Where(b => b.CreatedBy == usernameFilter);
                }

                var bookings = await query.OrderByDescending(b => b.CreatedOn).ToListAsync();
                Console.WriteLine($"[FetchBookingsInternalAsync] Found {bookings.Count} bookings in database");

                var list = new List<BookingListVM>();

                foreach (var b in bookings)
                {
                    var allSessions = b.ConferenceHallBookingSessions
                        .Select(s => new BookingSessionDetailVM
                        {
                            RecordId = s.Id,
                            Date = s.BookingDate,
                            SessionTime = GetSessionTimeRange(s.SessionId, s.Session?.SessionEn),
                            Status = s.Status == 1 ? "Pending" :
                                     s.Status == 2 ? "Approved" :
                                     s.Status == 3 ? "Cancelled" :
                                     s.Status == 4 ? "Rejected" : "Unknown",
                            StatusCode = s.Status,
                            IsCancelled = s.Status == 3
                        })
                        .OrderBy(s => s.Date)
                        .ToList();

                    var activeSessions = allSessions.Where(x => !x.IsCancelled).ToList();

                    // Skip bookings with no active sessions only for user view
                    if (!activeSessions.Any() && usernameFilter != null)
                    {
                        Console.WriteLine($"[FetchBookingsInternalAsync] Skipping booking {b.BookingId} - no active sessions");
                        continue;
                    }

                    // Status mapping based on your requirements
                    string bookingStatus = b.Status switch
                    {
                        1 => "Pending For Approval",
                        2 => "Approved",
                        3 or 6 or 7 => "Self-Cancelled",
                        4 => "Cancelled By Admin", // Rejected
                        5 => "Auto Approved",
                        13 or 25 => "Confirmed",
                        _ => $"Unknown ({b.Status})"
                    };

                    list.Add(new BookingListVM
                    {
                        BookingId = b.BookingId,
                        HallName = b.Hall != null ? b.Hall.HallNameEn : "Unknown Hall",
                        SeatingType = b.RoomType != null ? b.RoomType.RoomTypeEn : "Standard",
                        FromDate = b.StartDate.ToString("dd-MMM-yyyy"),
                        ToDate = b.EndDate.ToString("dd-MMM-yyyy"),
                        ProgramName = b.ProgramName ?? "",
                        Attendees = b.NoOfAttendees,
                        Remarks = b.Remarks ?? "",
                        BookingStatus = bookingStatus,
                        BookedBy = b.CreatedBy ?? "Unknown",
                        TotalSessionsBooked = allSessions.Count,
                        ActiveSessionsCount = activeSessions.Count,
                        SessionDetails = activeSessions
                    });
                }

                Console.WriteLine($"[FetchBookingsInternalAsync] Returning {list.Count} bookings after filtering");
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FetchBookingsInternalAsync] Exception: {ex.Message}");
                Console.WriteLine($"[FetchBookingsInternalAsync] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // --- 10. GET BOOKINGS BY HALL ID ---
        public async Task<List<BookingListVM>> GetBookingsByHallIdAsync(int hallId)
        {
            var context = _mainBookingRepo.GetContext();

            var query = context.ConferenceHallBookings
                .Include(b => b.Hall)
                .Include(b => b.RoomType)
                .Include(b => b.ConferenceHallBookingSessions)
                .ThenInclude(bs => bs.Session)
                .Where(b => b.HallId == hallId && b.Status != 3); // Filter by HallId and exclude deleted

            var bookings = await query.OrderByDescending(b => b.CreatedOn).ToListAsync();

            var list = new List<BookingListVM>();

            foreach (var b in bookings)
            {
                var allSessions = b.ConferenceHallBookingSessions
                    .Select(s => new BookingSessionDetailVM
                    {
                        RecordId = s.Id,
                        Date = s.BookingDate,
                        SessionTime = GetSessionTimeRange(s.SessionId, s.Session?.SessionEn),
                        Status = s.Status == 1 ? "Pending" :
                                 s.Status == 2 ? "Approved" :
                                 s.Status == 3 ? "Cancelled" :
                                 s.Status == 4 ? "Rejected" : "Unknown",
                        StatusCode = s.Status,
                        IsCancelled = s.Status == 3
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                var activeSessions = allSessions.Where(x => !x.IsCancelled).ToList();

                // For hall-specific bookings, show all bookings including those with no active sessions
                string bookingStatus = b.Status switch
                {
                    1 => "Pending For Approval",
                    2 => "Approved",
                    3 or 6 or 7 => "Self-Cancelled",
                    4 => "Cancelled By Admin",
                    5 => "Auto Approved",
                    13 or 25 => "Confirmed",
                    _ => $"Unknown ({b.Status})"
                };

                list.Add(new BookingListVM
                {
                    BookingId = b.BookingId,
                    HallName = b.Hall != null ? b.Hall.HallNameEn : "Unknown Hall",
                    SeatingType = b.RoomType != null ? b.RoomType.RoomTypeEn : "Standard",
                    FromDate = b.StartDate.ToString("dd-MMM-yyyy"),
                    ToDate = b.EndDate.ToString("dd-MMM-yyyy"),
                    ProgramName = b.ProgramName ?? "",
                    Attendees = b.NoOfAttendees,
                    Remarks = b.Remarks ?? "",
                    BookingStatus = bookingStatus,
                    BookedBy = b.CreatedBy ?? "Unknown",
                    TotalSessionsBooked = allSessions.Count,
                    ActiveSessionsCount = activeSessions.Count,
                    SessionDetails = activeSessions
                });
            }

            return list;
        }

        // --- 11. CANCEL ENTIRE BOOKING (USER) ---
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            try
            {
                // Get current user details for audit trailing
                var (userId, ipAddress) = await GetCurrentUserDetails();

                var context = _mainBookingRepo.GetContext();

                // Find the main booking
                var booking = await context.ConferenceHallBookings.FindAsync(bookingId);
                if (booking == null) return false;

                // Update main booking status to Cancelled
                booking.Status = 3; // Cancelled by user
                booking.UpdatedBy = userId;
                booking.UpdatedOn = DateTime.Now;

                // Get all active sessions for this booking (not already cancelled)
                var activeSessions = await context.ConferenceHallBookingSessions
                    .Where(s => s.BookingId == bookingId && s.Status != 3)
                    .ToListAsync();

                // Cancel all active sessions
                foreach (var session in activeSessions)
                {
                    session.Status = 3; // Cancelled
                    session.UpdatedBy = userId;
                    session.UpdatedOn = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // --- 12. UPDATE INDIVIDUAL SESSION STATUS ---
        public async Task<bool> UpdateSessionStatusAsync(int sessionId, int status)
        {
            try
            {
                // Get current user details for audit trailing
                var (userId, ipAddress) = await GetCurrentUserDetails();

                var context = _bookingSessionRepo.GetContext();
                var sessionRecord = await context.ConferenceHallBookingSessions
                    .FirstOrDefaultAsync(x => x.Id == sessionId);

                if (sessionRecord == null) return false;

                // Update session status
                sessionRecord.Status = status;
                sessionRecord.UpdatedBy = userId;
                sessionRecord.UpdatedOn = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}