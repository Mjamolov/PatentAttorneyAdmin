using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.ViewModels;

public class ClientDashboardViewModel
{
    public List<ClientRequest> RecentRequests { get; set; } = new();
    public List<ServiceApplication> RecentApplications { get; set; } = new();
}
