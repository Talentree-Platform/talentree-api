using Talentree.Core.Enums;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Support;

namespace Talentree.Service.Contracts
{
    public interface ISupportService
    {
        // Ticket Management
        Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, string userId);
        Task<Pagination<TicketListItemDto>> GetMyTicketsAsync(
            string userId, TicketStatus? status = null, TicketCategory? category = null,
            int pageIndex = 1, int pageSize = 20);
        Task<TicketDto> GetTicketByIdAsync(int ticketId, string userId);
        Task<TicketMessageDto> AddMessageAsync(AddTicketMessageDto dto, string userId);
        Task CloseTicketAsync(int ticketId, string userId);

        // Admin Operations
        Task<Pagination<TicketListItemDto>> GetAllTicketsAsync(
            TicketStatus? status = null, TicketPriority? priority = null,
            string? assignedToAdminId = null, int pageIndex = 1, int pageSize = 20);
        Task UpdateTicketStatusAsync(UpdateTicketStatusDto dto, string adminId);
        Task AssignTicketAsync(AssignTicketDto dto, string assignedBy);
        Task UpdateTicketPriorityAsync(int ticketId, TicketPriority priority, string adminId);

        // FAQ
        Task<List<FAQDto>> GetFAQsAsync(string? category = null);
        Task<List<FAQDto>> SearchFAQsAsync(string searchQuery);
        Task<FAQDto> GetFAQByIdAsync(int faqId);
        Task<List<FAQCategoryDto>> GetFAQCategoriesAsync();
        Task IncrementFAQViewCountAsync(int faqId);
    }
}