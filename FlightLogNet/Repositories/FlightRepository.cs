namespace FlightLogNet.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;

    using Models;
    using Entities;
    using Interfaces;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class FlightRepository(IMapper mapper, IConfiguration configuration) : IFlightRepository
    {
        public IList<FlightModel> GetFlightsByType(FlightType flightType)
        {
            using var dbContext = new LocalDatabaseContext(configuration);

            var flights = dbContext.Flights.Where(f => f.Type == flightType);

            return mapper.ProjectTo<FlightModel>(flights).ToList();
        }

        public IList<FlightModel> GetAirplanesInAir()
        {
            using var dbContext = new LocalDatabaseContext(configuration);

            var airborneFlights = dbContext.Flights
                .Where(f => f.LandingTime == null)
                .OrderBy(f => f.TakeoffTime)
                .ThenBy(f => f.Type == FlightType.Towplane ? 0 : 1) // Towplanes first
                .ThenBy(f => f.Id) // Optional: to ensure consistent ordering
                .AsQueryable();

            return mapper.ProjectTo<FlightModel>(airborneFlights).ToList();
        }


        public void LandFlight(FlightLandingModel landingModel)
        {
            using var dbContext = new LocalDatabaseContext(configuration);

            var flight = dbContext.Flights.Find(landingModel.FlightId) 
                         ?? throw new NotSupportedException($"Unable to land not-registered flight: {landingModel}.");
            flight.LandingTime = landingModel.LandingTime;
            dbContext.SaveChanges();
        }

        public void TakeoffFlight(long? gliderFlightId, long? towplaneFlightId)
        {
            using var dbContext = new LocalDatabaseContext(configuration);

            var flightStart = new FlightStart
            {
                Glider = dbContext.Flights.Find(gliderFlightId),
                Towplane = dbContext.Flights.Find(towplaneFlightId),
            };

            dbContext.FlightStarts.Add(flightStart);
            dbContext.SaveChanges();
        }

        public long CreateFlight(CreateFlightModel model)
        {
            using var dbContext = new LocalDatabaseContext(configuration);

            var copilot = model.CopilotId != null
                ? dbContext.Persons.Find(model.CopilotId)
                : null;

            var flight = new Flight
            {
                Airplane = dbContext.Airplanes.Find(model.AirplaneId),
                Copilot = copilot,
                Pilot = dbContext.Persons.Find(model.PilotId),
                TakeoffTime = model.TakeOffTime,
                Task = model.Task
            };

            dbContext.Flights.Add(flight);
            dbContext.SaveChanges();

            return flight.Id;
        }

        public IList<ReportModel> GetReport()
        {
            using var dbContext = new LocalDatabaseContext(configuration);

            var flightStarts = dbContext.FlightStarts
                .Include(flight => flight.Glider)
                .Include(flight => flight.Glider.Airplane)
                .Include(flight => flight.Glider.Pilot)
                .Include(flight => flight.Glider.Copilot)
                .Include(flight => flight.Towplane)
                .Include(flight => flight.Towplane.Airplane)
                .Include(flight => flight.Towplane.Pilot)
                .Include(flight => flight.Towplane.Copilot)
                .OrderByDescending(start => start.Towplane.TakeoffTime);

            return mapper.ProjectTo<ReportModel>(flightStarts).ToList();
        }
    }
}
