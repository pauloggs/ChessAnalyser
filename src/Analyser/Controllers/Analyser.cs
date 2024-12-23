﻿using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;

namespace Analyser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Analyser(IChessRepository chessRepository, IEtlService etlService) : ControllerBase
    {
        private readonly IChessRepository chessRepository = chessRepository;
        private readonly IEtlService etlService = etlService;

        /// <summary>
        /// Load PGN files and persist to database. For Windows, try C:\PGN\ path.
        /// </summary>
        [HttpGet("LoadGames")]
        public async Task<IActionResult> LoadGames(string filePath = "/Library/PGN")
        {
            try
            {

                await etlService.LoadGamesToDatabase(filePath);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get games from database.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetGames")]
        public async Task<IActionResult> GetGames()
        {
            var games = await chessRepository.GetGames();
            return Ok(games);
        }
    }
}
