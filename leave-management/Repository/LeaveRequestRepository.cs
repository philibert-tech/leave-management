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

        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public ICollection<LeaveRequest> FindAll()
        {
            return _db.LeaveRequests
                .Include(Q => Q.RequestingEmployee)
                .Include(Q => Q.ApprovedBy)
                .Include(Q => Q.LeaveType)
                .ToList();
        }

        public LeaveRequest FindById(int id)
        {
            var leaveHistory = _db.LeaveRequests
                .Include(Q => Q.RequestingEmployee)
                .Include(Q => Q.ApprovedBy)
                .Include(Q => Q.LeaveType)
                .FirstOrDefault(q => q.Id == id);
            return leaveHistory;
        }

        public ICollection<LeaveRequest> GetLeaveRequestByEmployee(string EmployeeId)
        {
            var leaveHistory =FindAll().Where(q => q.RequestingEmployeeId == EmployeeId).ToList();
            return leaveHistory;
        }

        public bool isExists(int id)
        {
            var exist = _db.LeaveRequests.Any(q => q.Id == id);
            return exist;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return Save();
        }
    }
}
