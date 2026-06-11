using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.SupportSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;
using Talentree.Service.DTOs.Support;

namespace Talentree.Service.Services
{
    public class SupportService : ISupportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly IAIService _aiService;
        private readonly ILogger<SupportService> _logger;
        private readonly INotificationHelperService _notificationHelper;
        private readonly UserManager<AppUser> _userManager;

        public SupportService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IEmailService emailService,
            IFileService fileService, IAIService aiService,
            INotificationHelperService notificationHelper,
            ILogger<SupportService> logger,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _emailService = emailService;
            _fileService = fileService;
            _aiService = aiService;
            _notificationHelper = notificationHelper;
             _logger = logger;
            _userManager = userManager;
        }

        // ═══════════════════════════════════════════════════════════
        // CREATE TICKET
        // ═══════════════════════════════════════════════════════════

        public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, string userId)
        {
            // Validate attachments
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                if (dto.Attachments.Count > 3)
                    throw new BadRequestException("Maximum 3 attachments allowed per ticket");

                foreach (var file in dto.Attachments)
                {
                    if (file.Length > 10 * 1024 * 1024) // 10MB
                        throw new BadRequestException($"File {file.FileName} exceeds 10MB limit");

                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf", "text/plain" };
                    if (!allowedTypes.Contains(file.ContentType))
                        throw new BadRequestException($"File type {file.ContentType} is not allowed");
                }
            }

            // Generate ticket number
            var ticketNumber = await GenerateTicketNumberAsync();

            // Create ticket
            var ticket = new SupportTicket
            {
                TicketNumber = ticketNumber,
                Subject = dto.Subject,
                Description = dto.Description,
                Category = dto.Category,
                Status = TicketStatus.Open,
                Priority = TicketPriority.Normal,
                BusinessOwnerUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<SupportTicket>().Add(ticket);
            _ = Task.Run(async () =>
            {
                try
                {
                    await _aiService.PredictTriageAsync(ticket.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background AI prediction task");
                }
            });
            await _unitOfWork.CompleteAsync();

            // Upload attachments
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var file in dto.Attachments)
                {
                    var fileUrl = await _fileService.UploadFileAsync(file, "support-tickets");

                    var attachment = new TicketAttachment
                    {
                        TicketId = ticket.Id,
                        FileName = file.FileName,
                        FileUrl = fileUrl,
                        FileSizeBytes = file.Length,
                        ContentType = file.ContentType,
                        UploadedBy = userId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Repository<TicketAttachment>().Add(attachment);
                }

                await _unitOfWork.CompleteAsync();
            }

          


            // Notify admins using helper
            await _notificationHelper.NotifyAllAdmins(
                title: "New Support Ticket",
                message: $"New {ticket.Category} ticket #{ticket.TicketNumber}: {ticket.Subject}",
                type: NotificationType.Support,
                actionUrl: $"/admin/support/tickets/{ticket.Id}"
            );

            // notify business owner
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Support,
                Title = "Ticket Created ✅",
                Message = $"Your support ticket #{ticket.TicketNumber} has been created. Our team will respond shortly.",
                ActionUrl = $"/support/tickets/{ticket.Id}",
                ActionText = "View Ticket",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "SupportTicket",
                RelatedEntityId = ticket.Id
            });

            _logger.LogInformation("Support ticket {TicketNumber} created by user {UserId}", ticket.TicketNumber, userId);

            // Get ticket with all data
            var spec = new TicketByIdSpecification(ticket.Id);
            var createdTicket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            return _mapper.Map<TicketDto>(createdTicket!);
        }

        // ═══════════════════════════════════════════════════════════
        // GET TICKETS
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<TicketListItemDto>> GetMyTicketsAsync(
            string userId,
            TicketStatus? status = null,
            TicketCategory? category = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var countSpec = new TicketsByUserSpecification(userId, status, category);
            var totalCount = await _unitOfWork.Repository<SupportTicket>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new TicketsByUserSpecification(userId, status, category, pageIndex, pageSize);
            var tickets = await _unitOfWork.Repository<SupportTicket>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<TicketListItemDto>>(tickets);

            return new Pagination<TicketListItemDto>(pageIndex, pageSize, totalCount, dtos);
        }

        public async Task<TicketDto> GetTicketByIdAsync(int ticketId, string userId)
        {


            var spec = new TicketByIdAndUserSpecification(ticketId, userId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            return _mapper.Map<TicketDto>(ticket);
        }

        // ═══════════════════════════════════════════════════════════
        // ADD MESSAGE
        // ═══════════════════════════════════════════════════════════

        public async Task<TicketMessageDto> AddMessageAsync(AddTicketMessageDto dto, string userId)
        {
            var spec = new TicketByIdAndUserSpecification(dto.TicketId, userId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            if (ticket.Status == TicketStatus.Closed)
                throw new BadRequestException("Cannot add message to closed ticket");

            // Validate attachments
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                if (dto.Attachments.Count > 3)
                    throw new BadRequestException("Maximum 3 attachments allowed per message");

                foreach (var file in dto.Attachments)
                {
                    if (file.Length > 10 * 1024 * 1024)
                        throw new BadRequestException($"File {file.FileName} exceeds 10MB limit");
                }
            }

            // Create message
            var message = new TicketMessage
            {
                TicketId = dto.TicketId,
                SenderId = userId,
                Content = dto.Content,
                IsAdminMessage = false,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<TicketMessage>().Add(message);
            await _unitOfWork.CompleteAsync();

            // Upload attachments
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var file in dto.Attachments)
                {
                    var fileUrl = await _fileService.UploadFileAsync(file, "support-tickets");

                    var attachment = new TicketAttachment
                    {
                        TicketId = dto.TicketId,
                        MessageId = message.Id,
                        FileName = file.FileName,
                        FileUrl = fileUrl,
                        FileSizeBytes = file.Length,
                        ContentType = file.ContentType,
                        UploadedBy = userId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Repository<TicketAttachment>().Add(attachment);
                }

                await _unitOfWork.CompleteAsync();
            }

            // Reopen if resolved
            if (ticket.Status == TicketStatus.Resolved)
            {
                ticket.Status = TicketStatus.Open;
                _unitOfWork.Repository<SupportTicket>().Update(ticket);
                await _unitOfWork.CompleteAsync();
            }


            // Notify admins using helper
            await _notificationHelper.NotifyAllAdmins(
                title: "New Ticket Reply",
                message: $"Business owner replied to ticket #{ticket.TicketNumber}",
                type: NotificationType.Support,
                actionUrl: $"/admin/support/tickets/{ticket.Id}"
            );

            _logger.LogInformation("New message added to ticket {TicketNumber}", ticket.TicketNumber);

            // Get message with data
            var messageSpec = new TicketMessageByIdSpecification(message.Id);
            var createdMessage = await _unitOfWork.Repository<TicketMessage>()
                .GetByIdWithSpecificationsAsync(messageSpec);

            return _mapper.Map<TicketMessageDto>(createdMessage!);
        }

        // ═══════════════════════════════════════════════════════════
        // CLOSE TICKET
        // ═══════════════════════════════════════════════════════════

        public async Task CloseTicketAsync(int ticketId, string userId)
        {
            var spec = new TicketByIdAndUserSpecification(ticketId, userId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            if (ticket.Status == TicketStatus.Closed)
                throw new BadRequestException("Ticket is already closed");

            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.ClosedBy = userId;

            _unitOfWork.Repository<SupportTicket>().Update(ticket);
            await _unitOfWork.CompleteAsync();

            // ✅ ADD NOTIFICATION
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Support,
                Title = "Ticket Closed ✅",
                Message = $"Your support ticket #{ticket.TicketNumber} has been closed.",
                ActionUrl = $"/support/tickets/{ticket.Id}",
                ActionText = "View Ticket",
                Priority = NotificationPriority.Normal,
                SendEmail = false,
                RelatedEntityType = "SupportTicket",
                RelatedEntityId = ticket.Id
            });

            _logger.LogInformation("Ticket {TicketNumber} closed by user {UserId}", ticket.TicketNumber, userId);
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN: GET ALL TICKETS
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<TicketListItemDto>> GetAllTicketsAsync(
            TicketStatus? status = null,
            TicketPriority? priority = null,
            string? assignedToAdminId = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var countSpec = new AllTicketsSpecification(status, priority, assignedToAdminId);
            var totalCount = await _unitOfWork.Repository<SupportTicket>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new AllTicketsSpecification(status, priority, assignedToAdminId, pageIndex, pageSize);
            var tickets = await _unitOfWork.Repository<SupportTicket>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<TicketListItemDto>>(tickets);

            return new Pagination<TicketListItemDto>(pageIndex, pageSize, totalCount, dtos);
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN: UPDATE STATUS
        // ═══════════════════════════════════════════════════════════

        public async Task UpdateTicketStatusAsync(UpdateTicketStatusDto dto, string adminId)
        {
            var spec = new TicketByIdSpecification(dto.TicketId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            var oldStatus = ticket.Status;
            ticket.Status = dto.Status;

            switch (dto.Status)
            {
                case TicketStatus.Resolved:
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedBy = adminId;
                    break;

                case TicketStatus.Closed:
                    ticket.ClosedAt = DateTime.UtcNow;
                    ticket.ClosedBy = adminId;
                    break;
            }

            _unitOfWork.Repository<SupportTicket>().Update(ticket);
            await _unitOfWork.CompleteAsync();

            // Add system message
            if (!string.IsNullOrEmpty(dto.Note))
            {
                var message = new TicketMessage
                {
                    TicketId = dto.TicketId,
                    SenderId = adminId,
                    Content = $"<strong>Status changed to {dto.Status}</strong><br>{dto.Note}",
                    IsAdminMessage = true,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<TicketMessage>().Add(message);
                await _unitOfWork.CompleteAsync();
            }

            // Notify business owner
            await NotifyBusinessOwnerStatusChangeAsync(ticket, oldStatus, dto.Status);

            _logger.LogInformation(
                "Ticket {TicketNumber} status updated from {OldStatus} to {NewStatus} by admin {AdminId}",
                ticket.TicketNumber, oldStatus, dto.Status, adminId);
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN: ASSIGN TICKET
        // ═══════════════════════════════════════════════════════════

        public async Task AssignTicketAsync(AssignTicketDto dto, string assignedBy)
        {
            var spec = new TicketByIdSpecification(dto.TicketId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            ticket.AssignedToAdminId = dto.AdminId;
            ticket.AssignedAt = DateTime.UtcNow;

            _unitOfWork.Repository<SupportTicket>().Update(ticket);
            await _unitOfWork.CompleteAsync();

            // Notify assigned admin
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.AdminId,
                Type = NotificationType.Support,
                Title = "Ticket Assigned to You",
                Message = $"Support ticket #{ticket.TicketNumber} has been assigned to you.",
                ActionUrl = $"/admin/support/tickets/{ticket.Id}",
                ActionText = "View Ticket",
                Priority = NotificationPriority.Normal,
                SendEmail = false
            });


            _logger.LogInformation(
                "Ticket {TicketNumber} assigned to admin {AdminId}",
                ticket.TicketNumber, dto.AdminId);
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN: UPDATE PRIORITY
        // ═══════════════════════════════════════════════════════════

        public async Task UpdateTicketPriorityAsync(int ticketId, TicketPriority priority, string adminId)
        {
            var spec = new TicketByIdSpecification(ticketId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            ticket.Priority = priority;

            _unitOfWork.Repository<SupportTicket>().Update(ticket);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN: GET TICKET DETAILS
        // ═══════════════════════════════════════════════════════════
        public async Task<TicketDto> GetTicketByIdForAdminAsync(int ticketId)
        {
            var spec = new TicketByIdSpecification(ticketId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            return _mapper.Map<TicketDto>(ticket);
        }

        // ═══════════════════════════════════════════════════════════
        // ADMIN: ADD MESSAGE/REPLY
        // ═══════════════════════════════════════════════════════════
        public async Task<TicketMessageDto> AddAdminMessageAsync(AddTicketMessageDto dto, string adminId)
        {
            var spec = new TicketByIdSpecification(dto.TicketId);
            var ticket = await _unitOfWork.Repository<SupportTicket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (ticket == null)
                throw new NotFoundException("Ticket not found");

            if (ticket.Status == TicketStatus.Closed)
                throw new BadRequestException("Cannot add message to closed ticket");

            // Validate attachments
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                if (dto.Attachments.Count > 3)
                    throw new BadRequestException("Maximum 3 attachments allowed per message");

                foreach (var file in dto.Attachments)
                {
                    if (file.Length > 10 * 1024 * 1024)
                        throw new BadRequestException($"File {file.FileName} exceeds 10MB limit");
                }
            }

            // Create message
            var message = new TicketMessage
            {
                TicketId = dto.TicketId,
                SenderId = adminId,
                Content = dto.Content,
                IsAdminMessage = true,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<TicketMessage>().Add(message);
            await _unitOfWork.CompleteAsync();

            // Upload attachments
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                foreach (var file in dto.Attachments)
                {
                    var fileUrl = await _fileService.UploadFileAsync(file, "support-tickets");

                    var attachment = new TicketAttachment
                    {
                        TicketId = dto.TicketId,
                        MessageId = message.Id,
                        FileName = file.FileName,
                        FileUrl = fileUrl,
                        FileSizeBytes = file.Length,
                        ContentType = file.ContentType,
                        UploadedBy = adminId,
                        UploadedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Repository<TicketAttachment>().Add(attachment);
                }

                await _unitOfWork.CompleteAsync();
            }

            // Set ticket status to InProgress if open or awaiting reply
            if (ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.AwaitingReply)
            {
                ticket.Status = TicketStatus.InProgress;
                _unitOfWork.Repository<SupportTicket>().Update(ticket);
                await _unitOfWork.CompleteAsync();
            }

            // Notify business owner of admin reply
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = ticket.BusinessOwnerUserId,
                Type = NotificationType.Support,
                Title = "New Reply from Support",
                Message = $"Support admin replied to ticket #{ticket.TicketNumber}",
                ActionUrl = $"/support/tickets/{ticket.Id}",
                ActionText = "View Reply",
                Priority = NotificationPriority.High,
                SendEmail = true
            });

            // Get message with data
            var messageSpec = new TicketMessageByIdSpecification(message.Id);
            var createdMessage = await _unitOfWork.Repository<TicketMessage>()
                .GetByIdWithSpecificationsAsync(messageSpec);

            return _mapper.Map<TicketMessageDto>(createdMessage!);
        }

        // ═══════════════════════════════════════════════════════════
        // FAQ
        // ═══════════════════════════════════════════════════════════

        public async Task<List<FAQDto>> GetFAQsAsync(string? category = null)
        {
            var spec = new PublishedFAQsSpecification(category);
            var faqs = await _unitOfWork.Repository<FAQ>()
                .GetAllWithSpecificationsAsync(spec);

            return _mapper.Map<List<FAQDto>>(faqs.ToList());
        }

        public async Task<List<FAQDto>> SearchFAQsAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new List<FAQDto>();

            var spec = new PublishedFAQsSpecification(null, searchQuery);
            var faqs = await _unitOfWork.Repository<FAQ>()
                .GetAllWithSpecificationsAsync(spec);

            return _mapper.Map<List<FAQDto>>(faqs.ToList());
        }

        public async Task<FAQDto> GetFAQByIdAsync(int faqId)
        {
            var faq = await _unitOfWork.Repository<FAQ>().GetByIdAsync(faqId);

            if (faq == null || !faq.IsPublished)
                throw new NotFoundException("FAQ not found");

            var dto = _mapper.Map<FAQDto>(faq);

            // Get related FAQs
            if (!string.IsNullOrEmpty(faq.RelatedFaqIds))
            {
                var relatedIds = faq.RelatedFaqIds.Split(',')
                    .Select(id => int.TryParse(id, out var result) ? result : 0)
                    .Where(id => id > 0)
                    .ToList();

                if (relatedIds.Any())
                {
                    var relatedFaqsList = new List<FAQ>();
                    foreach (var id in relatedIds)
                    {
                        var relatedFaq = await _unitOfWork.Repository<FAQ>().GetByIdAsync(id);
                        if (relatedFaq != null && relatedFaq.IsPublished)
                        {
                            relatedFaqsList.Add(relatedFaq);
                        }
                    }

                    dto.RelatedFaqs = _mapper.Map<List<FAQDto>>(relatedFaqsList);
                }
            }

            return dto;
        }

        public async Task<List<FAQCategoryDto>> GetFAQCategoriesAsync()
        {
            var spec = new PublishedFAQsSpecification();
            var faqs = await _unitOfWork.Repository<FAQ>()
                .GetAllWithSpecificationsAsync(spec);

            var categories = faqs
                .GroupBy(f => f.Category)
                .Select(g => new FAQCategoryDto
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderBy(c => c.Category)
                .ToList();

            return categories;
        }

        public async Task IncrementFAQViewCountAsync(int faqId)
        {
            var faq = await _unitOfWork.Repository<FAQ>().GetByIdAsync(faqId);

            if (faq != null && faq.IsPublished)
            {
                faq.ViewCount++;
                _unitOfWork.Repository<FAQ>().Update(faq);
                await _unitOfWork.CompleteAsync();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        private async Task<string> GenerateTicketNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var yearSpec = new TicketsCountByYearSpecification(year);
            var count = await _unitOfWork.Repository<SupportTicket>()
                .GetCountWithSpecificationsAsync(yearSpec);

            return $"TKT-{year}-{(count + 1):D5}";
        }

     

        private async Task NotifyBusinessOwnerStatusChangeAsync(
            SupportTicket ticket,
            TicketStatus oldStatus,
            TicketStatus newStatus)
        {
            var statusMessages = new Dictionary<TicketStatus, string>
            {
                { TicketStatus.InProgress, "Your support ticket is now being reviewed by our team." },
                { TicketStatus.AwaitingReply, "We need more information from you. Please check your ticket." },
                { TicketStatus.Resolved, "Your support ticket has been resolved! Please confirm." },
                { TicketStatus.Closed, "Your support ticket has been closed." }
            };

            if (statusMessages.ContainsKey(newStatus))
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = ticket.BusinessOwnerUserId,
                    Type = NotificationType.Support,
                    Title = $"Ticket #{ticket.TicketNumber} - Status Updated",
                    Message = statusMessages[newStatus],
                    ActionUrl = $"/support/tickets/{ticket.Id}",
                    ActionText = "View Ticket",
                    Priority = NotificationPriority.High,
                    SendEmail = true
                });
            }
        }

       
        private async Task<List<string>> GetAllAdminIdsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return admins.Select(u => u.Id).ToList();
        }
    }
}