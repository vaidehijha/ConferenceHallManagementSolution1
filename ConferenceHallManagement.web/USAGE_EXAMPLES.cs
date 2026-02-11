/*
 * ROLE-BASED FILTERING USAGE EXAMPLES
 * ====================================
 * 
 * This file contains examples of how to use the new Claims-based authentication
 * system with role-based filtering in your repositories and services.
 */

// ============================================================================
// EXAMPLE 1: Using RoleBasedFilterExtensions in a Repository
// ============================================================================

/*
using Models_ConferenceHallManagement.DTOs;
using Models_ConferenceHallManagement.Helpers;
using Microsoft.EntityFrameworkCore;

public class BookingRepository
{
    private readonly ConferenceHallManagementContext _context;
    
    // Method to get bookings filtered by user role
    public async Task<List<ConferenceHallBooking>> GetBookingsForUserAsync(UserSessionDetails userSession)
    {
        var query = _context.ConferenceHallBookings
            .Where(b => b.Status == true);

        // Apply role-based filter
        // Super Admin/Central Admin: See all
        // Regional Admin: See bookings in their region
        // Station Admin: See bookings in their location
        // User: See only own bookings
        
        query = query.ApplyRoleFilter(
            userSession,
            createdBySelector: b => b.CreatedBy,
            regionIdSelector: b => b.RegionId,      // Assuming your entity has RegionId
            locationIdSelector: b => b.LocationId    // Assuming your entity has LocationId
        );

        return await query.OrderByDescending(b => b.CreatedOn).ToListAsync();
    }
    
    // Check if user can access specific booking
    public async Task<bool> CanUserEditBookingAsync(int bookingId, UserSessionDetails userSession)
    {
        var booking = await _context.ConferenceHallBookings
            .Where(b => b.BookingId == bookingId)
            .Select(b => new { b.CreatedBy, b.RegionId, b.LocationId })
            .FirstOrDefaultAsync();

        if (booking == null) return false;

        return RoleBasedFilterExtensions.CanAccessRecord(
            userSession, 
            booking.CreatedBy, 
            booking.RegionId, 
            booking.LocationId
        );
    }
}
*/

// ============================================================================
// EXAMPLE 2: Using UserSessionService in Blazor Component
// ============================================================================

/*
@page "/my-bookings"
@rendermode InteractiveServer
@inject UserSessionService SessionService
@inject IBookingRepository BookingRepo

<h3>My Bookings</h3>

@if (bookings != null)
{
    @foreach (var booking in bookings)
    {
        <div class="card mb-2">
            <div class="card-body">
                <h5>@booking.BookingTitle</h5>
                <p>Created by: @booking.CreatedBy</p>
            </div>
        </div>
    }
}

@code {
    private List<ConferenceHallBooking> bookings;
    private UserSessionDetails userSession;

    protected override async Task OnInitializedAsync()
    {
        // Get user session from claims (NO DATABASE HIT!)
        userSession = await SessionService.GetUserSessionAsync();
        
        // Get filtered bookings based on user's role
        bookings = await BookingRepo.GetBookingsForUserAsync(userSession);
    }
    
    // Check if current user can edit a booking
    private bool CanEdit(ConferenceHallBooking booking)
    {
        return RoleBasedFilterExtensions.CanAccessRecord(
            userSession, 
            booking.CreatedBy,
            booking.RegionId,
            booking.LocationId
        );
    }
}
*/

// ============================================================================
// EXAMPLE 3: Using ClaimsHelper in MVC Controller
// ============================================================================

/*
using ConferenceHallManagement.Web.Services;
using Models_ConferenceHallManagement.Helpers;

public class BookingController : Controller
{
    private readonly IBookingRepository _repo;
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        // Get user session from claims (no DB hit)
        var userSession = ClaimsHelper.GetSessionFromClaims(User);
        
        // Get filtered bookings
        var bookings = await _repo.GetBookingsForUserAsync(userSession);
        
        return View(bookings);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var userSession = ClaimsHelper.GetSessionFromClaims(User);
        
        // Check if user can access this booking
        if (!await _repo.CanUserEditBookingAsync(id, userSession))
            return Forbid();
            
        var booking = await _repo.GetBookingByIdAsync(id);
        return View(booking);
    }
}
*/

// ============================================================================
// EXAMPLE 4: Conditional UI Based on Role
// ============================================================================

/*
@inject UserSessionService SessionService

@code {
    private UserSessionDetails session;
    
    protected override async Task OnInitializedAsync()
    {
        session = await SessionService.GetUserSessionAsync();
    }
}

<AuthorizeView Roles="Super Admin,Central Admin">
    <Authorized>
        <button class="btn btn-primary">Create Hall</button>
    </Authorized>
</AuthorizeView>

@if (session != null && RoleBasedFilterExtensions.IsAdmin(session))
{
    <div class="admin-panel">
        <h4>Admin Panel</h4>
        <p>Access Level: @RoleBasedFilterExtensions.GetAccessLevel(session)</p>
    </div>
}

@* Show user's roles and locations *@
@if (session?.Roles != null)
{
    <div class="user-roles">
        <h5>Your Roles:</h5>
        @foreach (var role in session.Roles)
        {
            <span class="badge bg-info">
                @role.RoleName - @role.RegionName - @role.LocationName
            </span>
        }
    </div>
}
*/

// ============================================================================
// EXAMPLE 5: Multiple Role Switching
// ============================================================================

/*
@page "/bookings-multi-role"
@inject UserSessionService SessionService

<h3>Bookings</h3>

@if (userSession?.Roles.Count > 1)
{
    <div class="mb-3">
        <label>View as:</label>
        <select @onchange="OnRoleChange">
            @foreach (var role in userSession.Roles)
            {
                <option value="@userSession.Roles.IndexOf(role)">
                    @role.RoleName - @role.RegionName - @role.LocationName
                </option>
            }
        </select>
    </div>
}

@code {
    private UserSessionDetails userSession;
    private UserRoleInfo selectedRole;
    private List<ConferenceHallBooking> bookings;
    
    protected override async Task OnInitializedAsync()
    {
        userSession = await SessionService.GetUserSessionAsync();
        if (userSession?.Roles.Any() == true)
        {
            selectedRole = userSession.Roles.First();
            await LoadBookings();
        }
    }
    
    private async Task OnRoleChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int index))
        {
            selectedRole = userSession.Roles[index];
            await LoadBookings();
        }
    }
    
    private async Task LoadBookings()
    {
        // Use selectedRole to filter bookings
        var query = _context.ConferenceHallBookings.AsQueryable();
        
        query = query.ApplyRoleBasedFilter(
            userSession,
            empNoSelector: b => b.CreatedBy,
            regionIdSelector: b => b.RegionId,
            locationIdSelector: b => b.LocationId,
            selectedRole: selectedRole  // Pass specific role
        );
        
        bookings = await query.ToListAsync();
        StateHasChanged();
    }
}
*/

// ============================================================================
// EXAMPLE 6: Custom Authorization Policy
// ============================================================================

/*
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Super Admin", "Central Admin", "Regional Admin", "Station Admin"));
        
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole("Super Admin"));
        
    options.AddPolicy("RequireRegionalAdmin", policy =>
        policy.RequireRole("Super Admin", "Central Admin", "Regional Admin"));
});

// In Controller
[Authorize(Policy = "RequireAdmin")]
public class AdminController : Controller
{
    // Only admins can access
}

// In Blazor
<AuthorizeView Policy="RequireSuperAdmin">
    <Authorized>
        <button>Super Admin Only Action</button>
    </Authorized>
</AuthorizeView>
*/

// ============================================================================
// EXAMPLE 7: Service Layer with Role Filtering
// ============================================================================

/*
public interface IBookingService
{
    Task<List<BookingDTO>> GetUserBookingsAsync(UserSessionDetails session);
    Task<BookingDTO> CreateBookingAsync(CreateBookingRequest request, UserSessionDetails session);
}

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _uow;
    
    public async Task<List<BookingDTO>> GetUserBookingsAsync(UserSessionDetails session)
    {
        var query = _uow.BookingRepository.GetAllQuery();
        
        // Apply role filter
        query = query.ApplyRoleFilter(
            session,
            b => b.CreatedBy,
            b => b.RegionId,
            b => b.LocationId
        );
        
        var bookings = await query.ToListAsync();
        return bookings.Select(b => MapToDTO(b)).ToList();
    }
    
    public async Task<BookingDTO> CreateBookingAsync(CreateBookingRequest request, UserSessionDetails session)
    {
        // Auto-populate region/location from user's role
        var primaryRole = session.Roles.FirstOrDefault(r => r.RoleName == session.PrimaryRole);
        
        var booking = new ConferenceHallBooking
        {
            BookingTitle = request.Title,
            CreatedBy = session.EmpNo,
            RegionId = primaryRole?.RegionId,
            LocationId = primaryRole?.LocationId,
            CreatedOn = DateTime.Now
        };
        
        _uow.BookingRepository.Add(booking);
        await _uow.SaveChangesAsync();
        
        return MapToDTO(booking);
    }
}
*/
