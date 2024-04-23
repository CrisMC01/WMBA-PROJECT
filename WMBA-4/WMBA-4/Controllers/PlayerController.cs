﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Numerics;
using WMBA_4.CustomControllers;
using WMBA_4.Data;
using WMBA_4.Models;
using WMBA_4.Utilities;
using WMBA_4.ViewModels;


namespace WMBA_4.Controllers
{
    [Authorize]
    public class PlayerController : ElephantController
    {
        private readonly WMBA_4_Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PlayerController(WMBA_4_Context context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Player
        public async Task<IActionResult> Index(int? id, string SearchString, int? TeamID, int? DivisionID, bool isActive, bool isInactive, int? page, int? pageSizeID,
            string actionButton, string sortDirection = "asc", string sortField = "Player")
        {
            var wMBA_4_Context = _context.Players.Include(p => p.Team);


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
                var convenorTeams = await GetConvenorTeamIdsAsync(userEmail);
                teamIds = convenorTeams;
            }

            var players = _context.Players
               .Include(p => p.Team)
               .ThenInclude(t => t.Division)
               .OrderBy(s => s.Status == true)
               .Where(p => teamIds.Contains(p.TeamID));



            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;



            //sorting sortoption array
            string[] sortOptions = new[] { "Player", "Team", "Division" };

            //filter
            if (TeamID.HasValue)
            {
                players = players.Where(p => p.TeamID == TeamID);
                numberFilters++;
            }
            if (DivisionID.HasValue)
            {
                players = players.Where(p => p.Team.DivisionID == DivisionID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                players = players.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (isActive == true)
            {
                players = players.Where(p => p.Status == true);
                numberFilters++;
            }
            if (isInactive == true)
            {
                players = players.Where(p => p.Status == false);
                numberFilters++;
            }
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                //@ViewData["ShowFilter"] = " show";
            }

            players = players
                .OrderByDescending(p => p.Status) // send all false status players to the back in the list 
                .ThenBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.Team.Name)
                .ThenBy(p => p.Team.Division.DivisionName);

            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start
                         //sorting
                if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
                {
                    if (sortOptions.Contains(actionButton))//Change of sort is requested
                    {
                        if (actionButton == sortField) //Reverse order on same field
                        {
                            sortDirection = sortDirection == "asc" ? "desc" : "asc";
                        }
                        sortField = actionButton;//Sort by the button clicked
                    }
                }
            }
            if (sortField == "Player")
            {
                if (sortDirection == "asc")
                {
                    players = players
                        .OrderBy(p => p.FirstName)
                        .ThenBy(p => p.LastName);
                }
                else
                {
                    players = players
                        .OrderByDescending(p => p.FirstName)
                        .ThenByDescending(p => p.LastName);
                }
            }
            else if (sortField == "Team")
            {
                if (sortDirection == "asc")
                {
                    players = players
                        .OrderBy(p => p.Team.Name);
                }
                else
                {
                    players = players
                        .OrderByDescending(p => p.Team.Name);
                }
            }
            else if (sortField == "Division")
            {
                if (sortDirection == "asc")
                {
                    players = players
                        .OrderBy(p => p.Team.Division.DivisionName);
                }
                else
                {
                    players = players
                        .OrderByDescending(p => p.Team.Division.DivisionName);
                }
            }

            //Filter by DivisionName - TeamName
            ViewBag.TeamID = new SelectList(_context.Teams
                .Include(t => t.Division)
                .OrderBy(t => t.Division.ID)
                .ThenBy(t => t.Name)
                .Select(t => new
                {
                    t.ID,
                    TeamName = t.Division.DivisionName + " - " + t.Name
                }), "ID", "TeamName");

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //ViewData["TeamID"] = new SelectList(_context.Teams.OrderBy(t=>t.Name), "ID", "Name");
            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName");

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Player>.CreateAsync(players.AsNoTracking(), page ?? 1, pageSize);


            return View(pagedData); ;
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Team)
                    .ThenInclude(t => t.Division)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (player == null)
            {
                return NotFound();
            }

            // 선수가 속한 팀의 코치를 가져옵니다.
            var coach = await _context.TeamStaff
                .Where(ts => ts.TeamID == player.TeamID && ts.Staff.Roles.Description == "Coach")
                .Select(ts => ts.Staff.FullName)
                .FirstOrDefaultAsync();

            var team = await _context.Teams
                .Include(t => t.Division)
                .Include(t => t.TeamStaff).ThenInclude(ts => ts.Staff).ThenInclude(s => s.Roles)
                .Include(t => t.TeamGames)
                    .ThenInclude(tg => tg.Game)
                        .ThenInclude(g => g.TeamGames)
                            .ThenInclude(tg => tg.Team)
                .FirstOrDefaultAsync(t => t.ID == player.TeamID);

            // 선수가 속한 팀의 모든 게임 정보를 가져옵니다.
            var teamGames = await _context.TeamGame
                .Where(tg => tg.TeamID == player.TeamID)
                .Include(tg => tg.Game) // Game 엔터티 로드
                    .ThenInclude(g => g.Location) // Location 속성 로드
                .Include(tg => tg.Game)
                    .ThenInclude(g => g.GameType)
                .Include(tg => tg.Game)
                    .ThenInclude(g => g.Season)
                .Include(tg => tg.Game)
                    .ThenInclude(g => g.GameLineUps)
                        .ThenInclude(gl => gl.ScoresPlayer)
                .ToListAsync();

            // 각 게임 라인업에 해당하는 scoreplayer 정보를 가져옵니다.
            foreach (var game in teamGames.Select(tg => tg.Game).OrderBy(g => g.Date))
            {
                foreach (var lineup in game.GameLineUps)
                {
                    await _context.Entry(lineup)
                        .Collection(gl => gl.ScoresPlayer)
                        .LoadAsync();
                }
            }

            int totalGamesPlayed = teamGames.Count(tg => tg.Game.GameLineUps.Any(gl => gl.PlayerID == id && gl.ScoresPlayer.Any()));
            ViewData["TotalGamesPlayed"] = totalGamesPlayed;
            ViewData["TeamGames"] = teamGames;
            ViewData["Coach"] = coach;

            return View(player);
        }


        // GET: Player/Activate/5
        public async Task<IActionResult> Activate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            // Set the player's status to active
            player.Status = true;
            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            var teamsByDivision = _context.Teams.OrderBy(t => t.DivisionID).ThenBy(t => t.Name).ToList();
            var teamSelectList = new List<SelectListItem>();

            int? currentDivisionID = null;
            foreach (var team in teamsByDivision)
            {
                if (team.DivisionID != currentDivisionID)
                {
                    // Add separator for new division
                    var division = _context.Divisions.Find(team.DivisionID);
                    teamSelectList.Add(new SelectListItem { Disabled = true, Text = $"-------------------------{division.DivisionName}-------------------------" });
                    currentDivisionID = team.DivisionID;
                }

                // Add team to select list
                teamSelectList.Add(new SelectListItem { Value = team.ID.ToString(), Text = team.Name });
            }
            ViewData["TeamID"] = teamSelectList;
            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName");

            return View();
        }


        // POST: Player/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MemberID,FirstName,LastName,JerseyNumber,Status,TeamID")] Player player)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (IsJerseyNumberDuplicate(player))
                    {
                        ModelState.AddModelError("JerseyNumber", "The jersey number should be unique within the team. Please choose a different jersey number.");
                    }
                    else
                    {
                        _context.Add(player);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Details", new { player.ID });
                    }
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Players.MemberID"))
                {
                    ModelState.AddModelError("MemberID", "The entered member ID is already in use. Please provide a different member ID.");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while saving. Retry a few times, and if the issue persists, seek assistance from your system administrator.");
                }
            }

            ViewData["TeamID"] = new SelectList(_context.Teams, "ID", "Name", player.TeamID);

            return View(player);
        }

        // GET: Player/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Team)
                    .ThenInclude(d => d.Division)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (player == null)
            {
                return NotFound();
            }

            var currentDivisionId = player.Team.DivisionID;

            // 현재 선수가 속한 디비전과 그 위 아래 디비전의 ID
            var divisionIds = new List<int?> { currentDivisionId, currentDivisionId + 1, currentDivisionId - 1 }; // 위 아래 디비전도 포함

            // 해당 디비전과 그 위 아래 디비전의 팀들을 가져옴
            var teams = await _context.Teams
                .Include(t => t.Division)
                .Where(t => divisionIds.Contains(t.DivisionID))
                .OrderBy(t => t.DivisionID)
                .ThenBy(t => t.Division.DivisionName)
                .ThenBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();

            var teamSelectList = new List<SelectListItem>();
            string currentDivisionName = null;

            foreach (var team in teams)
            {
                if (team.Division.DivisionName != currentDivisionName)
                {
                    // Add separator for new division
                    teamSelectList.Add(new SelectListItem { Disabled = true, Text = $"-------------------------{team.Division.DivisionName}-------------------------" });
                    currentDivisionName = team.Division.DivisionName;
                }

                // Add team to select list
                teamSelectList.Add(new SelectListItem { Value = team.ID.ToString(), Text = team.Name });
            }

            ViewData["TeamID"] = new SelectList(teamSelectList, "Value", "Text", player.TeamID);
            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName", player.Team.DivisionID);

            return View(player);
        }



        // POST: Player/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var playerToUpdate = await _context.Players
                .Include(p => p.Team)
                .ThenInclude(p => p.Division)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (playerToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Player>(playerToUpdate, "", p => p.MemberID, p => p.FirstName, p => p.LastName,
                p => p.JerseyNumber, p => p.Status, p => p.TeamID))
            {
                try
                {
                    //null will acceptable for duplicate 'null'
                    if (IsJerseyNumberDuplicate(playerToUpdate))
                    {
                        ModelState.AddModelError("JerseyNumber", "The jersey number should be unique within the team. Please choose a different jersey number.");
                    }
                    else
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Details", new { playerToUpdate.ID });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(playerToUpdate.ID))
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
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Players.MemberID"))
                    {
                        ModelState.AddModelError("MemberID", "The entered member ID is already in use. Please provide a different member ID.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while saving. Retry a few times, and if the issue persists, seek assistance from your system administrator.");
                    }
                }
            }

            var playerTeamDivisionId = playerToUpdate.Team.DivisionID;
            var teamsInBiggerDivision = await _context.Teams
                .Include(t => t.Division)
                .Where(t => t.DivisionID == playerTeamDivisionId - 1 || t.DivisionID == playerTeamDivisionId + 1 || t.DivisionID == playerTeamDivisionId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewData["TeamID"] = new SelectList(teamsInBiggerDivision, "ID", "Name", playerToUpdate.TeamID);
            ViewData["DivisionID"] = new SelectList(_context.Divisions, "ID", "DivisionName", playerToUpdate.Team.DivisionID);

            return View(playerToUpdate);
        }
        [HttpGet]
        public async Task<IActionResult> GetDivisionName(int teamId)
        {
            var team = await _context.Teams.Include(t => t.Division).FirstOrDefaultAsync(t => t.ID == teamId);
            if (team != null)
            {
                return Json(team.Division.DivisionName);
            }
            return Json(null);
        }

        // GET: Player/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Players == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Team)
                .ThenInclude(d => d.Division)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Player/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Players == null)
            {
                return Problem("The player data could not be retrieved. It may have been deleted or does not exist. If the issue persists, please contact support.");
            }
            var player = await _context.Players.FindAsync(id);

            try
            {
                if (player != null)
                {
                    player.Status = false; // Set status to inactive
                    _context.Players.Update(player);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return Redirect(ViewData["returnURL"].ToString());
        }
        private bool IsJerseyNumberDuplicate(Player player)
        {
            //bool isDuplicated = false;
            //if (_context.Players.Any(p => p.TeamID == player.TeamID && p.ID != player.ID && p.JerseyNumber == player.JerseyNumber))
            //    isDuplicated = true;
            //return isDuplicated;
            if (player.JerseyNumber == null || player.JerseyNumber == "0")
            {
                return false;
            }

            return _context.Players.Any(p => p.TeamID == player.TeamID && p.ID != player.ID && p.JerseyNumber == player.JerseyNumber);
        }
        private async Task<List<int>> GetCoachTeamsAsync(string coachEmail)
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
            return new List<int>();
        }

        private async Task<List<int>> GetScorekeeperTeamsAsync(string scorekeeperEmail)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == scorekeeperEmail);
            if (staff != null)
            {
                var teamIds = await _context.TeamStaff
                    .Where(ts => ts.StaffID == staff.ID)
                    .Select(ts => ts.TeamID)
                    .ToListAsync();

                return teamIds;
            }
            return new List<int>();
        }
        private async Task<List<int>> GetConvenorTeamIdsAsync(string convenorEmail)
        {
            var user = await _userManager.FindByEmailAsync(convenorEmail);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var teamIds = new List<int>();

                foreach (var role in roles)
                {
                    switch (role)
                    {
                        case "RookieConvenor":
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("9U"));
                            break;
                        case "IntermediateConvenor":
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("11U"));
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("13U"));
                            break;
                        case "SeniorConvenor":
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("15U"));
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("18U"));
                            break;
                        case "Admin":
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("9U"));
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("11U"));
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("13U"));
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("15U"));
                            teamIds.AddRange(await GetTeamIdsByDivisionAsync("18U"));
                            break;
                        default:
                            break;
                    }
                }

                return teamIds.ToList();
            }
            return new List<int>();
        }

        private async Task<List<int>> GetTeamIdsByDivisionAsync(string divisionName)
        {
            var teamIds = await _context.Teams
                .Where(t => t.Division.DivisionName == divisionName)
                .Select(t => t.ID)
                .ToListAsync();

            return teamIds;
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.ID == id);
        }
    }
}