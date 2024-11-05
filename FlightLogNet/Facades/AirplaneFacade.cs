namespace FlightLogNet.Facades
{
    using System.Collections.Generic;

    using Models;
    using Repositories.Interfaces;

    public class AirplaneFacade(IAirplaneRepository airplaneRepository)
    {
        public IList<AirplaneModel> GetClubAirplanes()
        {
            return airplaneRepository.GetClubAirplanes();
        }
    }
}
