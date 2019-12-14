using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;

namespace za.co.grindrodbank.a3s.Services
{
    public interface IClientService
    {
        Task<List<Oauth2Client>> GetListAsync();
    }
}
