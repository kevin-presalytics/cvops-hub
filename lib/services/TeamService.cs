using lib.models.db;
using System.Threading.Tasks;

namespace lib.services
{
    public interface ITeamService
    {
        Team GetOrCreateDefaultTeam(User user);
        Team GetTeamById(string teamId);
        Team UpdateTeam(Team team);
    }
    public class TeamService
    {
        public TeamService()
        {
        }

        public Team GetOrCreateDefaultTeam(User user)
        {
            throw new System.NotImplementedException();
        }

        public Team GetTeamById(string teamId)
        {
            throw new System.NotImplementedException();
        }

        
    }
}