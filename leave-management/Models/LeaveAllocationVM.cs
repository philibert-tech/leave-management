﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveAllocationVM
    {

        public int Id { get; set; }
        public int NumberOfDays { get; set; }
        public DateTime DateCreated { get; set; }
        public int Period { get; set; }

        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }

        public LeaveTypeVM LeaveType { get; set; }
        public int LeaveTypeId { get; set; }

    }

    public class CreateLeaveAllocationVm
    {
        public int NumberUpdated { get; set; }
        public List<LeaveTypeVM> LeaveTypes { get; set; }
    }

    public class ViewAllocationVm
    {
        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }
        public List<LeaveAllocationVM> LeaveAllocation { get; set; }

    }

    public class EditLeaveAllocationVm
    {
        public int Id { get; set; }
        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }
        public int NumberOfDays { get; set; }
        public LeaveTypeVM LeaveType { get; set; }

    }
}
