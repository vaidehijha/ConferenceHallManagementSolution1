using System;
using System.Collections.Generic;

namespace ConferenceHallManagement.web.ViewModels
{
    // Ye main ViewModel hai jo tumhare Form/Page ka data hold karega
    public class UserBookingVM
    {
        // User jo form bharega uska data
        public int SelectedHallId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public string Remarks { get; set; } = string.Empty;
        public int NumberOfAttendees { get; set; }
    }

    // --- SUPPORTING CLASSES (Jo Loop aur Availability check ke liye chahiye) ---

    // 1. Ek poora din (Date) represent karne ke liye
    public class BookingDayVM
    {
        public DateTime Date { get; set; }

        // UI par Date acche format mein dikhane ke liye
        public string DisplayDate => Date.ToString("dd-MMM-yyyy (dddd)");

        public List<BookingSessionVM> Sessions { get; set; } = new List<BookingSessionVM>();
    }

    // 2. Ek specific session (Morning/Evening) represent karne ke liye
    public class BookingSessionVM
    {
        public int SessionId { get; set; }

        public string SessionName { get; set; } = string.Empty;

        public bool IsBooked { get; set; }      // True = Disabled/Red (Already Booked)
        public bool IsSelected { get; set; }    // True = User ne tick kiya

        
        public string BookedByInfo { get; set; } = ""; // User ka Naam
        public string ContactInfo { get; set; } = "";  // Phone Number
    }
}