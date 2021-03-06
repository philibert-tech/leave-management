﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace leave_management.Controllers
{
    [Authorize(Roles ="Administrator")]
    
    public class LeaveTypesController : Controller
    {

        private readonly ILeaveTypeRepository _repo;
        private readonly ILeaveAllocationRepository _AlloRepo;
        private readonly ILeaveRequestRepository _requestRepo;
        private readonly IMapper _mapper;

        public LeaveTypesController(ILeaveTypeRepository repo, ILeaveAllocationRepository AlloRepo, ILeaveRequestRepository requestRepo, IMapper mapper)
        {
            _repo = repo;
            _AlloRepo = AlloRepo;
            _requestRepo = requestRepo;
            _mapper = mapper;
        }
        // GET: LeaveTypesController
        public async Task<ActionResult> Index()
        {
            var leavetypes =await _repo.FindAll();
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leavetypes.ToList());
            return View(model);
        }

        // GET: LeaveTypesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            if (!await _repo.isExists(id))
            {
                return NotFound();
            }

            var leavetype = await _repo.FindById(id);
            var model = _mapper.Map<LeaveType, LeaveTypeVM>(leavetype);


            return View(model);
        }

        // GET: LeaveTypesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeVM model)
         {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveType = _mapper.Map<LeaveTypeVM, LeaveType>(model);
                leaveType.DateCreated = DateTime.Now;
               var isSuccess = await  _repo.Create(leaveType);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something Went Wrong....");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong....");
                return View(model);
            }
        }

        // GET: LeaveTypesController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (!await _repo.isExists(id)){
                return NotFound();
            }

            var leavetype = await  _repo.FindById(id);
            var model = _mapper.Map<LeaveType, LeaveTypeVM>(leavetype);


            return View(model);
        }

        // POST: LeaveTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeVM model)
        {
            try
            {
                if (!ModelState.IsValid)
            {
                return View(model);
            }
            var leaveType = _mapper.Map<LeaveTypeVM, LeaveType>(model);

            var isSuccess =await _repo.Update(leaveType);

            if (!isSuccess)
            {
                ModelState.AddModelError("", "Something Went Wrong....");
                return View(model);
            }

            return RedirectToAction(nameof(Index));

            }
            catch
            {
                ModelState.AddModelError("", "Something Went Wrong....");
                return View(model);
            }
        }

        // GET: LeaveTypesController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            if (!await _repo.isExists(id))
            {
                return NotFound();
            }

            var leavetype =await  _repo.FindById(id);

            if (leavetype == null)
            {
                return NotFound();
            }

            var isSuccess =await _repo.Delete(leavetype);

            if (!isSuccess)
            {
                return BadRequest();
            }

            var allocation = await _AlloRepo.FindAll();

            if (allocation != null)
            {
            var AlloToDetete = allocation.ToList().Where(Q => Q.LeaveTypeId == id);

            foreach (var item in AlloToDetete)
            {
                await _AlloRepo.Delete(item);
            }
            }
           

            var request = await _requestRepo.FindAll();

            if (allocation != null)
            {
            var requestToDetete = request.ToList().Where(Q => Q.LeaveTypeId == id);

            foreach (var item in requestToDetete)
            {
                await _requestRepo.Delete(item);
            }
            }
                
            return RedirectToAction(nameof(Index));

        }

        // POST: LeaveTypesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveTypeVM model)
        {
            try
            {             
                var leavetype =await _repo.FindById(id);

                if(leavetype == null)
                {
                    return NotFound();
                }

                    var isSuccess =await _repo.Delete(leavetype);

                    if (!isSuccess)
                    {
                      
                        return View(model);
                    }

                    return RedirectToAction(nameof(Index));    
            }
            catch
            {

                return View(model);
            }
        }
    }
}
