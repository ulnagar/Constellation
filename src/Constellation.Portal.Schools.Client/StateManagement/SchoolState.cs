namespace Constellation.Portal.Schools.Client.StateManagement;

using Constellation.Application.DTOs;
using System.Net.Http.Json;

public class SchoolState
{
    public event Action OnChange;

    public SchoolDto SelectedSchool { get; private set; }
    public List<SchoolDto> AvailableSchools { get; private set; }

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SchoolState(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task UpdateAvailableSchools()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var http = scope.ServiceProvider.GetService<HttpClient>();

        AvailableSchools = await http.GetFromJsonAsync<List<SchoolDto>>("home/schools");
        SelectedSchool = AvailableSchools.OrderBy(school => school.Code).First();

        NotifyStateChanged();
    }

    public async Task GetAvailableSchools()
    {
        if (AvailableSchools == null || AvailableSchools.Count == 0){
            await UpdateAvailableSchools();
        }
    }

    public void UpdateSelectedSchool(string Code)
    {
        SelectedSchool = AvailableSchools.FirstOrDefault(school => school.Code == Code);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
