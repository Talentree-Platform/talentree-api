using System.Threading.Tasks;
using Talentree.Service.DTOs.Admin;

namespace Talentree.Service.Contracts
{
    public interface ISecuritySettingsService
    {
        Task<SecuritySettingsDto> GetSecuritySettingsAsync();
        Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(UpdateSecuritySettingsDto dto, string performingAdminId);
    }
}
