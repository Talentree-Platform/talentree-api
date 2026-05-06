using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.UserManagementSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;
using Talentree.Service.DTOs.UserManagement;

// Alias to avoid ambiguity with Stripe.Product
using TalentreeProduct = Talentree.Core.Entities.Product;

namespace Talentree.Service.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public UserManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // ═══════════════════════════════════════════════════════════
        // BUSINESS OWNER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<BusinessOwnerListDto>> GetBusinessOwnersAsync(
            string? searchQuery = null,
            ApprovalStatus? status = null,
            AccountStatus? accountStatus = null,
            string? category = null,
            DateTime? registrationDateFrom = null,
            DateTime? registrationDateTo = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var countSpec = new BusinessOwnersSpecification(
                searchQuery, status, accountStatus, category,
                registrationDateFrom, registrationDateTo);

            var totalCount = await _unitOfWork.Repository<AppUser>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new BusinessOwnersSpecification(
                searchQuery, status, accountStatus, category,
                registrationDateFrom, registrationDateTo, pageIndex, pageSize);

            var users = await _unitOfWork.Repository<AppUser>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = new List<BusinessOwnerListDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<BusinessOwnerListDto>(user);

                // Get stats
                dto.TotalProducts = await GetUserProductCountAsync(user.Id);
                dto.TotalOrders = await GetUserOrderCountAsync(user.Id);
                dto.TotalRevenue = await GetUserTotalRevenueAsync(user.Id);

                dtos.Add(dto);
            }

            return new Pagination<BusinessOwnerListDto>(pageIndex, pageSize, totalCount, dtos);
        }

        public async Task<BusinessOwnerDetailsDto> GetBusinessOwnerDetailsAsync(string userId)
        {
            var spec = new BusinessOwnerByIdSpecification(userId);
            var user = await _unitOfWork.Repository<AppUser>()
                .GetByIdWithSpecificationsAsync(spec);

            if (user == null)
                throw new NotFoundException("Business owner not found");

            var dto = _mapper.Map<BusinessOwnerDetailsDto>(user);

            // Get stats
            dto.TotalProducts = await GetUserProductCountAsync(userId);
            dto.TotalOrders = await GetUserOrderCountAsync(userId);
            dto.TotalRevenue = await GetUserTotalRevenueAsync(userId);
            dto.ComplaintCount = await GetUserComplaintCountAsync(userId);

            // Get action logs
            dto.ActionLogs = await GetUserActionLogsAsync(userId);

            return dto;
        }

        public async Task SuspendBusinessOwnerAsync(SuspendUserDto dto, string adminId)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (user.AccountStatus == AccountStatus.Suspended)
                throw new BadRequestException("User is already suspended");

            if (user.AccountStatus == AccountStatus.Banned)
                throw new BadRequestException("Cannot suspend a banned user");

            // Update user status
            user.AccountStatus = AccountStatus.Suspended;
            user.SuspendedAt = DateTime.UtcNow;
            user.SuspendedBy = adminId;
            user.SuspensionReason = dto.Reason;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to suspend user");

            // Log action
            await LogUserActionAsync(dto.UserId, adminId, "Suspend", dto.Reason, dto.Notes);

            // Hide products from marketplace
            await HideUserProductsAsync(dto.UserId);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.UserId,
                Type = NotificationType.Account,
                Title = "Account Suspended",
                Message = $"Your account has been suspended. Reason: {dto.Reason}",
                Priority = NotificationPriority.High,
                SendEmail = true
            });
        }

        public async Task UnsuspendBusinessOwnerAsync(string userId, string adminId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (user.AccountStatus != AccountStatus.Suspended)
                throw new BadRequestException("User is not suspended");

            // Update user status
            user.AccountStatus = AccountStatus.Active;
            user.SuspendedAt = null;
            user.SuspendedBy = null;
            user.SuspensionReason = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to unsuspend user");

            // Log action
            await LogUserActionAsync(userId, adminId, "Unsuspend", "Account suspension lifted", null);

            // Restore products
            await RestoreUserProductsAsync(userId);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Reactivated",
                Message = "Your account suspension has been lifted. You can now access your account.",
                Priority = NotificationPriority.High,
                SendEmail = true
            });
        }

        public async Task BanBusinessOwnerAsync(BanUserDto dto, string adminId)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (user.AccountStatus == AccountStatus.Banned)
                throw new BadRequestException("User is already banned");

            // Update user status
            user.AccountStatus = AccountStatus.Banned;
            user.BannedAt = DateTime.UtcNow;
            user.BannedBy = adminId;
            user.BanReason = dto.Reason;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to ban user");

            // Log action
            await LogUserActionAsync(dto.UserId, adminId, "Ban", dto.Reason, dto.Notes);

            // Permanently hide products
            await PermanentlyHideUserProductsAsync(dto.UserId);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.UserId,
                Type = NotificationType.Account,
                Title = "Account Banned",
                Message = $"Your account has been permanently banned. Reason: {dto.Reason}",
                Priority = NotificationPriority.High,
                SendEmail = true
            });
        }

        public async Task BlockBusinessOwnerAsync(BlockUserDto dto, string adminId)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (user.IsBlocked)
                throw new BadRequestException("User is already blocked");

            // Update user status
            user.IsBlocked = true;
            user.BlockedAt = DateTime.UtcNow;
            user.BlockedBy = adminId;
            user.BlockReason = dto.Reason;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to block user");

            // Log action
            await LogUserActionAsync(dto.UserId, adminId, "Block", dto.Reason, null);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.UserId,
                Type = NotificationType.Account,
                Title = "Account Blocked",
                Message = $"Your account has been blocked. Reason: {dto.Reason}",
                Priority = NotificationPriority.High,
                SendEmail = true
            });
        }

        public async Task UnblockBusinessOwnerAsync(string userId, string adminId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (!user.IsBlocked)
                throw new BadRequestException("User is not blocked");

            // Update user status
            user.IsBlocked = false;
            user.BlockedAt = null;
            user.BlockedBy = null;
            user.BlockReason = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to unblock user");

            // Log action
            await LogUserActionAsync(userId, adminId, "Unblock", "Account block removed", null);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Unblocked",
                Message = "Your account has been unblocked. You can now access your account.",
                Priority = NotificationPriority.Normal,
                SendEmail = true
            });
        }

        // ═══════════════════════════════════════════════════════════
        // CUSTOMER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<CustomerListDto>> GetCustomersAsync(
            string? searchQuery = null,
            AccountStatus? accountStatus = null,
            DateTime? registrationDateFrom = null,
            DateTime? registrationDateTo = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var countSpec = new CustomersSpecification(
                searchQuery, accountStatus, registrationDateFrom, registrationDateTo);

            var totalCount = await _unitOfWork.Repository<AppUser>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new CustomersSpecification(
                searchQuery, accountStatus, registrationDateFrom, registrationDateTo,
                pageIndex, pageSize);

            var users = await _unitOfWork.Repository<AppUser>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = new List<CustomerListDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<CustomerListDto>(user);

                // Get stats
                dto.TotalOrders = await GetCustomerOrderCountAsync(user.Id);
                dto.TotalSpent = await GetCustomerTotalSpentAsync(user.Id);

                dtos.Add(dto);
            }

            return new Pagination<CustomerListDto>(pageIndex, pageSize, totalCount, dtos);
        }

        public async Task<CustomerDetailsDto> GetCustomerDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Customer not found");

            if (user.BusinessOwnerProfile != null)
                throw new BadRequestException("This is a business owner, not a customer");

            var dto = _mapper.Map<CustomerDetailsDto>(user);

            // Get stats
            dto.TotalOrders = await GetCustomerOrderCountAsync(userId);
            dto.TotalSpent = await GetCustomerTotalSpentAsync(userId);
            dto.TotalReviews = await GetCustomerReviewCountAsync(userId);

            // Get action logs
            dto.ActionLogs = await GetUserActionLogsAsync(userId);

            return dto;
        }

        public async Task BlockCustomerAsync(BlockUserDto dto, string adminId)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (user.IsBlocked)
                throw new BadRequestException("User is already blocked");

            // Update user status
            user.IsBlocked = true;
            user.BlockedAt = DateTime.UtcNow;
            user.BlockedBy = adminId;
            user.BlockReason = dto.Reason;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to block user");

            // Log action
            await LogUserActionAsync(dto.UserId, adminId, "Block", dto.Reason, null);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = dto.UserId,
                Type = NotificationType.Account,
                Title = "Account Blocked",
                Message = $"Your account has been blocked. Reason: {dto.Reason}",
                Priority = NotificationPriority.High,
                SendEmail = true
            });
        }

        public async Task UnblockCustomerAsync(string userId, string adminId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            if (!user.IsBlocked)
                throw new BadRequestException("User is not blocked");

            // Update user status
            user.IsBlocked = false;
            user.BlockedAt = null;
            user.BlockedBy = null;
            user.BlockReason = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to unblock user");

            // Log action
            await LogUserActionAsync(userId, adminId, "Unblock", "Account block removed", null);

            // Send notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Unblocked",
                Message = "Your account has been unblocked.",
                Priority = NotificationPriority.Normal,
                SendEmail = true
            });
        }

        public async Task DeleteCustomerAsync(string userId, string adminId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            // Log action before deletion
            await LogUserActionAsync(userId, adminId, "Delete", "Account permanently deleted", null);

            // Delete user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to delete user");
        }

        // ═══════════════════════════════════════════════════════════
        // COMPLAINT MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        public async Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto dto, string reportedByUserId)
        {
            var complaint = new Complaint
            {
                ReportedUserId = dto.ReportedUserId,
                ReportedByUserId = reportedByUserId,
                ViolationType = dto.ViolationType,
                Description = dto.Description,
                RelatedOrderId = dto.RelatedOrderId,
                RelatedProductId = dto.RelatedProductId,
                RelatedContext = dto.RelatedContext,
                Status = ComplaintStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Complaint>().Add(complaint);
            await _unitOfWork.CompleteAsync();

            // Check if auto-block rules should be applied
            await CheckAndApplyAutoBlockRulesAsync(dto.ReportedUserId);

            // Get complaint with includes
            var spec = new ComplaintsSpecification(dto.ReportedUserId);
            var createdComplaint = await _unitOfWork.Repository<Complaint>()
                .GetAllWithSpecificationsAsync(spec);

            return _mapper.Map<ComplaintDto>(createdComplaint.FirstOrDefault(c => c.Id == complaint.Id)!);
        }

        public async Task<Pagination<ComplaintDto>> GetComplaintsAsync(
            string? reportedUserId = null,
            ComplaintStatus? status = null,
            ViolationType? violationType = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var countSpec = new ComplaintsSpecification(reportedUserId, status, violationType);
            var totalCount = await _unitOfWork.Repository<Complaint>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new ComplaintsSpecification(reportedUserId, status, violationType, pageIndex, pageSize);
            var complaints = await _unitOfWork.Repository<Complaint>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<ComplaintDto>>(complaints);

            return new Pagination<ComplaintDto>(pageIndex, pageSize, totalCount, dtos);
        }

        public async Task<ComplaintDto> GetComplaintByIdAsync(int complaintId)
        {
            var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(complaintId);
            if (complaint == null)
                throw new NotFoundException("Complaint not found");

            return _mapper.Map<ComplaintDto>(complaint);
        }

        public async Task ResolveComplaintAsync(ResolveComplaintDto dto, string adminId)
        {
            var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(dto.ComplaintId);
            if (complaint == null)
                throw new NotFoundException("Complaint not found");

            if (complaint.Status == ComplaintStatus.Resolved)
                throw new BadRequestException("Complaint is already resolved");

            complaint.Status = ComplaintStatus.Resolved;
            complaint.ReviewedBy = adminId;
            complaint.ReviewedAt = DateTime.UtcNow;
            complaint.Resolution = dto.Resolution;
            complaint.AdminNotes = dto.AdminNotes;

            _unitOfWork.Repository<Complaint>().Update(complaint);
            await _unitOfWork.CompleteAsync();

            // If admin decides to block user
            if (dto.BlockUser)
            {
                await BlockBusinessOwnerAsync(new BlockUserDto
                {
                    UserId = complaint.ReportedUserId,
                    Reason = $"Complaint resolved: {dto.Resolution}"
                }, adminId);
            }

            // Notify reporter
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = complaint.ReportedByUserId,
                Type = NotificationType.Support,
                Title = "Complaint Resolved",
                Message = $"Your complaint has been resolved. Resolution: {dto.Resolution}",
                Priority = NotificationPriority.Normal,
                SendEmail = true
            });

            // Check auto-block rules
            await CheckAndApplyAutoBlockRulesAsync(complaint.ReportedUserId);
        }

        public async Task RejectComplaintAsync(int complaintId, string reason, string adminId)
        {
            var complaint = await _unitOfWork.Repository<Complaint>().GetByIdAsync(complaintId);
            if (complaint == null)
                throw new NotFoundException("Complaint not found");

            complaint.Status = ComplaintStatus.Rejected;
            complaint.ReviewedBy = adminId;
            complaint.ReviewedAt = DateTime.UtcNow;
            complaint.Resolution = reason;

            _unitOfWork.Repository<Complaint>().Update(complaint);
            await _unitOfWork.CompleteAsync();

            // Notify reporter
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = complaint.ReportedByUserId,
                Type = NotificationType.Support,
                Title = "Complaint Rejected",
                Message = $"Your complaint was reviewed and rejected. Reason: {reason}",
                Priority = NotificationPriority.Normal,
                SendEmail = false
            });
        }

        // ═══════════════════════════════════════════════════════════
        // AUTO-BLOCK SYSTEM
        // ═══════════════════════════════════════════════════════════

        public async Task CheckAndApplyAutoBlockRulesAsync(string userId)
        {
            // Rule 1: 3 confirmed complaints
            var confirmedComplaintsSpec = new ComplaintsByUserSpecification(userId);
            var confirmedComplaints = await _unitOfWork.Repository<Complaint>()
                .GetCountWithSpecificationsAsync(confirmedComplaintsSpec);

            if (confirmedComplaints >= 3)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && !user.IsBlocked)
                {
                    // Auto-block user
                    user.IsBlocked = true;
                    user.BlockedAt = DateTime.UtcNow;
                    user.BlockedBy = "SYSTEM";
                    user.BlockReason = "Auto-blocked: 3 or more confirmed complaints";

                    await _userManager.UpdateAsync(user);

                    // Log auto-block
                    var autoBlockLog = new AutoBlockLog
                    {
                        UserId = userId,
                        Reason = "3 confirmed complaints received",
                        BlockedAt = DateTime.UtcNow,
                        IsReviewed = false
                    };

                    _unitOfWork.Repository<AutoBlockLog>().Add(autoBlockLog);
                    await _unitOfWork.CompleteAsync();

                    // Notify admin
                    await NotifyAdminsOfAutoBlockAsync(userId, "3 confirmed complaints");
                }
            }

            // Rule 2: Inactive > 12 months (check separately via background job)
            // Rule 3: Fake/duplicate accounts (requires manual flagging)
        }

        public async Task<List<AutoBlockLog>> GetPendingAutoBlocksAsync()
        {
            var spec = new PendingAutoBlocksSpecification();
            var logs = await _unitOfWork.Repository<AutoBlockLog>()
                .GetAllWithSpecificationsAsync(spec);

            return logs.ToList();
        }

        public async Task ReviewAutoBlockAsync(AutoBlockReviewDto dto, string adminId)
        {
            var log = await _unitOfWork.Repository<AutoBlockLog>().GetByIdAsync(dto.AutoBlockLogId);
            if (log == null)
                throw new NotFoundException("Auto-block log not found");

            if (log.IsReviewed)
                throw new BadRequestException("This auto-block has already been reviewed");

            log.IsReviewed = true;
            log.ReviewedBy = adminId;
            log.ReviewedAt = DateTime.UtcNow;
            log.AdminDecision = dto.Decision;
            log.AdminNotes = dto.AdminNotes;

            _unitOfWork.Repository<AutoBlockLog>().Update(log);
            await _unitOfWork.CompleteAsync();

            // Apply decision
            switch (dto.Decision.ToLower())
            {
                case "maintain":
                    // Keep block as is
                    await LogUserActionAsync(log.UserId, adminId, "Auto-Block Maintained",
                        "Admin reviewed and maintained auto-block", dto.AdminNotes);
                    break;

                case "warn":
                    // Unblock but warn user
                    await UnblockBusinessOwnerAsync(log.UserId, adminId);
                    await LogUserActionAsync(log.UserId, adminId, "Warning Issued",
                        "Auto-block reviewed - user warned", dto.AdminNotes);
                    break;

                case "unblock":
                    // Remove block
                    await UnblockBusinessOwnerAsync(log.UserId, adminId);
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // USER ACTION LOGS
        // ═══════════════════════════════════════════════════════════

        public async Task<List<UserActionLogDto>> GetUserActionLogsAsync(string userId)
        {
            var spec = new UserActionLogsSpecification(userId);
            var logs = await _unitOfWork.Repository<UserActionLog>()
                .GetAllWithSpecificationsAsync(spec);

            return _mapper.Map<List<UserActionLogDto>>(logs.ToList());
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        private async Task LogUserActionAsync(string userId, string adminId, string action, string reason, string? notes)
        {
            var log = new UserActionLog
            {
                UserId = userId,
                AdminId = adminId,
                Action = action,
                Reason = reason,
                Notes = notes,
                ActionDate = DateTime.UtcNow
            };

            _unitOfWork.Repository<UserActionLog>().Add(log);
            await _unitOfWork.CompleteAsync();
        }

        private async Task<int> GetUserProductCountAsync(string userId)
        {
            return await _unitOfWork.Repository<TalentreeProduct>()
                .CountAsync(p => p.CreatedBy == userId && !p.IsDeleted);
        }

        private async Task<int> GetUserOrderCountAsync(string userId)
        {
            return await _unitOfWork.Repository<MaterialOrder>()
                .CountAsync(o => o.BusinessOwnerId == userId);
        }

        private async Task<decimal> GetUserTotalRevenueAsync(string userId)
        {
            var orders = await _unitOfWork.Repository<MaterialOrder>()
                .FindAsync(o => o.BusinessOwnerId == userId &&
                                o.Status == MaterialOrderStatus.Delivered);

            return orders.Sum(o => o.TotalAmount);
        }

        private async Task<int> GetUserComplaintCountAsync(string userId)
        {
            return await _unitOfWork.Repository<Complaint>()
                .CountAsync(c => c.ReportedUserId == userId);
        }

        private async Task<int> GetCustomerOrderCountAsync(string userId)
        {
            // TODO: Implement when Customer Order entity is available
            // Customers don't place MaterialOrders, they place different orders
            return 0;
        }

        private async Task<decimal> GetCustomerTotalSpentAsync(string userId)
        {
            // TODO: Implement when Customer Order entity is available
            return 0;
        }

        private async Task<int> GetCustomerReviewCountAsync(string userId)
        {
            // TODO: Implement when Review entity is created
            // For now return 0
            return 0;
        }

        private async Task HideUserProductsAsync(string userId)
        {
            var products = await _unitOfWork.Repository<TalentreeProduct>()
                .FindAsync(p => p.CreatedBy == userId);

            foreach (var product in products)
            {
                product.IsVisible = false;
                _unitOfWork.Repository<TalentreeProduct>().Update(product);
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task RestoreUserProductsAsync(string userId)
        {
            var products = await _unitOfWork.Repository<TalentreeProduct>()
                .FindAsync(p => p.CreatedBy == userId && p.Status == ProductStatus.Approved);

            foreach (var product in products)
            {
                product.IsVisible = true;
                _unitOfWork.Repository<TalentreeProduct>().Update(product);
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task PermanentlyHideUserProductsAsync(string userId)
        {
            var products = await _unitOfWork.Repository<TalentreeProduct>()
                .FindAsync(p => p.CreatedBy == userId);

            foreach (var product in products)
            {
                product.IsVisible = false;
                product.IsDeleted = true;
                product.DeletedAt = DateTime.UtcNow;
                product.DeletedBy = "SYSTEM";
                _unitOfWork.Repository<TalentreeProduct>().Update(product);
            }

            await _unitOfWork.CompleteAsync();
        }

        private async Task NotifyAdminsOfAutoBlockAsync(string userId, string reason)
        {
            // TODO: Get all admin IDs from AspNetUserRoles
            var adminIds = new List<string>();

            foreach (var adminId in adminIds)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = adminId,
                    Type = NotificationType.System,
                    Title = "Auto-Block Applied",
                    Message = $"User {userId} has been auto-blocked. Reason: {reason}. Please review.",
                    ActionUrl = $"/admin/users/auto-blocks",
                    Priority = NotificationPriority.High,
                    SendEmail = false
                });
            }
        }
    }
}