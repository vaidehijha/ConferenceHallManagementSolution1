using System;
using System.Collections.Generic;

namespace ConferenceHallManagement.web.ViewModels
{
    // 1. MAIN ROW (Jo table me dikhega)
    public class BookingListVM
    {
        public int BookingId { get; set; }
        public string HallName { get; set; } = "";
        public string SeatingType { get; set; } = "";
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public string ProgramName { get; set; } = "";
        public int Attendees { get; set; }
        public string Remarks { get; set; } = "";
        public string BookingStatus { get; set; } = "";
        public string BookedBy { get; set; } = "";
        public int ActiveSessionsCount { get; set; }
        public int TotalSessionsBooked { get; set; }

        // Iske andar sessions ki list hogi (Jo modal me dikhegi)
        public List<BookingSessionDetailVM> SessionDetails { get; set; } = new();
    }

    // 2. INNER ROW (Jo Popup/Modal me dikhega)
    public class BookingSessionDetailVM
    {
        public int RecordId { get; set; } // Session Table ki Primary Key
        public DateTime Date { get; set; }
        public string DisplayDate => Date.ToString("dd-MMM-yyyy (dddd)");
        public string SessionTime { get; set; } = ""; // "9:00 AM - 1:00 PM"
        public string Status { get; set; } = "";
        public int StatusCode { get; set; } // Numeric status code
        public bool IsCancelled { get; set; }
    }
}