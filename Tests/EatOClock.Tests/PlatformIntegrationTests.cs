using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Restaurant_Service.Data;
using Restaurant_Service.Services;
using Restaurant_Service.DTOs;
using Moq;
using Order_Service.Services;
using Order_Service.Models;
using Microsoft.AspNetCore.Http;
using Menu_Service.Services;

namespace EatOClock.Tests;

// =====================================================
// PLATFORM INTEGRATION TESTS
// Covers Restaurant Service and Order Service workflows
// =====================================================

[TestFixture]
public class PlatformIntegrationTests
{
    private Restaurant_Service.Data.AppDbContext _restaurantDb;
    private RestaurantService _restaurantService;

    private Order_Service.Data.AppDbContext _orderDb;
    private OrderServiceImpl _orderService;

    [SetUp]
    public void Setup()
    {
        // Create isolated in-memory DB for Restaurant Service
        var restOptions = new DbContextOptionsBuilder<Restaurant_Service.Data.AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _restaurantDb = new Restaurant_Service.Data.AppDbContext(restOptions);
        _restaurantService = new RestaurantService(_restaurantDb);

        // Create isolated in-memory DB for Order Service
        var orderOptions = new DbContextOptionsBuilder<Order_Service.Data.AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _orderDb = new Order_Service.Data.AppDbContext(orderOptions);

        // Mock external dependencies
        var mockHttpFactory = new Mock<IHttpClientFactory>();
        var mockAccessor = new Mock<IHttpContextAccessor>();

        _orderService = new OrderServiceImpl(
            _orderDb,
            mockHttpFactory.Object,
            mockAccessor.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _restaurantDb.Dispose();
        _orderDb.Dispose();
    }

    // =====================================================
    // RESTAURANT SERVICE TESTS
    // =====================================================

    [Test]
    public async Task CreateRestaurant_ShouldReturnSuccess()
    {
        // Arrange
        var req = new CreateRestaurantDTO
        {
            Name = "Test Rest",
            Address = "123 Main",
            Cuisine = "Italian"
        };

        // Act
        var res = await _restaurantService.CreateRestaurantAsync(req, "user123");

        // Assert
        Assert.That(res, Is.Not.Null);
        Assert.That(res.Name, Is.EqualTo("Test Rest"));
    }

    [Test]
    public async Task GetAllRestaurants_ShouldReturnList()
    {
        // Arrange
        var req = new CreateRestaurantDTO
        {
            Name = "Test Rest",
            Address = "123 Main",
            Cuisine = "Italian"
        };

        await _restaurantService.CreateRestaurantAsync(req, "user123");

        // Act
        var res = await _restaurantService.GetAllRestaurantsAsync(true);

        // Assert
        Assert.That(res, Is.Not.Empty);
    }

    [Test]
    public async Task UpdateRestaurantStatus_ShouldChangeStatus()
    {
        // Arrange
        var req = new CreateRestaurantDTO { Name = "Test Rest" };

        var createRes =
            await _restaurantService.CreateRestaurantAsync(req, "user123");

        // Act
        var res = await _restaurantService.RejectRestaurantAsync(createRes.Id);

        // Assert
        Assert.That(res, Is.True);

        var rest =
            await _restaurantService.GetRestaurantByIdAsync(createRes.Id);

        Assert.That(rest.IsApproved, Is.False);
    }

    [Test]
    public async Task GetRestaurantById_ShouldReturnCorrectRestaurant()
    {
        // Arrange
        var req = new CreateRestaurantDTO { Name = "Target Rest" };

        var createRes =
            await _restaurantService.CreateRestaurantAsync(req, "user123");

        // Act
        var res =
            await _restaurantService.GetRestaurantByIdAsync(createRes.Id);

        // Assert
        Assert.That(res, Is.Not.Null);
        Assert.That(res.Name, Is.EqualTo("Target Rest"));
    }

    [Test]
    public async Task GetMyRestaurants_ShouldReturnOnlyOwnersRestaurants()
    {
        // Arrange
        await _restaurantService.CreateRestaurantAsync(
            new CreateRestaurantDTO { Name = "Rest A" },
            "ownerA");

        await _restaurantService.CreateRestaurantAsync(
            new CreateRestaurantDTO { Name = "Rest B" },
            "ownerB");

        // Act
        var res =
            await _restaurantService.GetRestaurantsByOwnerAsync("ownerA");

        // Assert
        Assert.That(res, Is.Not.Empty);
        Assert.That(res.First().Name, Is.EqualTo("Rest A"));
    }

    // =====================================================
    // ORDER SERVICE TESTS
    // =====================================================

    [Test]
    public async Task PlaceOrder_ShouldCreateOrder()
    {
        // Arrange
        var req = new Order_Service.DTOs.PlaceOrderRequest(
            Guid.NewGuid(),
            "COD",
            "123 Street",
            "Notes",
            null,
            new List<Order_Service.DTOs.OrderItemRequest>
            {
                new(
                    Guid.NewGuid(),
                    "Pizza",
                    10.0m,
                    2,
                    null)
            });

        // Act
        var res = await _orderService.PlaceOrderAsync("cust123", req);

        // Assert
        Assert.That(res, Is.Not.Null);
        Assert.That(res.FinalAmount, Is.EqualTo(20.0m));
        Assert.That(res.Status, Is.EqualTo("PLACED"));
    }

    [Test]
    public void PlaceOrder_WithoutItems_ShouldThrowException()
    {
        // Arrange
        var req = new Order_Service.DTOs.PlaceOrderRequest(
            Guid.NewGuid(),
            "COD",
            "123 Street",
            "Notes",
            null,
            new List<Order_Service.DTOs.OrderItemRequest>());

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orderService.PlaceOrderAsync("cust123", req));
    }

    [Test]
    public async Task CancelOrder_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var req = new Order_Service.DTOs.PlaceOrderRequest(
            Guid.NewGuid(),
            "COD",
            "Address",
            null,
            null,
            new List<Order_Service.DTOs.OrderItemRequest>
            {
                new(Guid.NewGuid(), "Item", 10m, 1, null)
            });

        var order = await _orderService.PlaceOrderAsync("cust123", req);

        // Act
        var res =
            await _orderService.CancelOrderAsync(order.OrderId, "cust123");

        // Assert
        Assert.That(res.Status, Is.EqualTo("CANCELLED"));
    }

    // =====================================================
    // MENU SERVICE TESTS
    // =====================================================
}

[TestFixture]
public class MenuServiceTests
{
    private Menu_Service.Data.AppDbContext _menuDb;
    private Menu_Service.Services.MenuService _menuService;

    [SetUp]
    public void Setup()
    {
        // Create isolated in-memory DB for menu testing
        var options = new DbContextOptionsBuilder<Menu_Service.Data.AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _menuDb = new Menu_Service.Data.AppDbContext(options);
        _menuService = new Menu_Service.Services.MenuService(_menuDb);
    }

    [TearDown]
    public void TearDown()
    {
        _menuDb.Dispose();
    }

    // =====================================================
    // CATEGORY TESTS
    // =====================================================

    [Test]
    public async Task CreateCategory_ShouldPersistAndReturnCategory()
    {
        // Arrange
        var restId = Guid.NewGuid();

        var req = new Menu_Service.DTOs.CreateCategoryRequest(
            restId,
            "Starters",
            "All starters",
            1);

        // Act
        var result =
            await _menuService.CreateCategoryAsync(req, "owner1");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Starters"));
        Assert.That(result.RestaurantId, Is.EqualTo(restId));
    }

    [Test]
    public async Task GetCategoriesByRestaurant_ShouldReturnOnlyMatchingRestaurant()
    {
        // Arrange
        var restId = Guid.NewGuid();

        await _menuService.CreateCategoryAsync(
            new Menu_Service.DTOs.CreateCategoryRequest(
                restId,
                "Mains",
                "Main course",
                1),
            "owner1");

        // Act
        var result =
            await _menuService.GetCategoriesByRestaurantAsync(restId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Mains"));
    }

    // =====================================================
    // MENU ITEM TESTS
    // =====================================================

    [Test]
    public async Task CreateItem_ShouldPersistAndReturnItem()
    {
        // Arrange
        var restId = Guid.NewGuid();

        var category =
            await _menuService.CreateCategoryAsync(
                new Menu_Service.DTOs.CreateCategoryRequest(
                    restId,
                    "Mains",
                    "",
                    1),
                "owner1");

        var itemReq =
            new Menu_Service.DTOs.CreateMenuItemRequest(
                category.Id,
                restId,
                "Butter Chicken",
                "Creamy curry",
                12.99m,
                null,
                false);

        // Act
        var result =
            await _menuService.CreateItemAsync(itemReq, "owner1");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Butter Chicken"));
        Assert.That(result.Price, Is.EqualTo(12.99m));
    }
}