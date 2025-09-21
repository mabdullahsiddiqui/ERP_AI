using System.Threading.Tasks;
using ERP_AI.Data;

namespace ERP_AI.CloudSync
{
    public interface IConflictResolver
    {
        Task<bool> ResolveAsync(ConflictResolution conflict);
        ConflictResolutionStrategy GetResolutionStrategy(string entityType);
    }
}

