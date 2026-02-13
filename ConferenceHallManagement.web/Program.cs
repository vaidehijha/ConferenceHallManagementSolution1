using ConferenceHallManagement.web.Components;
using ConferenceHallManagement.Web.Services;
using ConferenceHallManagement.web.Services;
using BLL_ConferenceHallManagement;
using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using Repository_ConferenceHallManagement.AppDataRepositoy;
using Repository_ConferenceHallManagement.UtilityRepository;
using UoW_ConferenceHallManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- CONTROLLERS ---
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// --- COOKIE AUTHENTICATION ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PowerGrid_Auth_Cookie";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// --- CASCADING AUTH STATE WITH COOKIE SUPPORT ---
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// --- USER SESSION SERVICE (Claims-based, no session storage) ---
builder.Services.AddScoped<UserSessionService>();

// --- DB CONTEXT ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ConferenceHallManagementContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var connectionStringEmp = builder.Configuration.GetConnectionString("EmpdetConnection");
builder.Services.AddDbContext<EmpdetContext>(options =>
{
    options.UseSqlServer(connectionStringEmp);
});

// --- REPOSITORIES ---
builder.Services.AddScoped<IMasterBookingStatusDataRepository, MasterBookingStatusDataRepository>();
builder.Services.AddScoped<IMasterRoomTypeDataRepository, MasterRoomTypeDataRepository>();
builder.Services.AddScoped<IMasterTempEmployeeRoleDataRepository, MasterTempEmployeeRoleDataRepository>();
builder.Services.AddScoped<IConferenceHallBookingDataRepository, ConferenceHallBookingDataRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmpRoleRepository, EmpRoleRepository>();
builder.Services.AddScoped<Repository_ConferenceHallManagement.AppDataRepositoy.IConferenceHallDataRepository, Repository_ConferenceHallManagement.AppDataRepositoy.ConferenceHallDataRepository>();
builder.Services.AddScoped<ICHSessionDataRepository, CHSessionDataRepository>();
builder.Services.AddScoped<ICHBookingSessionsDataRepository, CHBookingSessionsDataRepository>();
builder.Services.AddScoped<IMasterRegionDataRepository, MasterRegionDataRepository>();
builder.Services.AddScoped<IMasterLocationDataRepository, MasterLocationDataRepository>();
builder.Services.AddScoped<IMasterRoleDataRepository, MasterRoleDataRepository>();

// --- UNIT OF WORK ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- BUSINESS LOGIC LAYER (BLL) ---
builder.Services.AddScoped<IBllEmployee, BLLEmployee>();
builder.Services.AddScoped<IBllAuthenticationService, BllAuthenticationService>();

// --- BLAZOR SERVICES ---
builder.Services.AddScoped<IMasterBookingStatusBlazorService, MasterBookingStatusBlazorService>();
builder.Services.AddScoped<IMasterRoomTypeBlazorService, MasterRoomTypeBlazorService>();
builder.Services.AddScoped<ITempEmployeeRoleBlazorService, TempEmployeeRoleBlazorService>();
builder.Services.AddScoped<ConferenceHallManagement.web.Services.HallConfigurationService>();
builder.Services.AddScoped<IUserBookingService, UserBookingService>();
builder.Services.AddScoped<IHallConfigurationService, HallConfigurationService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<ISessionConfigService, SessionConfigService>();

// --- BUILD ---
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// --- AUTH MIDDLEWARE (ORDER IS CRITICAL) ---
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// --- MAP CONTROLLERS BEFORE RAZOR COMPONENTS ---
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();