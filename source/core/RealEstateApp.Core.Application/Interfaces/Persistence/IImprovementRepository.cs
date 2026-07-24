using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Persistence;

public interface IImprovementRepository : IGenericRepository<Improvement>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task RemoveRelationsAsync(int improvementId);
}