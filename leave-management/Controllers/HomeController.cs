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

namespace leave_management.Controllers
{
   
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

        public ActionResult MyLeave()
        {
            var EmployeeId = _userManager.GetUserAsync(User).Result.Id;
            var requests = _requestRepo.GetLeaveRequestByEmployee(EmployeeId).ToList();
            var allocations = _allocationRepo.GetLeaveAllocationsByEmployee(EmployeeId).ToList();

            var MappedAllocations = _mapper.Map<List<LeaveAllocationVM>>(allocations);
            var MappedRequest = _mapper.Map<List<LeaveRequestVM>>(requests);



            var model = new MyEmployeeLeaveVM
            {
                LeaveAllocations = MappedAllocations,
                LeaveRequests = MappedRequest
            };
            return View(model);
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
