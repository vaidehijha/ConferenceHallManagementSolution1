using Models_ConferenceHallManagement.AppDbModels;
using Models_ConferenceHallManagement; // Added to find 'HallConfigurationModel'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UoW_ConferenceHallManagement;

namespace BLL_ConferenceHallManagement
{
    public interface IBLLConferenceHall
    {
        Task<int> AddConferenceHall(ConferenceHall conferenceHall);
        Task<int> UpdateConferenceHall(ConferenceHall conferenceHall);
        Task<ConferenceHall> GetConferenceHallByconferenceHallId(int hallId);
        Task<IEnumerable<ConferenceHall>?> GetAllConferenceHalls();
    }

    public class BLLConferenceHall : IBLLConferenceHall
    {
        private readonly IUnitOfWork _unitOfWork;

        public BLLConferenceHall(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddConferenceHall(ConferenceHall conferenceHall)
        {
            if (conferenceHall == null)
            {
                throw new ArgumentNullException(nameof(conferenceHall), "Conference Hall cannot be null");
            }
            await _unitOfWork.ConferenceHallDataRepository.AddAsync(conferenceHall);
            await _unitOfWork.SaveChangesAsync();
            return conferenceHall.HallId;
        }

        public async Task<int> UpdateConferenceHall(ConferenceHall conferenceHall)
        {
            if (conferenceHall == null)
            {
                throw new ArgumentNullException(nameof(conferenceHall), "Conference Hall cannot be null");
            }
            await _unitOfWork.ConferenceHallDataRepository.UpdateHallWithSessionsAsync(conferenceHall);
            return conferenceHall.HallId;
        }

        public async Task<ConferenceHall> GetConferenceHallByconferenceHallId(int hallId)
        {
            var conferenceHall = await _unitOfWork.ConferenceHallDataRepository.GetHallByIdAsync(hallId);
            if (conferenceHall == null)
            {
                throw new KeyNotFoundException($"Hall with ID {hallId} not found.");
            }
            return conferenceHall;
        }

        public async Task<IEnumerable<ConferenceHall>?> GetAllConferenceHalls()
        {
            var dataList = await _unitOfWork.ConferenceHallDataRepository.GetAllHallsAsync();
            return dataList?.Where(x => x.Status == true);
        }
    }
}