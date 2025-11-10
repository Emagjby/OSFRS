
using Moq;
using OSFRS.Backend.DTOs;
using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Services;
using OSFRS.Models.Entities;

namespace OSFRS.Tests.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _mockRepo;
    private readonly Mock<IAppLogger<ReservationService>> _mockLogger;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _mockRepo = new Mock<IReservationRepository>();
        _mockLogger = new Mock<IAppLogger<ReservationService>>();
        _service = new ReservationService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldCreate_WhenValid()
    {
        var reservation = new Reservation
        {
            Id = 1,
            FacilityId = 10,
            UserId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        _mockRepo.Setup(r => r.IsSlotAvailableAsync(reservation.StartTime, reservation.EndTime, reservation.FacilityId))
            .ReturnsAsync(true);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>())).ReturnsAsync(reservation);

        var result = await _service.CreateReservationAsync(new CreateReservationDto
        {
            FacilityId = reservation.FacilityId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        }, reservation.UserId);

        Assert.NotNull(result);
        Assert.Equal(reservation.FacilityId, result.FacilityId);
        Assert.Equal(reservation.UserId, result.UserId);
        Assert.Equal(reservation.StartTime, result.StartTime);
        Assert.Equal(reservation.EndTime, result.EndTime);
        _mockRepo.Verify(r => r.AddAsync(It.Is<Reservation>(x =>
            x.FacilityId == reservation.FacilityId &&
            x.UserId == reservation.UserId &&
            x.StartTime == reservation.StartTime &&
            x.EndTime == reservation.EndTime
        )), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrow_WhenSlotNotAvailable()
    {
        var reservation = new Reservation
        {
            Id = 2,
            FacilityId = 20,
            UserId = 2,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3)
        };

        _mockRepo.Setup(r => r.IsSlotAvailableAsync(reservation.StartTime, reservation.EndTime, reservation.FacilityId))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateReservationAsync(new CreateReservationDto
        {
            FacilityId = reservation.FacilityId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        }, reservation.UserId));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
        // The method logs the creation attempt before checking availability.
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrow_WhenInvalidTimes()
    {
        var reservation = new Reservation
        {
            Id = 3,
            FacilityId = 30,
            UserId = 3,
            StartTime = DateTime.UtcNow.AddHours(4),
            EndTime = DateTime.UtcNow.AddHours(3) // End before Start
        };
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateReservationAsync(new CreateReservationDto
        {
            FacilityId = reservation.FacilityId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        }, reservation.UserId));
        _mockRepo.Verify(r => r.IsSlotAvailableAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldUpdate_WhenValid()
    {
        var reservation = new Reservation
        {
            Id = 4,
            FacilityId = 40,
            UserId = 321,
            StartTime = DateTime.UtcNow.AddHours(5),
            EndTime = DateTime.UtcNow.AddHours(6)
        };

        var updateDto = new UpdateReservationDto
        {
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };

        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);
        _mockRepo.Setup(r => r.IsSlotAvailableAsync(updateDto.StartTime, updateDto.EndTime, reservation.FacilityId))
            .ReturnsAsync(true);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).ReturnsAsync(new Reservation
        {
            Id = reservation.Id,
            FacilityId = reservation.FacilityId,
            UserId = reservation.UserId,
            StartTime = updateDto.StartTime,
            EndTime = updateDto.EndTime
        });

        var updated = await _service.UpdateReservationAsync(reservation.Id, updateDto, reservation.UserId);
        Assert.NotNull(updated);
        Assert.Equal(reservation.Id, updated.Id);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateReservationAsync_ShouldThrow_WhenUnauthorizedUser()
    {
        var reservation = new Reservation
        {
            Id = 5,
            FacilityId = 50,
            UserId = 999,
            StartTime = DateTime.UtcNow.AddHours(7),
            EndTime = DateTime.UtcNow.AddHours(8)
        };

        var updateDto = new UpdateReservationDto
        {
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };

        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateReservationAsync(reservation.Id, updateDto, 123));
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
        _mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldCancel_WhenValidUser()
    {
        var reservation = new Reservation
        {
            Id = 6,
            FacilityId = 60,
            UserId = 111,
            StartTime = DateTime.UtcNow.AddHours(9),
            EndTime = DateTime.UtcNow.AddHours(10),
            Status = "Pending"
        };
        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);
        _mockRepo.Setup(r => r.UpdateStatusAsync(reservation.Id, "Cancelled"))
            .Callback(() => reservation.Status = "Cancelled")
            .ReturnsAsync(new Reservation
            {
                Id = reservation.Id,
                FacilityId = reservation.FacilityId,
                UserId = reservation.UserId,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = "Cancelled"
            });

        await _service.CancelReservationAsync(reservation.Id, reservation.UserId);

        // After cancellation, setup the mock to return the updated reservation
        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);
        var updatedReservation = await _mockRepo.Object.GetReservationByIdAsync(reservation.Id);
        Assert.Equal("Cancelled", updatedReservation!.Status);
        _mockRepo.Verify(r => r.UpdateStatusAsync(reservation.Id, "Cancelled"), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CancelReservationAsync_ShouldThrow_WhenNotOwner()
    {
        var reservation = new Reservation
        {
            Id = 7,
            FacilityId = 70,
            UserId = 222,
            StartTime = DateTime.UtcNow.AddHours(11),
            EndTime = DateTime.UtcNow.AddHours(12),
            Status = "Approved"
        };
        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CancelReservationAsync(reservation.Id, 333));
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
        _mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetReservationsAsync_ShouldReturnFilteredByFacility()
    {
        var reservations = new List<Reservation>
        {
            new Reservation { Id = 8, FacilityId = 100, UserId = 1 },
            new Reservation { Id = 9, FacilityId = 101, UserId = 2 }
        };
        _mockRepo.Setup(r => r.GetByFacilityAndRangeAsync(100, null, null)).ReturnsAsync(reservations.Where(x => x.FacilityId == 100).ToList());

        var result = await _service.GetReservationsAsync(100);
        Assert.Single(result);
        Assert.All(result, r => Assert.Equal(100, r.FacilityId));
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetAvailabilityCalendarAsync_ShouldReturnAvailableSlots()
    {
        var facilityId = 200;
        var date = DateTime.UtcNow.Date;
        // Booked reservations for the day, e.g. 8-9 and 9-10
        var reservations = new List<Reservation>
        {
            new Reservation { FacilityId = facilityId, StartTime = date.AddHours(8), EndTime = date.AddHours(9) },
            new Reservation { FacilityId = facilityId, StartTime = date.AddHours(9), EndTime = date.AddHours(10) }
        };
        _mockRepo.Setup(r => r.GetByFacilityAndRangeAsync(facilityId, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(reservations);

        var result = await _service.GetAvailabilityCalendarAsync(facilityId, date);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(reservations[0].StartTime, list[0].StartTime);
        Assert.Equal(reservations[1].StartTime, list[1].StartTime);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetAvailabilityCalendarAsync_ShouldReturnEmpty_WhenNoSlots()
    {
        var facilityId = 201;
        var date = DateTime.UtcNow.Date;
        _mockRepo.Setup(r => r.GetByFacilityAndRangeAsync(facilityId, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<Reservation>());

        var result = await _service.GetAvailabilityCalendarAsync(facilityId, date);
        Assert.Empty(result);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SearchReservationAsync_ShouldReturnExpectedResults()
    {
        var reservations = new List<Reservation>
        {
            new Reservation { Id = 10, FacilityId = 300, UserId = 400 }
        };
        _mockRepo.Setup(r => r.SearchAsync(400, 300, null, null)).ReturnsAsync(reservations);

        var result = await _service.SearchReservationAsync(400, 300);
        var list = result.ToList();

        Assert.Single(result);
        Assert.Equal(300, list[0].FacilityId);
        Assert.Equal(400, list[0].UserId);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetAllReservationsAsync_ShouldReturnAll()
    {
        var reservations = new List<Reservation>
        {
            new Reservation { Id = 11, FacilityId = 400, UserId = 1 },
            new Reservation { Id = 12, FacilityId = 401, UserId = 2 }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reservations);

        var result = await _service.GetAllReservationsAsync();
        var count = result.ToList().Count();

        Assert.Equal(2, count);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteReservationAsync_ShouldDelete_WhenAdmin()
    {
        var reservation = new Reservation { Id = 13, FacilityId = 500, UserId = 1 };
        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);
        _mockRepo.Setup(r => r.DeleteAsync(reservation.Id)).Returns(Task.FromResult(true));

        await _service.DeleteReservationAsync(reservation.Id, 1);
        _mockRepo.Verify(r => r.DeleteAsync(reservation.Id), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteReservationAsync_ShouldThrow_WhenUnauthorized()
    {
        var reservation = new Reservation { Id = 14, FacilityId = 501, UserId = 2 };
        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteReservationAsync(reservation.Id, 2));
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        _mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AdminUpdateReservationAsync_ShouldUpdateSuccessfully()
    {
        var reservation = new Reservation
        {
            Id = 15,
            FacilityId = 600,
            UserId = 1,
            StartTime = DateTime.UtcNow.AddHours(13),
            EndTime = DateTime.UtcNow.AddHours(14)
        };

        var updateDto = new UpdateReservationDto
        {
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime
        };

        _mockRepo.Setup(r => r.GetReservationByIdAsync(reservation.Id)).ReturnsAsync(reservation);
        _mockRepo.Setup(r => r.IsSlotAvailableAsync(updateDto.StartTime, updateDto.EndTime, reservation.FacilityId))
            .ReturnsAsync(true);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).ReturnsAsync(new Reservation
        {
            Id = reservation.Id,
            FacilityId = reservation.FacilityId,
            UserId = reservation.UserId,
            StartTime = updateDto.StartTime,
            EndTime = updateDto.EndTime
        });

        var adminId = 99;
        var updated = await _service.AdminUpdateReservationAsync(reservation.Id, updateDto, adminId);
        Assert.NotNull(updated);
        Assert.Equal(reservation.Id, updated.Id);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Once);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }
}
