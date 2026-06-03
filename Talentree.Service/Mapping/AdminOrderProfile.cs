using AutoMapper;
using System.Linq;
using Talentree.Core.Entities;
using Talentree.Service.DTOs.Admin.Orders;
using Talentree.Service.DTOs.Admin.Transactions;
using Talentree.Service.DTOs.Refund;

namespace Talentree.Service.Mapping
{
    public class AdminOrderProfile : Profile
    {
        public AdminOrderProfile()
        {
            CreateMap<CustomerOrder, AdminOrderSummaryDto>()
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.DisplayName))
                .ForMember(d => d.CustomerEmail, o => o.MapFrom(s => s.Customer.Email))
                .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Sum(i => i.Quantity)))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.SellerNames, o => o.MapFrom(s => string.Join(", ", s.Items.Select(i => i.SellerName).Distinct())));

            CreateMap<CustomerOrder, AdminOrderDetailDto>()
                .IncludeBase<CustomerOrder, AdminOrderSummaryDto>()
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()));

            CreateMap<RefundRequest, RefundRequestDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Order.Customer.DisplayName))
                .ForMember(d => d.BusinessOwnerName, o => o.MapFrom(s => s.OrderItem.SellerName));

            CreateMap<Transaction, AdminTransactionDto>()
                .ForMember(d => d.BusinessOwnerName, o => o.MapFrom(s => s.BusinessOwnerId)) // Set to ID for now, service can fill name later
                .ForMember(d => d.BusinessOwnerEmail, o => o.Ignore());
        }
    }
}
