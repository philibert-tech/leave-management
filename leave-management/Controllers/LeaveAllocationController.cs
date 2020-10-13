using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize(Roles ="Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveTypeRepository _TypeRepo;
        private readonly ILeaveAllocationRepository _AllocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;


        public LeaveAllocationController(ILeaveTypeRepository TypeRepo, ILeaveAllocationRepository AllocationRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _TypeRepo = TypeRepo;
            _AllocationRepo = AllocationRepo;
            _mapper = mapper;
            _userManager = userManager;
        }
        // GET: LeaveAllocationController
        public async Task<ActionResult> Index()
        {
            var leavetypes =await _TypeRepo.FindAll();
            var mappedLeaveTypes = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leavetypes.ToList());
            var model = new CreateLeaveAllocationVm
            {
                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = 0
            };
            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType =await _TypeRepo.FindById(id);
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result.ToList();
         

            foreach (var emp in employees)
            {
                if (await _AllocationRepo.CheckAllocation(id, emp.Id)){
                    continue;                
                }

                if ((leaveType.Name == "Maternity Leave") && (emp.Gender == "Male"  || emp.Gender == "Others"))
                {
                    continue;
                }

                if ((leaveType.Name == "Paternity Leave") && (emp.Gender == "Female" || emp.Gender == "Others"))
                {
                    continue;
                }

                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year

                };

                var leaveallocation = _mapper.Map<LeaveAllocationVM, LeaveAllocation>(allocation);
              await  _AllocationRepo.Create(leaveallocation);
            }
            return RedirectToAction(nameof(Index));
        }

        public ActionResult ListEmployees()
        {
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;
            var model = _mapper.Map<List<EmployeeVM>>(employees);

            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee =_mapper.Map<EmployeeVM>( _userManager.FindByIdAsync(id).Result);

            var allocations = _mapper.Map<List<LeaveAllocationVM>>(await _AllocationRepo.GetLeaveAllocationsByEmployee(id));

            var model = new ViewAllocationVm
            {
                Employee = employee,
                LeaveAllocation = allocations

            };
            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveAllocation =await _AllocationRepo.FindById(id);
            var model = _mapper.Map<EditLeaveAllocationVm>(leaveAllocation);
            return View(model);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVm model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var record =await _AllocationRepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;

              var isSuccess =await  _AllocationRepo.Update(record);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something Went Wrong....");
                    return View(model);
                }

                return RedirectToAction(nameof(Details), new { id = model.EmployeeId});
            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong....");
                return View(model);
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
