using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface ISchoolRegisterGateway
    {
        Task GetSchoolPrincipals();
        Task UpdateSchoolDetails();
    }
}