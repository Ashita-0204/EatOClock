using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Restaurant_Service.DTOs;
using Restaurant_Service.Models;

namespace Restaurant_Service.Interfaces;

public interface IRestaurantService
{
    Task<RestaurantDTO?> CreateRestaurantAsync(CreateRestaurantDTO dto, string ownerId);
    Task<RestaurantDTO?> GetRestaurantByIdAsync(Guid id);
    Task<IEnumerable<RestaurantDTO>> GetAllRestaurantsAsync(bool includeUnapproved = false);
    Task<IEnumerable<RestaurantDTO>> SearchRestaurantsAsync(SearchRestaurantDTO dto);
    Task<IEnumerable<RestaurantDTO>> GetNearbyRestaurantsAsync(NearbySearchDTO dto);
    Task<RestaurantDTO?> UpdateRestaurantAsync(Guid id, UpdateRestaurantDTO dto, string userId);
    Task<bool> DeleteRestaurantAsync(Guid id, string userId);
    Task<bool> ApproveRestaurantAsync(Guid id);
    Task<bool> RejectRestaurantAsync(Guid id);
    Task<IEnumerable<RestaurantDTO>> GetRestaurantsByOwnerAsync(string ownerId);
}
