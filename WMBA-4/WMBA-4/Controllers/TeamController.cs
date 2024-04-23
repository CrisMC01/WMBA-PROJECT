﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using WMBA_4.CustomControllers;
using WMBA_4.Data;
using WMBA_4.Models;
using WMBA_4.Utilities;
using WMBA_4.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace WMBA_4.Controllers
{
    [Authorize]
    public class TeamController : ElephantController
    {
        private readonly WMBA_4_Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeamController(WMBA_4_Context context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: Team
        public async Task<IActionResult> Index(string SearchString, int? DivisionID, int? CoachID, bool isActive, bool isInactive, int? page, int? pageSizeID,
            string actionButton, string sortDirection = "asc", string sortField = "Team")
        {
            var userEmail = User.Identity.Name;

            List<int> teamIds = new List<int>();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (await _userManager.IsInRoleAsync(user, "Coach"))
            {

                teamIds = await GetCoachTeamsAsync(userEmail);
            }
            else if (await _userManager.IsInRoleAsync(user, "Scorekeeper"))
            {

                teamIds = await GetScorekeeperTeamsAsync(userEmail);
            }
            else
            {
                var convenorDivisions = await GetConvenorDivisionsAsync(userEmail);

                teamIds = await _context.Teams
                    .Where(t => convenorDivisions.Contains(t.Division.DivisionName))
                    .Select(t => t.ID)
                    .ToListAsync();
            }

            var teams = _context.Teams
                .Include(t => t.Division)
                .Include(t => t.TeamStaff).ThenInclude(ts => ts.Staff)
                .Where(t => teamIds.Contains(t.ID));

            PopulateDropDownLists();

            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            string[] sortOptions = new[] { "Team", "Division", "Coach" };

            //Filter
            if (DivisionID.HasValue)
            {
                teams = teams.Where(p => p.DivisionID == DivisionID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                teams = teams.Where(p => p.Name.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (isActive == true)
            {
                teams = teams.Where(p => p.Status == true);
                numberFilters++;
            }
            if (isInactive == true)
            {
                teams = teams.Where(p => p.Status == false);
                numberFilters++;
            }
            if (numberFilters != 0)
            {
                ViewData["Filtering"] = " btn-danger";

                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
            }

            teams = teams.OrderByDescending(p => p.Status)
                    .ThenBy(p => p.Name)
                    .ThenBy(p => p.Division.DivisionName);

            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (!String.IsNullOrEmpty(actionButton))
                {
                    if (sortOptions.Contains(actionButton))
                    {
                        if (actionButton == sortField)
                        {
                            sortDirection = sortDirection == "asc" ? "desc" : "asc";
                        }
                        sortField = actionButton;
                    }
                }
                if (sortField == "Team")
                {
                    if (sortDirection == "asc")
                    {
                        teams = teams
                            .OrderBy(p => p.Name);
                    }
                    else
                    {
                        teams = teams
                            .OrderByDescending(p => p.Name);
                    }
                }
                else if (sortField == "Division")
                {
                    if (sortDirection == "asc")
                    {
                        teams = teams
                            .OrderBy(p => p.Division);
                    }
                    else
                    {
                        teams = teams
                            .OrderByDescending(p => p.Division);
                    }
                }
            }

            //Filter by DivisionName - TeamName
            ViewBag.TeamID = new SelectList(_context.Teams
                .Include(t => t.Division)
                .OrderBy(t => t.Division.ID)
                .ThenBy(t => t.Name)
                .Select(t => new {
                    t.ID,
                    TeamName = t.Division.DivisionName + " - " + t.Name
                }), "ID", "TeamName");

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName");

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Team>.CreateAsync(teams.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData); ;
        }

        // GET: Player/Activate/5
        public async Task<IActionResult> Activate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // Set the player's status to active
            team.Status = true;
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Team/Details/5
        public IActionResult Details(int id)
        {
            var team = _context.Teams
     .Include(t => t.Division)
     .Include(t => t.TeamStaff).ThenInclude(ts => ts.Staff).ThenInclude(s => s.Roles)
     .Include(t => t.TeamGames)
         .ThenInclude(tg => tg.Game)
             .ThenInclude(g => g.TeamGames)
                 .ThenInclude(tg => tg.Team)
     .FirstOrDefault(t => t.ID == id && t.TeamGames.Any(tg => tg.Game.Status == true));

            var players = from p in _context.Players
            .Include(p => p.Team)
            .Where(s => s.Status == true && s.TeamID == id)
            .OrderBy(p => p.LastName)
            .Where(p => p.Status == true)
            .AsNoTracking()
                          select p;

            if (team == null)
            {
                team = _context.Teams
                    .Include(t => t.Division)
                    .Include(t => t.TeamStaff).ThenInclude(ts => ts.Staff).ThenInclude(s => s.Roles)
                    .FirstOrDefault(t => t.ID == id);

                if (team == null)
                {
                    return NotFound();
                }
            }

            var opponentTeams = new Dictionary<int, string>();

            foreach (var teamGame in team.TeamGames)
            {
                if (teamGame.IsHomeTeam)
                {
                    var opponentTeam = teamGame.Game.TeamGames
                        .FirstOrDefault(tg => tg.IsVisitorTeam)?.Team.Name;

                    opponentTeams[teamGame.GameID] = opponentTeam ?? "Unknown Team";
                }
                else if (teamGame.IsVisitorTeam)
                {
                    var opponentTeam = teamGame.Game.TeamGames
                        .FirstOrDefault(tg => tg.IsHomeTeam)?.Team.Name;

                    opponentTeams[teamGame.GameID] = opponentTeam ?? "Unknown Team";
                }
            }
            // Get the coach of the team
            var coach = team.TeamStaff.FirstOrDefault(ts => ts.Staff.Roles.Description == "Coach")?.Staff;
            var scoreK = team.TeamStaff.FirstOrDefault(ts => ts.Staff.Roles.Description == "Scorekeeper")?.Staff;

            // Pass the coach to the view 
            ViewBag.Coach = coach;
            ViewBag.Scorekeeper = scoreK;

            ViewBag.OpponentTeams = opponentTeams;
            ViewData["Players"] = players.ToList();

            return View(team);
        }

        // GET: Team/Create
        public async Task<IActionResult> CreateAsync()
        {
         

         
            var convenorEmail = User.Identity.Name;
            //var user = await _userManager.FindByEmailAsync(convenorEmail);
            List<string> allowedDivisions = await GetConvenorDivisionsAsync(convenorEmail);
            ViewData["DivisionID"] = new SelectList(_context.Divisions.Where(d => allowedDivisions.Contains(d.DivisionName)), "ID", "DivisionName"); ;
            // Filter staff members based on their roles
            var scorekeeperStaff = _context.Staff.Where(s => s.Roles.Description == "Scorekeeper").ToList();
            var coachStaff = _context.Staff.Where(s => s.Roles.Description == "Coach").ToList();

            // Create select items for scorekeepers and coaches
            var scorekeeperSelectItems = scorekeeperStaff.Select(s => new SelectListItem
            {
                Value = s.ID.ToString(),
                Text = $"{s.FirstName} {s.LastName}"
            });

            var coachSelectItems = coachStaff.Select(s => new SelectListItem
            {
                Value = s.ID.ToString(),
                Text = $"{s.FirstName} {s.LastName}"
            });

            
            // Pass select items and selected staff IDs to the view
            ViewBag.ScorekeeperIds = new MultiSelectList(scorekeeperSelectItems, "Value", "Text");
            ViewBag.CoachIds = new MultiSelectList(coachSelectItems, "Value", "Text");

            return View();
        }

        // POST: Team/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("Team/Create/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,DivisionID")] Team team,  List<int> ScorekeeperIds, List<int> CoachIds)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(team);
                    await _context.SaveChangesAsync();

                    foreach (var staffId in ScorekeeperIds)
                    {
                        var teamStaffsck = new TeamStaff { TeamID = team.ID, StaffID = staffId };
                        _context.Add(teamStaffsck);
                    }
                    foreach (var staffId in CoachIds)
                    {
                        var teamStaffcoach = new TeamStaff { TeamID = team.ID, StaffID = staffId };
                        _context.Add(teamStaffcoach);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { team.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Team name", "Unable to save changes. Remember, you cannot have duplicate Team Names.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName", team.DivisionID);
            return View(team);
        }

        // GET: Team/Edit/5
        [HttpGet]
        [Route("Team/Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.TeamStaff).ThenInclude(ts => ts.Staff)
                .FirstOrDefaultAsync(t => t.ID == id);

            if (team == null)
            {
                return NotFound();
            }

            var convenorEmail = User.Identity.Name;
            List<string> allowedDivisions = await GetConvenorDivisionsAsync(convenorEmail);
            ViewData["DivisionID"] = new SelectList(_context.Divisions.Where(d => allowedDivisions.Contains(d.DivisionName)), "ID", "DivisionName", team.DivisionID);

            // Filter staff members based on their roles
            var scorekeeperStaff = _context.Staff.Where(s => s.Roles.Description == "Scorekeeper").ToList();
            var coachStaff = _context.Staff.Where(s => s.Roles.Description == "Coach").ToList();

            // Create select items for scorekeepers and coaches
            var scorekeeperSelectItems = scorekeeperStaff.Select(s => new SelectListItem
            {
                Value = s.ID.ToString(),
                Text = $"{s.FirstName} {s.LastName}"
            });

            var coachSelectItems = coachStaff.Select(s => new SelectListItem
            {
                Value = s.ID.ToString(),
                Text = $"{s.FirstName} {s.LastName}"
            });

            // Get the selected staff IDs
            var selectedScorekeeperIds = team.TeamStaff
                .Where(ts => ts.Staff != null && ts.Staff.Roles != null && ts.Staff.Roles.Description == "Scorekeeper")
                .Select(ts => ts.StaffID.ToString())
                .ToList();

            var selectedCoachIds = team.TeamStaff
                .Where(ts => ts.Staff != null && ts.Staff.Roles != null && ts.Staff.Roles.Description == "Coach")
                .Select(ts => ts.StaffID.ToString())
                .ToList();

            // Pass select items and selected staff IDs to the view
            ViewBag.ScorekeeperIds = new MultiSelectList(scorekeeperSelectItems, "Value", "Text", selectedScorekeeperIds);
            ViewBag.CoachIds = new MultiSelectList(coachSelectItems, "Value", "Text", selectedCoachIds);

            return View(team);
        }


        // POST: Team/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("Team/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,DivisionID")] Team team, List<int> ScorekeeperIds, List<int> CoachIds)
        {
            if (id != team.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var existingStaffMembers = _context.TeamStaff.Where(ts => ts.TeamID == id);
                    _context.TeamStaff.RemoveRange(existingStaffMembers);


                    foreach (var staffId in ScorekeeperIds)
                    {
                        var teamStaffsck = new TeamStaff { TeamID = id, StaffID = staffId };
                        _context.Add(teamStaffsck);
                    }
                    foreach (var staffId in CoachIds)
                    {
                        var teamStaffcoach = new TeamStaff { TeamID = id, StaffID = staffId };
                        _context.Add(teamStaffcoach);
                    }
                    _context.Update(team);
                    await _context.SaveChangesAsync();

                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Team Names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
                return RedirectToAction("Details", new { team.ID });
            }
            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName", team.DivisionID);
            return View("Details", new List<WMBA_4.Models.Team> { team });
        }
        

        // GET: Team/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.Division)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Team/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Teams == null)
            {
                return Problem("Entity set 'WMBA_4_Context.Teams'  is null.");
            }
            var team = await _context.Teams.FindAsync(id);



            if (team != null)
            {
                // Verify if the team has games scheduled
                bool hasScheduledGames = _context.TeamGame
                    .Any(tg => tg.TeamID == id && tg.Game.Date >= DateTime.Today);

                // Verify if the team has Players assigned
                bool hasplayers = _context.Players
                    .Any(tg => tg.TeamID == id);

                if (hasScheduledGames)
                {
                    //Display an error message indicating that the team has games scheduled
                    ModelState.AddModelError(string.Empty, "You cannot delete the team because it has scheduled games for today or later.");
                    return View(nameof(Delete), team);
                }
                else if (hasplayers)
                {
                    //Display an error message indicating that the team has players assigned
                    ModelState.AddModelError(string.Empty, "You cannot delete the team because it has players assigned.");
                    return View(nameof(Delete), team);

                }
                else
                {
                    team.Status = false;
                    _context.Teams.Update(team);
                    await _context.SaveChangesAsync();
                }

            }

            return Redirect(ViewData["returnURL"].ToString());
        }

        /// <summary>
        /// This is for importing Teams
        /// </summary>
        /// <returns></returns>
        public ActionResult GoToImportPlayers()
        {
            return View("ImportTeam");
        }
        [Authorize(Roles = "Admin,RookieConvenor, IntermediateConvenor, SeniorConvenor")]

        [HttpPost]
        public async Task<IActionResult> ImportTeam(IFormFile theExcel)
        {
            string feedBack = string.Empty;
            string errorInvalid = "Invalid Excel file. Please save your CSV document as Excel file and try again.";
            string errorCSV = "Error: That file is not an Excel spreadsheet or CSV file";
            string errorFile = "Error: Problem with the file";
            string errorNofile = "Error: No file uploaded";
            if (theExcel != null)
            {
                string mimeType = theExcel.ContentType;
                long fileLength = theExcel.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet"))
                    {
                        ExcelPackage excel;
                        try
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await theExcel.CopyToAsync(memoryStream);
                                excel = new ExcelPackage(memoryStream);
                            }
                            var workSheet = excel.Workbook.Worksheets[0];

                            // Call data processing method
                            feedBack = await ProcessImportedData(workSheet);

                        }
                        catch
                        {

                            feedBack = "<span class=\"text-danger\">" + errorInvalid + "</span>";

                        }

                    }
                    else if (mimeType.Contains("csv"))
                    {
                        var format = new ExcelTextFormat();
                        format.Delimiter = ',';
                        bool firstRowIsHeader = true;

                        using var reader = new System.IO.StreamReader(theExcel.OpenReadStream());

                        using ExcelPackage package = new ExcelPackage();
                        var result = reader.ReadToEnd();
                        ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("Imported Report Data");

                        workSheet.Cells["A1"].LoadFromText(result, format, TableStyles.None, firstRowIsHeader);

                        // Call data processing method
                        feedBack = await ProcessImportedData(workSheet);
                    }
                    else
                    {
                        feedBack = "<span class=\"text-danger\">" + errorCSV + "</span>";
                    }
                }
                else
                {
                    feedBack = "<span class=\"text-danger\">" + errorFile + "</span>";
                }
            }
            else
            {
                feedBack = "<span class=\"text-danger\">" + errorNofile + "</span>";
            }

            TempData["Feedback"] = feedBack;


            return View();

        }

        private async Task<string> ProcessImportedData(ExcelWorksheet workSheet)
        {
            string feedBack = string.Empty;
            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

            if (workSheet.Cells[1, 2].Text == "First Name" &&
                workSheet.Cells[1, 3].Text == "Last Name" &&
                workSheet.Cells[1, 4].Text == "Member ID" &&
                workSheet.Cells[1, 6].Text == "Division" &&
                workSheet.Cells[1, 7].Text == "Club" &&
                workSheet.Cells[1, 8].Text == "Team")
            {
                int successCount = 0;
                int errorCount = 0;

                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        Player pl = new Player();
                        try
                        {
                            pl.FirstName = workSheet.Cells[row, 2].Text;
                            pl.LastName = workSheet.Cells[row, 3].Text;
                            pl.MemberID = workSheet.Cells[row, 4].Text;

                            // Validación para la columna "Season"
                            string season = workSheet.Cells[row, 5].Text;
                            var currentSeason = _context.Seasons.FirstOrDefault();
                            int currentYear = int.Parse(currentSeason.SeasonCode);

                            //int currentYear = DateTime.Now.Year;
                            if (season != currentYear.ToString())
                            {
                                transaction.Rollback();
                                errorCount++;
                                feedBack += "<span class=\"text-danger\">" + "Error: Record " + pl.FirstName + " " + pl.LastName + " was rejected because the Season value is not the current year."
                                        + "</span>" + "<br />";
                                continue; // Salta al siguiente registro
                            }
                            Player existingPlayer = _context.Players.FirstOrDefault(p => p.MemberID == pl.MemberID);
                            if (existingPlayer == null)
                            {
                                Team t = new Team();
                                //For Divisions
                                string DivisonName = workSheet.Cells[row, 6].Text;
                                Division existingDiv = _context.Divisions.FirstOrDefault(t => t.DivisionName == DivisonName);
                                if (existingDiv == null)
                                {
                                    Division newDivision = new Division { DivisionName = DivisonName, Status = true, ClubID = 1 };
                                    _context.Divisions.Add(newDivision);
                                    await _context.SaveChangesAsync();
                                    t.DivisionID = newDivision.ID;
                                }
                                else
                                {
                                    t.DivisionID = existingDiv.ID;
                                }
                                //For Teams
                                string teamNameFirst = workSheet.Cells[row, 8].Text;
                                string teamName = Regex.Replace(teamNameFirst, @"^\d+[A-Za-z]*\s*", "");
                                Team existingTeam = _context.Teams.FirstOrDefault(t => t.Name == teamName);
                                if (existingTeam == null)
                                {
                                    Team newTeam = newTeam = new Team { Name = teamName, DivisionID = t.DivisionID };
                                    _context.Teams.Add(newTeam);

                                    await _context.SaveChangesAsync();
                                    pl.TeamID = newTeam.ID;
                                }
                                else
                                {
                                    pl.TeamID = existingTeam.ID;
                                }
                                _context.Players.Add(pl);
                                await _context.SaveChangesAsync();
                                successCount++;
                                transaction.Commit();
                            }
                            else
                            {
                                // El jugador ya existe, por lo que no lo agregamos
                                transaction.Rollback();
                                errorCount++;
                                feedBack += "<span class=\"text-danger\">" + "Error: Record " + pl.FirstName + " " + pl.LastName + " was rejected as a duplicate."
                                        + "</span>" + "<br />";
                            }
                        }
                        catch (DbUpdateException dex)
                        {
                            transaction.Rollback();
                            errorCount++;
                            if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                            {
                                feedBack += "<span class=\"text-danger\">" + "Error: Record " + pl.FirstName + " " + pl.LastName + " was rejected as a duplicate."
                                        + "</span>" + "<br />";
                            }
                            else
                            {
                                feedBack += "<span class=\"text-danger\">" + "Error: Record " + pl.FirstName + " " + pl.LastName + " caused an error."
                                        + "</span>" + "<br />";
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            errorCount++;
                            if (ex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                            {
                                feedBack += "<span class=\"text-danger\">" + "<span class=\"text-danger\">" + "Error: Record " + pl.FirstName + pl.LastName + " was rejected because the Team name is duplicated."
                                        + "</span>" + "<br />";
                            }
                            else
                            {
                                feedBack += "<span class=\"text-danger\">" + "Error: Record " + pl.FirstName + pl.LastName + " caused and error."
                                        + "</span>" + "<br />";
                            }
                        }
                    }
                }
                foreach (var entry in _context.ChangeTracker.Entries<Player>().Where(e => e.State == EntityState.Added))
                {
                    entry.State = EntityState.Detached;
                }
                if (successCount > 0)
                {
                    feedBack += "Your file has been successfully imported and saved." + "<br/>";
                    feedBack += "Result: " + "<span class=\"text-bold\">" + (successCount + errorCount).ToString() + "</span>" +
                " Records with " + "<span class=\"text-bold text-primary\">" + successCount.ToString() + "</span>" + " inserted and " +
                "<span class=\"text-bold text-danger\">" + errorCount.ToString() + "</span>" + " rejected";
                }
                else
                {
                    feedBack += "Result: " + "<span class=\"text-bold mt-2\">" + (successCount + errorCount).ToString() + "</span>" +
                " Records with " + "<span class=\"text-bold text-primary\">" + successCount.ToString() + "</span>" + " inserted and " +
                "<span class=\"text-bold text-danger\">" + errorCount.ToString() + "</span>" + " rejected";
                }
            }
            else
            {
                feedBack = "Error: You may have selected the wrong file to upload.<br /> Remember, you must have the headings 'First Name','Last Name','Member ID','Season','Division' and 'Team' in the first two cells of the first row.";
            }
            return feedBack;
        }

        private SelectList DivisionList(int? selectedId)
        {
            return new SelectList(_context
                .Divisions
                .OrderBy(m => m.DivisionName), "ID", "DivisionName", selectedId);
        }

        private void PopulateDropDownLists(Team team = null)
        {
            ViewData["DivisionID"] = DivisionList(team?.DivisionID);

        }
        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.ID == id);
        }

        private async Task<List<string>> GetConvenorDivisionsAsync(string convenorEmail)
        {
            var user = await _userManager.FindByEmailAsync(convenorEmail);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                switch (roles.FirstOrDefault())
                {
                    case "RookieConvenor":
                        return new List<string> { "9U" };
                    case "IntermediateConvenor":
                        return new List<string> { "11U", "13U" };
                    case "SeniorConvenor":
                        return new List<string> { "15U", "18U" };
                    default:
                        return new List<string> { "9U", "11U", "13U", "15U", "18U" };
                }
            }
            return new List<string>();
        }

        private async Task<List<int>> GetCoachTeamsAsync(string coachEmail)
        {
            var user = await _userManager.FindByEmailAsync(coachEmail);
            if (user != null)
            {

                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == coachEmail);
                if (staff != null)
                {

                    var teamIds = await _context.TeamStaff
                        .Where(ts => ts.StaffID == staff.ID)
                        .Select(ts => ts.TeamID)
                        .ToListAsync();

                    return teamIds;
                }
            }
            return new List<int>();
        }

        private async Task<List<int>> GetScorekeeperTeamsAsync(string coachEmail)
        {
            var user = await _userManager.FindByEmailAsync(coachEmail);
            if (user != null)
            {

                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == coachEmail);
                if (staff != null)
                {

                    var teamIds = await _context.TeamStaff
                        .Where(ts => ts.StaffID == staff.ID)
                        .Select(ts => ts.TeamID)
                        .ToListAsync();

                    return teamIds;
                }
            }
            return new List<int>();
        }



    }

}