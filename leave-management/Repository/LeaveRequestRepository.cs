using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(LeaveRequest entity)
        {
           await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            return await _db.LeaveRequests
                .Include(Q => Q.RequestingEmployee)
                .Include(Q => Q.ApprovedBy)
                .Include(Q => Q.LeaveType)
                .ToListAsync();
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            var leaveHistory =await _db.LeaveRequests
                .Include(Q => Q.RequestingEmployee)
                .Include(Q => Q.ApprovedBy)
                .Include(Q => Q.LeaveType)
                .FirstOrDefaultAsync(q => q.Id == id);
            return leaveHistory;
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveRequestByEmployee(string EmployeeId)
        {
            var leaveHistory = await FindAll();
             return  leaveHistory.Where(q => q.RequestingEmployeeId == EmployeeId).ToList();
       
        }

        public async Task<bool> isExists(int id)
        {
            var exist =await _db.LeaveRequests.AnyAsync(q => q.Id == id);
            return exist;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
