using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Basket;

namespace Talentree.Core.Service.Contract
{
    public interface IMaterialBasketService
    {
        /// <summary>Gets the BO's basket, creates one if it doesn't exist</summary>
        Task<MaterialBasketDto> GetBasketAsync(string businessOwnerId);

        /// <summary>Adds a material to basket — validates MOQ and stock</summary>
        Task<MaterialBasketDto> AddItemAsync(string businessOwnerId, AddToBasketDto dto);

        /// <summary>Updates quantity of an existing basket item</summary>
        Task<MaterialBasketDto> UpdateItemAsync(string businessOwnerId, int itemId, UpdateBasketItemDto dto);

        /// <summary>Removes a single item from the basket</summary>
        Task<MaterialBasketDto> RemoveItemAsync(string businessOwnerId, int itemId);

        /// <summary>Clears all items from the basket</summary>
        Task ClearBasketAsync(string businessOwnerId);
    }
}
