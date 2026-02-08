using Models_ConferenceHallManagement.AppDbModels;
using ConferenceHallManagement.Web.ViewModels;
using UoW_ConferenceHallManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConferenceHallManagement.web.Services
{
    public interface ITempEmployeeRoleBlazorService
    {
        Task<IEnumerable<TempEmployeeRoleVM>> GetAllAsync();
        Task<TempEmployeeRoleVM?> GetByIdAsync(int id);
        Task<int> CreateAsync(TempEmployeeRoleVM model);
        Task<int> UpdateAsync(TempEmployeeRoleVM model);
        Task<int> DeleteAsync(int id);
    }

    public class TempEmployeeRoleBlazorService : ITempEmployeeRoleBlazorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TempEmployeeRoleBlazorService> _logger;

        public TempEmployeeRoleBlazorService(
            IUnitOfWork unitOfWork,
            ILogger<TempEmployeeRoleBlazorService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TempEmployeeRoleVM>> GetAllAsync()
        {
            try
            {
                // 'EmpRole' Repo se data la rahe hain
                var entities = await _unitOfWork.EmpRole.GetAllAsync();

                return entities.Where(x => x.Status == true)
                    .Select(e => new TempEmployeeRoleVM
                    {
                        Id = e.Id,
                        EmployeeNo = e.EmpNo ?? "",
                        RegionId = e.RegionId ?? 0,
                        LocationId = e.LocationId ?? 0,
                        RoleId = e.RoleId,
                        IsActive = e.Status
                    }).OrderByDescending(x => x.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync error");
                return Enumerable.Empty<TempEmployeeRoleVM>();
            }
        }

        public async Task<TempEmployeeRoleVM?> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.EmpRole.GetAsync(id);
                if (entity == null) return null;

                return new TempEmployeeRoleVM
                {
                    Id = entity.Id,
                    EmployeeNo = entity.EmpNo ?? "",
                    RegionId = entity.RegionId ?? 0,
                    LocationId = entity.LocationId ?? 0,
                    RoleId = entity.RoleId,
                    IsActive = entity.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetByIdAsync error: {id}");
                return null;
            }
        }

        public async Task<int> CreateAsync(TempEmployeeRoleVM model)
        {
            try
            {
                var currentTime = DateTime.Now;

                // Ab hum 'CreatedBy' aur baaki fields set kar sakte hain
                // kyunki EmpRole.cs me ab ye properties hain
                var entity = new EmpRole
                {
                    EmpNo = model.EmployeeNo,
                    RegionId = model.RegionId,
                    LocationId = model.LocationId,
                    RoleId = model.RoleId,
                    Status = true,

                    // Audit Fields
                    CreatedBy = "System", // Ya logged-in user ka ID agar available ho
                    CreatedOn = currentTime,
                    CreatedFrom = "Blazor",
                    UpdatedBy = "System",
                    UpdatedOn = currentTime,
                    UpdatedFrom = "Blazor"
                };

                _unitOfWork.EmpRole.Add(entity);
                return await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync error");
                return 0;
            }
        }

        public async Task<int> UpdateAsync(TempEmployeeRoleVM model)
        {
            try
            {
                var existingData = await _unitOfWork.EmpRole.GetAsync(model.Id);

                if (existingData == null) return 0;

                existingData.EmpNo = model.EmployeeNo;
                existingData.RegionId = model.RegionId;
                existingData.LocationId = model.LocationId;
                existingData.RoleId = model.RoleId;
                existingData.Status = model.IsActive;

                // Audit Fields Update
                existingData.UpdatedBy = "System";
                existingData.UpdatedOn = DateTime.Now;
                existingData.UpdatedFrom = "Blazor";

                _unitOfWork.EmpRole.Update(existingData);
                return await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateAsync error ID: {model.Id}");
                return 0;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                var existingData = await _unitOfWork.EmpRole.GetAsync(id);

                if (existingData == null) return 0;

                existingData.Status = false; // Soft delete

                // Audit Fields Update
                existingData.UpdatedBy = "System";
                existingData.UpdatedOn = DateTime.Now;
                existingData.UpdatedFrom = "Blazor";

                _unitOfWork.EmpRole.Update(existingData);
                return await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteAsync error ID: {id}");
                return 0;
            }
        }
    }
}