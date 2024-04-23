using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMBA_4.Data.WMBAMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CityName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GameTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PositionCode = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    PositionName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonCode = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    SeasonName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClubName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    CityID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Clubs_Cities_CityID",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CityID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Locations_Cities_CityID",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Staff_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DivisionName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DivisionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClubID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Divisions_Clubs_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Clubs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocationID = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonID = table.Column<int>(type: "INTEGER", nullable: false),
                    GameTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Games_GameTypes_GameTypeID",
                        column: x => x.GameTypeID,
                        principalTable: "GameTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Seasons_SeasonID",
                        column: x => x.SeasonID,
                        principalTable: "Seasons",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffDivision",
                columns: table => new
                {
                    DivisionID = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffDivision", x => new { x.DivisionID, x.StaffID });
                    table.ForeignKey(
                        name: "FK_StaffDivision_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffDivision_Staff_StaffID",
                        column: x => x.StaffID,
                        principalTable: "Staff",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ranking = table.Column<int>(type: "INTEGER", nullable: false),
                    DivisionID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Teams_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ranking = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberID = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    JerseyNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Players_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamGame",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    GameID = table.Column<int>(type: "INTEGER", nullable: false),
                    IsHomeTeam = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVisitorTeam = table.Column<bool>(type: "INTEGER", nullable: false),
                    score = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamGame", x => new { x.TeamID, x.GameID });
                    table.ForeignKey(
                        name: "FK_TeamGame_Games_GameID",
                        column: x => x.GameID,
                        principalTable: "Games",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamGame_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamStaff",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStaff", x => new { x.TeamID, x.StaffID });
                    table.ForeignKey(
                        name: "FK_TeamStaff_Staff_StaffID",
                        column: x => x.StaffID,
                        principalTable: "Staff",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamStaff_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameLineUps",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BattingOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    GameID = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLineUps", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GameLineUps_Games_GameID",
                        column: x => x.GameID,
                        principalTable: "Games",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLineUps_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLineUps_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Innings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InningNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ScorePerInning = table.Column<int>(type: "INTEGER", nullable: false),
                    ScorePerInningOpponent = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    GameID = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamGameGameID = table.Column<int>(type: "INTEGER", nullable: true),
                    TeamGameTeamID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Innings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Innings_Games_GameID",
                        column: x => x.GameID,
                        principalTable: "Games",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Innings_TeamGame_TeamGameTeamID_TeamGameGameID",
                        columns: x => new { x.TeamGameTeamID, x.TeamGameGameID },
                        principalTable: "TeamGame",
                        principalColumns: new[] { "TeamID", "GameID" });
                    table.ForeignKey(
                        name: "FK_Innings_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameLineUpPositions",
                columns: table => new
                {
                    GameLineUpID = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLineUpPositions", x => new { x.GameLineUpID, x.PositionID });
                    table.ForeignKey(
                        name: "FK_GameLineUpPositions_GameLineUps_GameLineUpID",
                        column: x => x.GameLineUpID,
                        principalTable: "GameLineUps",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLineUpPositions_Positions_PositionID",
                        column: x => x.PositionID,
                        principalTable: "Positions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScorePlayers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    H = table.Column<int>(type: "INTEGER", nullable: false),
                    RBI = table.Column<int>(type: "INTEGER", nullable: false),
                    Singles = table.Column<int>(type: "INTEGER", nullable: false),
                    Doubles = table.Column<int>(type: "INTEGER", nullable: false),
                    Triples = table.Column<int>(type: "INTEGER", nullable: false),
                    HR = table.Column<int>(type: "INTEGER", nullable: false),
                    BB = table.Column<int>(type: "INTEGER", nullable: false),
                    PA = table.Column<int>(type: "INTEGER", nullable: false),
                    AB = table.Column<int>(type: "INTEGER", nullable: false),
                    Run = table.Column<int>(type: "INTEGER", nullable: false),
                    HBP = table.Column<int>(type: "INTEGER", nullable: false),
                    StrikeOut = table.Column<int>(type: "INTEGER", nullable: false),
                    Out = table.Column<int>(type: "INTEGER", nullable: false),
                    Fouls = table.Column<int>(type: "INTEGER", nullable: false),
                    Balls = table.Column<int>(type: "INTEGER", nullable: false),
                    BattingOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    GameLineUpID = table.Column<int>(type: "INTEGER", nullable: false),
                    GameID = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScorePlayers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ScorePlayers_GameLineUps_GameLineUpID",
                        column: x => x.GameLineUpID,
                        principalTable: "GameLineUps",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScorePlayers_Games_GameID",
                        column: x => x.GameID,
                        principalTable: "Games",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ScorePlayers_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Inplays",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Runs = table.Column<int>(type: "INTEGER", nullable: false),
                    Strikes = table.Column<int>(type: "INTEGER", nullable: false),
                    Outs = table.Column<int>(type: "INTEGER", nullable: false),
                    Fouls = table.Column<int>(type: "INTEGER", nullable: false),
                    Balls = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamAtBat = table.Column<int>(type: "INTEGER", nullable: false),
                    Turns = table.Column<int>(type: "INTEGER", nullable: false),
                    OpponentOuts = table.Column<int>(type: "INTEGER", nullable: false),
                    InningID = table.Column<int>(type: "INTEGER", nullable: false),
                    NextPlayer = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerInBase1ID = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerInBase2ID = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerInBase3ID = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerBattingID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inplays", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Inplays_Innings_InningID",
                        column: x => x.InningID,
                        principalTable: "Innings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inplays_Players_PlayerBattingID",
                        column: x => x.PlayerBattingID,
                        principalTable: "Players",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Inplays_Players_PlayerInBase1ID",
                        column: x => x.PlayerInBase1ID,
                        principalTable: "Players",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Inplays_Players_PlayerInBase2ID",
                        column: x => x.PlayerInBase2ID,
                        principalTable: "Players",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Inplays_Players_PlayerInBase3ID",
                        column: x => x.PlayerInBase3ID,
                        principalTable: "Players",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_CityID",
                table: "Clubs",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_ClubID",
                table: "Divisions",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_DivisionName",
                table: "Divisions",
                column: "DivisionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameLineUpPositions_PositionID",
                table: "GameLineUpPositions",
                column: "PositionID");

            migrationBuilder.CreateIndex(
                name: "IX_GameLineUps_GameID",
                table: "GameLineUps",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_GameLineUps_PlayerID",
                table: "GameLineUps",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_GameLineUps_TeamID",
                table: "GameLineUps",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameTypeID",
                table: "Games",
                column: "GameTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Games_LocationID",
                table: "Games",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SeasonID",
                table: "Games",
                column: "SeasonID");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_GameID",
                table: "Innings",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_TeamGameTeamID_TeamGameGameID",
                table: "Innings",
                columns: new[] { "TeamGameTeamID", "TeamGameGameID" });

            migrationBuilder.CreateIndex(
                name: "IX_Innings_TeamID",
                table: "Innings",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_Inplays_InningID",
                table: "Inplays",
                column: "InningID");

            migrationBuilder.CreateIndex(
                name: "IX_Inplays_PlayerBattingID",
                table: "Inplays",
                column: "PlayerBattingID");

            migrationBuilder.CreateIndex(
                name: "IX_Inplays_PlayerInBase1ID",
                table: "Inplays",
                column: "PlayerInBase1ID");

            migrationBuilder.CreateIndex(
                name: "IX_Inplays_PlayerInBase2ID",
                table: "Inplays",
                column: "PlayerInBase2ID");

            migrationBuilder.CreateIndex(
                name: "IX_Inplays_PlayerInBase3ID",
                table: "Inplays",
                column: "PlayerInBase3ID");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CityID",
                table: "Locations",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Players_JerseyNumber_TeamID",
                table: "Players",
                columns: new[] { "JerseyNumber", "TeamID" },
                unique: true,
                filter: "[JerseyNumber] is not null and [TeamID] is not null");

            migrationBuilder.CreateIndex(
                name: "IX_Players_MemberID",
                table: "Players",
                column: "MemberID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamID",
                table: "Players",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionName",
                table: "Positions",
                column: "PositionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScorePlayers_GameID",
                table: "ScorePlayers",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_ScorePlayers_GameLineUpID",
                table: "ScorePlayers",
                column: "GameLineUpID");

            migrationBuilder.CreateIndex(
                name: "IX_ScorePlayers_PlayerID",
                table: "ScorePlayers",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_SeasonCode",
                table: "Seasons",
                column: "SeasonCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_RoleId",
                table: "Staff",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDivision_StaffID",
                table: "StaffDivision",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamGame_GameID",
                table: "TeamGame",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DivisionID",
                table: "Teams",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStaff_StaffID",
                table: "TeamStaff",
                column: "StaffID");
            ExtraMigration.Steps(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameLineUpPositions");

            migrationBuilder.DropTable(
                name: "Inplays");

            migrationBuilder.DropTable(
                name: "ScorePlayers");

            migrationBuilder.DropTable(
                name: "StaffDivision");

            migrationBuilder.DropTable(
                name: "TeamStaff");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Innings");

            migrationBuilder.DropTable(
                name: "GameLineUps");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "TeamGame");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "GameTypes");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
