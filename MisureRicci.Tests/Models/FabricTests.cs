using MisureRicci.Models;
using Moq;
using System;
using FluentAssertions;
using Xunit;

namespace MisureRicci.Tests.Models
{
    public class FabricTests
    {
        private readonly MockRepository mockRepository;



        public FabricTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private static Fabric CreateFabric()
        {
            return new Fabric();
        }

        [Fact]
        public void CreateFabric_ShouldInitializeExpectedDefaults()
        {
            // Arrange
            var fabric = CreateFabric();

            // Assert
            fabric.Should().NotBeNull();
            fabric.Id.Should().Be(0);
            fabric.Nome.Should().BeEmpty();
            fabric.Descrizione.Should().BeNull();
            fabric.Composizione.Should().BeNull();
            fabric.IsActive.Should().BeTrue();
            fabric.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            this.mockRepository.VerifyAll();
        }
    }
}
