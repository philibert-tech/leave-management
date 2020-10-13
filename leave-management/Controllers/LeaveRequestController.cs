using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(
            ILeaveRequestRepository leaveRequestRepo,
            ILeaveTypeRepository leaveTypeRepo,
            ILeaveAllocationRepository leaveAllocationRepo,
            IMapper mapper,
            UserManager<Employee> userManager)
        {
            _leaveRequestRepo = leaveRequestRepo;
            _mapper = mapper;
            _userManager = userManager;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
        }

        [Authorize(Roles="Administrator")]
        public async Task<ActionResult> Index()
        {
            var leaveRequests =await  _leaveRequestRepo.FindAll();
            var leaveRequestModels = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);

            var model = new AdminLeaveRequestViewVm
            {
                TotalRequest = leaveRequestModels.Count,
                ApprovedRequest = leaveRequestModels.Count(q=>q.Approved == true),
                PendingRequest  = leaveRequestModels.Count(q => q.Approved == null),
                RejectedRequest = leaveRequestModels.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestModels
            };
            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest =await _leaveRequestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);
            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
             var leaveRequest =await _leaveRequestRepo.FindById(id);
            leaveRequest.Approved = true;
            leaveRequest.ApprovedById = _userManager.GetUserAsync(User).Result.Id;
            leaveRequest.DateActioned = DateTime.Now;

                var isSuccess =await _leaveRequestRepo.Update(leaveRequest);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something Went Wrong with the registration....");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var employeeAllocation =await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);
                    var InitialNumberOfDays = employeeAllocation.NumberOfDays;
                    var DaysRequested = (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays;
                    var DaysRemaining = InitialNumberOfDays - DaysRequested;

                    employeeAllocation.NumberOfDays = DaysRemaining;


                    var IsSuccess =await _leaveAllocationRepo.Update(employeeAllocation);

                    if (!isSuccess)
                    {
                        ModelState.AddModelError("", "Something Went Wrong with the registration....");
                        return RedirectToAction(nameof(Index));
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong with the registration....");
                return RedirectToAction(nameof(Index));

            }
           



        }
        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var leaveRequest =await _leaveRequestRepo.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = _userManager.GetUserAsync(User).Result.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var isSuccess =await _leaveRequestRepo.Update(leaveRequest);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something Went Wrong with the registration....");
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong with the registration....");
                return RedirectToAction(nameof(Index));

            }
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var userId = _userManager.GetUserAsync(User).Result.Id;
            var EmployeeAllocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployee(userId);

            var list = new List<LeaveType>();
            foreach (var item in EmployeeAllocation.ToList())
            {
                list.Add(item.LeaveType);
            }

            var leaveTypeItems = list.Select(q => new SelectListItem {
            Text = q.Name,
            Value = q.Id.ToString()
            });



            var model = new CreateLeaveRequestVm
            {
                LeaveTypes = leaveTypeItems
            };

            return View(model);
        } 

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVm model)
        {                    
            try
            {
                var StartDate = Convert.ToDateTime(model.StartDate);
                var EndDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes =await _leaveTypeRepo.FindAll();

                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });

                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if ((StartDate < DateTime.Now)|| (EndDate < DateTime.Now))
                {
                    ModelState.AddModelError("", "Start Date or End Date cannot be earlier than today's date");
                    return View(model);
                }

                if (DateTime.Compare(StartDate, EndDate) > 0 ){
                    ModelState.AddModelError("", "Start date cannot be greater than End date");
                    return View(model);
                }

                var employee = _userManager.GetUserAsync(User).Result;
                var allocation =await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                int DaysRequested = (int)(EndDate.Date - StartDate.Date).TotalDays;

                if (DaysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "Days Requested Exceeds number of days available");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    LeaveTypeId = model.LeaveTypeId,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    RequestsComments = model.RequestsComments
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
             var isSuccess =await  _leaveRequestRepo.Create(leaveRequest);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something Went Wrong with the registration....");
                    return View();
                }



                return RedirectToAction(nameof(Index), "Home");
            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong....");
                return View();
            }
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            var model =await _leaveRequestRepo.FindById(id);

            model.Cancelled = true;

           await _leaveRequestRepo.Update(model);

            var NOofDaysRequested = (int)(model.EndDate.Date - model.StartDate.Date).TotalDays;
            var allocation =await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(model.RequestingEmployeeId, model.LeaveTypeId);

            allocation.NumberOfDays += NOofDaysRequested;

           await _leaveAllocationRepo.Update(allocation);

            return RedirectToAction("MyLeave", "Home");
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
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
