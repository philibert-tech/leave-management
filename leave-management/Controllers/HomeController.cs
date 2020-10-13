using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using leave_management.Models;
using leave_management.Contracts;
using leave_management.Data;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace leave_management.Controllers
{
   [Authorize]
    public class HomeController : Controller
    { 
        private readonly ILeaveAllocationRepository  _allocationRepo;
        private readonly ILeaveRequestRepository _requestRepo;
        private readonly UserManager<Employee> _userManager;
        private readonly IMapper _mapper;

        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ILogger<HomeController> logger,
            ILeaveAllocationRepository allocationRepo,
            ILeaveRequestRepository requestRepo,
            UserManager<Employee> userManager,
            IMapper mapper
            )
        {
            _logger = logger;
            _allocationRepo = allocationRepo;
            _requestRepo = requestRepo;
            _userManager = userManager;
            _mapper = mapper;

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> MyLeave()
        {
            var EmployeeId = _userManager.GetUserAsync(User).Result.Id;
            var requests =await  _requestRepo.GetLeaveRequestByEmployee(EmployeeId);
            var allocations =await _allocationRepo.GetLeaveAllocationsByEmployee(EmployeeId);

            var MappedAllocations = _mapper.Map<List<LeaveAllocationVM>>(allocations.ToList());
            var MappedRequest = _mapper.Map<List<LeaveRequestVM>>(requests.ToList());



            var model = new MyEmployeeLeaveVM
            {
                LeaveAllocations = MappedAllocations,
                LeaveRequests = MappedRequest
            };
            return View(model);
        }

        public async Task<ActionResult> MakeAdmin(string id)
        {
            var employee =  _userManager.FindByIdAsync(id).Result;

         var result =   await _userManager.RemoveFromRoleAsync(employee, "Employee");

            if (result.Succeeded)
            {
                 var response = await _userManager.AddToRoleAsync(employee, "Administrator");

                if (response.Succeeded)
                {
                    var allocation = await _allocationRepo.GetLeaveAllocationsByEmployee(employee.Id);
                    var requests = await _requestRepo.GetLeaveRequestByEmployee(employee.Id);

                    if(allocation != null)
                    {
                        foreach(var item in allocation)
                        {
                            await _allocationRepo.Delete(item);
                        }
                    }

                    if (requests != null)
                    {
                        foreach (var item in requests)
                        {
                            await _requestRepo.Delete(item);
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Failed to assign Administrator role");
                    return RedirectToAction("Details", "LeaveAllocation", new { id = employee.Id});
                }


            }
            else
            {
                ModelState.AddModelError("", "Failed to remove Employee role");
                return RedirectToAction("Details", "LeaveAllocation", new { id = employee.Id });
            }



            return RedirectToAction("ListEmployees", "LeaveAllocation");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
