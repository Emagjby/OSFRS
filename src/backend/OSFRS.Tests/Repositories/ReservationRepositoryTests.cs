using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OSFRS.Models.Entities;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Data;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Tests.Repositories;

public class ReservationRepositoryTests
{
    private readonly IReservationRepository _repo;
    private readonly OSFRSDbContext _context;
    private readonly Mock<IAppLogger<ReservationRepository>> _mockLogger;

    public ReservationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OSFRSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new OSFRSDbContext(options);
        _mockLogger = new Mock<IAppLogger<ReservationRepository>>();
        _repo = new ReservationRepository(_context, _mockLogger.Object);

        SeedData();
    }

    private void SeedData()
    {
        var user1 = new User { Id = 1, Name = "User1", Email = "user1@example.com", PasswordHash = "Demo", Username = "userone" };
        var user2 = new User { Id = 2, Name = "User2", Email = "user2@example.com", PasswordHash = "Demo", Username = "usertwo" };

        var reservations = new List<Reservation>
        {
            new Reservation
            {
                Id = 1,
                UserId = 1,
                FacilityId = 1,
                StartTime = new DateTime(2025, 6, 1, 9, 0, 0),
                EndTime = new DateTime(2025, 6, 1, 10, 0, 0),
                Status = "Approved"
            },
            new Reservation
            {
                Id = 2,
                UserId = 1,
                FacilityId = 2,
                StartTime = new DateTime(2025, 6, 2, 11, 0, 0),
                EndTime = new DateTime(2025, 6, 2, 12, 0, 0),
                Status = "Pending"
            },
            new Reservation
            {
                Id = 3,
                UserId = 2,
                FacilityId = 1,
                StartTime = new DateTime(2025, 6, 3, 14, 0, 0),
                EndTime = new DateTime(2025, 6, 3, 15, 0, 0),
                Status = "Cancelled"
            }
        };

        _context.Users.AddRange(user1, user2);
        _context.Reservations.AddRange(reservations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_ShouldAddReservation()
    {
        var reservation = new Reservation
        {
            UserId = 1,
            FacilityId = 1,
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2),
            Status = "Pending"
        };

        var added = await _repo.AddAsync(reservation);

        Assert.NotNull(added);
        Assert.True(added.Id > 0);
        var dbReservation = await _context.Reservations.FindAsync(added.Id);
        Assert.NotNull(dbReservation);
        Assert.Equal(reservation.UserId, dbReservation.UserId);

        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetReservationByIdAsync_ShouldReturnReservation()
    {
        var reservation = await _repo.GetReservationByIdAsync(1);
        Assert.NotNull(reservation);
        Assert.Equal(1, reservation.Id);
    }

    [Fact]
    public async Task GetByUserAsync_ShouldReturnUserReservations()
    {
        var reservations = await _repo.GetByUserAsync(1);
        Assert.NotNull(reservations);
        Assert.All(reservations, r => Assert.Equal(1, r.UserId));
        Assert.Equal(2, reservations.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllReservations()
    {
        var allReservations = await _repo.GetAllAsync();
        Assert.NotNull(allReservations);
        Assert.Equal(3, allReservations.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyReservation()
    {
        var reservation = await _repo.GetReservationByIdAsync(1);
        reservation!.Status = "Cancelled";

        var updated = await _repo.UpdateAsync(reservation);

        Assert.Equal("Cancelled", updated!.Status);
        var dbReservation = await _context.Reservations.FindAsync(1);
        Assert.Equal("Cancelled", dbReservation!.Status);

        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldChangeStatus()
    {
        var updated = await _repo.UpdateStatusAsync(2, "Approved");
        Assert.Equal("Approved", updated!.Status);
        var dbReservation = await _context.Reservations.FindAsync(2);
        Assert.Equal("Approved", dbReservation!.Status);

        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenReservationNotFound()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.UpdateStatusAsync(999, "Approved"));
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveReservation()
    {
        var deleted = await _repo.DeleteAsync(3);
        Assert.True(deleted);
        var dbReservation = await _context.Reservations.FindAsync(3);
        Assert.Null(dbReservation);

        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteAsync_ShouldLogWarning_WhenReservationNotFound()
    {
        var result = await _repo.DeleteAsync(999);
        Assert.False(result);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task IsSlotAvailableAsync_ShouldReturnTrue_WhenFree()
    {
        var start = new DateTime(2025, 6, 4, 9, 0, 0);
        var end = new DateTime(2025, 6, 4, 10, 0, 0);

        var isAvailable = await _repo.IsSlotAvailableAsync(start, end, 1);

        Assert.True(isAvailable);
    }

    [Fact]
    public async Task IsSlotAvailableAsync_ShouldReturnFalse_WhenConflict()
    {
        var start = new DateTime(2025, 6, 1, 9, 30, 0);
        var end = new DateTime(2025, 6, 1, 10, 30, 0);

        var isAvailable = await _repo.IsSlotAvailableAsync(start, end, 1);

        Assert.False(isAvailable);
    }

    [Fact]
    public async Task GetByFacilityAndRangeAsync_ShouldReturnFilteredReservations()
    {
        var start = new DateTime(2025, 6, 1);
        var end = new DateTime(2025, 6, 2, 23, 59, 59);

        var results = await _repo.GetByFacilityAndRangeAsync(1, start, end);

        Assert.Single(results);
        Assert.All(results, r => Assert.Equal(1, r.FacilityId));
        Assert.All(results, r => Assert.True(r.StartTime >= start && r.EndTime <= end));
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatches()
    {
        var results = await _repo.SearchAsync(1, 1);
        Assert.Single(results);
        Assert.Contains(results, r => r.FacilityId == 1 && r.UserId == 1);
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnTrue_WhenOverlappingReservationExists()
    {
        var start = new DateTime(2025, 6, 1, 9, 30, 0);
        var end = new DateTime(2025, 6, 1, 10, 30, 0);

        var hasConflict = await _repo.HasConflictAsync(1, start, end, 2);

        Assert.True(hasConflict);
    }
}