﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolbookApp.Data;
using SchoolbookApp.Models;

namespace SchoolbookApp.Controllers
{
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GradesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Grades
        public async Task<IActionResult> Index()
        {
            IdentityUser usr = await GetCurrentUserAsync();
            return View(await _context.Grade.Where(x => x.StudentId == usr.Id).ToListAsync());
        }

        public async Task<IActionResult> TeacherMain()
        {
            IdentityUser usr = await GetCurrentUserAsync();

            //всички часове, които води логнатият учител
            var subjectsByTeacher = _context.Subject.Where(x => x.TeacherId == usr.Id).ToList(); 

            //Изпраща предметите, по които учителят преподава, към фронтенда
            ViewBag.SubjectTypes = _context.Subject.Where(x => x.TeacherId == usr.Id) //всички часове, които води логнатият учител
                                                    .Select(x => x.SubjectType)       //избира само предмета, по който е даденият час
                                                    .Distinct()                       //за повтарящи се предмети (Математика на 1 А, Математика на 7 Б...)
                                                    .ToList();

            //Изпраща към фронтенда свързана таблица с кокнкретните предмети (във фронтенда - Item1), водени от учителя, и класовете (във фронтенда - Item2), на които ги води.
            ViewBag.SchoolClasses = _context.SchoolClass.ToList().Join(
                                        subjectsByTeacher,
                                        x => x.Id,
                                        y => y.SchoolClassId,
                                        (x, y) => (x, y)).ToList();
            return View("TeacherMain");
        }


        public async Task<IActionResult> TeacherClass()
        {
            return View("TeacherClass");
        }

        // GET: Grades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grade
                .FirstOrDefaultAsync(m => m.Id == id);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // GET: Grades/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Value,Basis,MyProperty,IsSemesterGrade,IsFinalGrade,Id,StudentId,DateTime")] Grade grade)
        {
            if (ModelState.IsValid)
            {
                IdentityDbContext _identityContext = _context as IdentityDbContext;
                grade.StudentId = _identityContext.Users.Where(x => x.Email == grade.StudentId).FirstOrDefault().Id;
                _context.Add(grade);
                //проверка за свързване на потребители чре many to many таблицата
                //_context.UserUser.Add(new UserUser() { StudentId = grade.StudentId, UserId= "a5b7c601-d6e3-4198-826f-c685aee3cdc4" });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grade);
        }

        // GET: Grades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grade.FindAsync(id);
            if (grade == null)
            {
                return NotFound();
            }
            return View(grade);
        }

        // POST: Grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Value,Basis,MyProperty,IsSemesterGrade,IsFinalGrade,Id,StudentId,DateTime")] Grade grade)
        {
            if (id != grade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(grade);
        }

        // GET: Grades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grade
                .FirstOrDefaultAsync(m => m.Id == id);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grade.FindAsync(id);
            _context.Grade.Remove(grade);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GradeExists(int id)
        {
            return _context.Grade.Any(e => e.Id == id);
        }

        private Task<IdentityUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

    }
}
