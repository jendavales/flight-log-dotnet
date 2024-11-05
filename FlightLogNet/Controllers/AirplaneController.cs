using System;
using System.Linq;

namespace FlightLogNet.Controllers
{
    using Facades;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using System.Collections.Generic;
    
    [EnableCors]
    [Route("airplane")]
    public class AirplaneController(ILogger<AirplaneController> logger, AirplaneFacade airplaneFacade)
        : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<AirplaneModel>> GetClubAirplanes()
        {
            logger.LogInformation("Fetching list of club airplanes.");

            try
            {
                var clubAirplanes = airplaneFacade.GetClubAirplanes();

                if (clubAirplanes.Count == 0)
                {
                    logger.LogWarning("No club airplanes found.");
                    return NoContent(); // Returns 204 No Content if no airplanes are found
                }

                logger.LogInformation("Successfully retrieved club airplanes.");
                return Ok(clubAirplanes); // Returns 200 OK with the list of airplanes
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching club airplanes.");
                return StatusCode(500, "Internal server error"); // Returns 500 Internal Server Error on exception
            }
        }
    }
}
