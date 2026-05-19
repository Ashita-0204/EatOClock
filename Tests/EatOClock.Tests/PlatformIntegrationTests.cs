using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Auth_Service.DTOs;
using Auth_Service.Enums;
using DeliveryAgent_Service.DTOs;
using DeliveryAgent_Service.Models;
using Review_Service.DTOs;

namespace EatOClock.Tests;

[TestFixture]
public class PlatformIntegrationTests
{
    // --- AUTH SERVICE DTO TESTS ---

    // Test Cases 1, 2, 3: RegisterDTO GetRole mapping for the allowed roles
    [Test]
    [TestCase(1, AllowedRegistrationRole.Customer)]
    [TestCase(2, AllowedRegistrationRole.RestaurantOwner)]
    [TestCase(3, AllowedRegistrationRole.DeliveryAgent)]
    public void RegisterDTO_GetRole_ShouldMapCorrectly(int inputRole, AllowedRegistrationRole expectedEnum)
    {
        // Arrange
        var dto = new RegisterDTO { Role = inputRole };

        // Act
        var result = dto.GetRole();

        // Assert
        Assert.That(result, Is.EqualTo(expectedEnum));
    }

    // Test Case 4: RegisterDTO validation fails with invalid role assignment
    [Test]
    public void RegisterDTO_Validation_ShouldFail_WhenRoleIsInvalid()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = "test@eatoclock.com",
            Password = "password123",
            FullName = "John Doe",
            Role = 99 // Invalid role (out of range 1-3)
        };

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results[0].ErrorMessage, Does.Contain("Role must be 1 (Customer), 2 (RestaurantOwner), or 3 (DeliveryAgent)."));
    }

    // Test Case 5: RegisterDTO validation succeeds with correct payload
    [Test]
    public void RegisterDTO_Validation_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange
        var dto = new RegisterDTO
        {
            Email = "test@eatoclock.com",
            Password = "password123",
            FullName = "John Doe",
            Role = 1, // Valid Customer role
            PhoneNumber = "1234567890"
        };

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(results, Is.Empty);
    }

    // --- DELIVERY AGENT SERVICE REQUEST TESTS ---

    // Test Case 6: RegisterAgentRequest constructor initializes all properties properly
    [Test]
    public void RegisterAgentRequest_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var fullName = "Speedy Rider";
        var phone = "9876543210";
        var email = "rider@delivery.com";
        var vehicleType = VehicleType.Scooter;
        var vehicleNumber = "AB-12-CD-3456";

        // Act
        var req = new RegisterAgentRequest(fullName, phone, email, vehicleType, vehicleNumber);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(req.FullName, Is.EqualTo(fullName));
            Assert.That(req.Phone, Is.EqualTo(phone));
            Assert.That(req.Email, Is.EqualTo(email));
            Assert.That(req.VehicleType, Is.EqualTo(vehicleType));
            Assert.That(req.VehicleNumber, Is.EqualTo(vehicleNumber));
        });
    }

    // Test Case 7: UpdateLocationRequest constructor initializes properties properly
    [Test]
    public void UpdateLocationRequest_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var lat = 12.9716;
        var lng = 77.5946;

        // Act
        var req = new UpdateLocationRequest(lat, lng);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(req.Latitude, Is.EqualTo(lat));
            Assert.That(req.Longitude, Is.EqualTo(lng));
        });
    }

    // --- REVIEW SERVICE DTO TESTS ---

    // Test Case 8: SubmitReviewDTO constructor initializes properties properly
    [Test]
    public void SubmitReviewDTO_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var foodRating = 4;
        var deliveryRating = 5;
        var comment = "Great food, fast delivery!";

        // Act
        var dto = new SubmitReviewDTO(orderId, restaurantId, agentId, foodRating, deliveryRating, comment);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.OrderId, Is.EqualTo(orderId));
            Assert.That(dto.RestaurantId, Is.EqualTo(restaurantId));
            Assert.That(dto.AgentId, Is.EqualTo(agentId));
            Assert.That(dto.FoodRating, Is.EqualTo(foodRating));
            Assert.That(dto.DeliveryRating, Is.EqualTo(deliveryRating));
            Assert.That(dto.Comment, Is.EqualTo(comment));
        });
    }

    // Test Case 9: SubmitReviewDTO validation fails when ratings are out of bounds
    [Test]
    public void SubmitReviewDTO_Validation_ShouldFail_WhenRatingsAreOutOfRange()
    {
        // Arrange
        var dto = new SubmitReviewDTO
        {
            OrderId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            FoodRating = 6,        // Out of range [1, 5]
            DeliveryRating = 0,    // Out of range [1, 5]
            Comment = "Bad rating boundaries"
        };

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(2)); // Both ratings should trigger errors
    }

    // Test Case 10: SubmitReviewDTO validation succeeds with correct payload values
    [Test]
    public void SubmitReviewDTO_Validation_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange
        var dto = new SubmitReviewDTO
        {
            OrderId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            FoodRating = 5,
            DeliveryRating = 5,
            Comment = "Excellent service"
        };

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(results, Is.Empty);
    }

    // Test Case 11: EditReviewDTO constructor initializes properties properly
    [Test]
    public void EditReviewDTO_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var foodRating = 3;
        var deliveryRating = 4;
        var comment = "Revised review details";

        // Act
        var dto = new EditReviewDTO(foodRating, deliveryRating, comment);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.FoodRating, Is.EqualTo(foodRating));
            Assert.That(dto.DeliveryRating, Is.EqualTo(deliveryRating));
            Assert.That(dto.Comment, Is.EqualTo(comment));
        });
    }

    // Test Case 12: EditReviewDTO validation fails when ratings are out of bounds
    [Test]
    public void EditReviewDTO_Validation_ShouldFail_WhenRatingsAreOutOfRange()
    {
        // Arrange
        var dto = new EditReviewDTO
        {
            FoodRating = -1,      // Out of range [1, 5]
            DeliveryRating = 10,   // Out of range [1, 5]
            Comment = "Updated but invalid"
        };

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(dto, context, results, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
    }
}
