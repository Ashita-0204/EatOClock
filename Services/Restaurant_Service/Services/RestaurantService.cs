using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Restaurant_Service.Data;
using Restaurant_Service.DTOs;
using Restaurant_Service.Interfaces;
using Restaurant_Service.Models;

namespace Restaurant_Service.Services;

public class RestaurantService : IRestaurantService
{
    private readonly AppDbContext _context;

    public RestaurantService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RestaurantDTO?> CreateRestaurantAsync(CreateRestaurantDTO dto, string ownerId)
    {
        var restaurant = new Restaurant
        {
            Name = dto.Name,
            Description = dto.Description,
            Cuisine = dto.Cuisine,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ImageUrl = dto.ImageUrl,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            OpeningTime = dto.OpeningTime,
            ClosingTime = dto.ClosingTime,
            OwnerId = ownerId,
            IsApproved = false, // Require Admin approval
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        return MapToDTO(restaurant);
    }

    public async Task<RestaurantDTO?> GetRestaurantByIdAsync(Guid id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        return restaurant != null ? MapToDTO(restaurant) : null;
    }

    public async Task<IEnumerable<RestaurantDTO>> GetAllRestaurantsAsync(bool includeUnapproved = false)
    {
        var query = _context.Restaurants.Where(r => r.IsActive);

        if (!includeUnapproved)
            query = query.Where(r => r.IsApproved);

        var restaurants = await query.ToListAsync();
        return restaurants.Select(MapToDTO);
    }

    public async Task<IEnumerable<RestaurantDTO>> SearchRestaurantsAsync(SearchRestaurantDTO dto)
    {
        var query = _context.Restaurants.Where(r => r.IsActive);

        if (dto.IsApproved.HasValue)
            query = query.Where(r => r.IsApproved == dto.IsApproved.Value);
        else
            query = query.Where(r => r.IsApproved);

        if (!string.IsNullOrWhiteSpace(dto.Cuisine))
            query = query.Where(r => r.Cuisine.ToLower() == dto.Cuisine.ToLower());

        if (dto.MinRating.HasValue)
            query = query.Where(r => r.Rating >= dto.MinRating.Value);

        if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
        {
            var term = dto.SearchTerm.ToLower();
            query = query.Where(r => 
                r.Name.ToLower().Contains(term) || 
                r.Description.ToLower().Contains(term) ||
                r.Address.ToLower().Contains(term));
        }

        var restaurants = await query.ToListAsync();
        return restaurants.Select(MapToDTO);
    }

    public async Task<IEnumerable<RestaurantDTO>> GetNearbyRestaurantsAsync(NearbySearchDTO dto)
    {
        var allRestaurants = await _context.Restaurants
            .Where(r => r.IsActive && r.IsApproved)
            .ToListAsync();

        var nearbyRestaurants = allRestaurants
            .Select(r => new
            {
                Restaurant = r,
                Distance = CalculateDistance(dto.Latitude, dto.Longitude, r.Latitude, r.Longitude)
            })
            .Where(x => x.Distance <= dto.RadiusKm)
            .OrderBy(x => x.Distance)
            .ToList();

        if (!string.IsNullOrWhiteSpace(dto.Cuisine))
            nearbyRestaurants = nearbyRestaurants
                .Where(x => x.Restaurant.Cuisine.ToLower() == dto.Cuisine.ToLower())
                .ToList();

        if (dto.MinRating.HasValue)
            nearbyRestaurants = nearbyRestaurants
                .Where(x => x.Restaurant.Rating >= dto.MinRating.Value)
                .ToList();

        return nearbyRestaurants.Select(x =>
        {
            var restaurantDTO = MapToDTO(x.Restaurant);
            restaurantDTO.Distance = Math.Round(x.Distance, 2);
            return restaurantDTO;
        });
    }

    public async Task<RestaurantDTO?> UpdateRestaurantAsync(Guid id, UpdateRestaurantDTO dto, string userId)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null || restaurant.OwnerId != userId)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            restaurant.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            restaurant.Description = dto.Description;

        if (!string.IsNullOrWhiteSpace(dto.Cuisine))
            restaurant.Cuisine = dto.Cuisine;

        if (!string.IsNullOrWhiteSpace(dto.Address))
            restaurant.Address = dto.Address;

        if (dto.Latitude.HasValue)
            restaurant.Latitude = dto.Latitude.Value;

        if (dto.Longitude.HasValue)
            restaurant.Longitude = dto.Longitude.Value;

        if (dto.ImageUrl != null)
            restaurant.ImageUrl = dto.ImageUrl;

        if (dto.PhoneNumber != null)
            restaurant.PhoneNumber = dto.PhoneNumber;

        if (dto.Email != null)
            restaurant.Email = dto.Email;

        if (dto.OpeningTime.HasValue)
            restaurant.OpeningTime = dto.OpeningTime;

        if (dto.ClosingTime.HasValue)
            restaurant.ClosingTime = dto.ClosingTime;

        restaurant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDTO(restaurant);
    }

    public async Task<bool> DeleteRestaurantAsync(Guid id, string userId)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null || restaurant.OwnerId != userId)
            return false;

        restaurant.IsActive = false;
        restaurant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveRestaurantAsync(Guid id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
            return false;

        restaurant.IsApproved = true;
        restaurant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectRestaurantAsync(Guid id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
            return false;

        restaurant.IsApproved = false;
        restaurant.IsActive = false;
        restaurant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<RestaurantDTO>> GetRestaurantsByOwnerAsync(string ownerId)
    {
        var restaurants = await _context.Restaurants
            .Where(r => r.OwnerId == ownerId && r.IsActive)
            .ToListAsync();

        return restaurants.Select(MapToDTO);
    }

    // Haversine formula for calculating distance between two GPS coordinates
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private static RestaurantDTO MapToDTO(Restaurant restaurant)
    {
        return new RestaurantDTO
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Description = restaurant.Description,
            Cuisine = restaurant.Cuisine,
            Address = restaurant.Address,
            Latitude = restaurant.Latitude,
            Longitude = restaurant.Longitude,
            ImageUrl = restaurant.ImageUrl,
            Rating = restaurant.Rating,
            ReviewCount = restaurant.ReviewCount,
            IsApproved = restaurant.IsApproved,
            IsActive = restaurant.IsActive,
            PhoneNumber = restaurant.PhoneNumber,
            Email = restaurant.Email,
            OpeningTime = restaurant.OpeningTime,
            ClosingTime = restaurant.ClosingTime
        };
    }
}
