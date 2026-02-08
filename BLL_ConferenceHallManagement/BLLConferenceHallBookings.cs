using Models_ConferenceHallManagement.AppDbModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UoW_ConferenceHallManagement;

namespace BLL_ConferenceHallManagement
{
    public interface IBLLConferenceHallBookings
    {
        Task<int> AddConferenceHallBooking(ConferenceHallBooking booking);
        Task<int> UpdateConferenceHallBooking(ConferenceHallBooking booking);
        Task<ConferenceHallBooking> GetConferenceHallBookingBybookingId(int bookingId);
        Task<IEnumerable<ConferenceHallBooking>?> GetAllConferenceHallBookings();
    }

    public class BLLConferenceHallBookings: IBLLConferenceHallBookings
    {
        private readonly IUnitOfWork _unitOfWork;

        public BLLConferenceHallBookings(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddConferenceHallBooking(ConferenceHallBooking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking), "Booking cannot be null");
            }
            await _unitOfWork.ConferenceHallBookingDataRepository.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();
            return booking.BookingId;
        }

        public async Task<int> UpdateConferenceHallBooking(ConferenceHallBooking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking), "Booking cannot be null");
            }
            await _unitOfWork.SaveChangesAsync();
            return booking.BookingId;
        }

        public async Task<ConferenceHallBooking> GetConferenceHallBookingBybookingId(int bookingId)
        {
            var bookings = await _unitOfWork.ConferenceHallBookingDataRepository.GetBookingsByUserIdAsync("");
            var booking = bookings as ConferenceHallBooking;
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
            }
            return booking;
        }

        public async Task<IEnumerable<ConferenceHallBooking>?> GetAllConferenceHallBookings()
        {
            var dataList = await _unitOfWork.ConferenceHallBookingDataRepository.GetFilteredBookings(1, null, null);
            return dataList;
        }
    }
}
