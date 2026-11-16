using Xunit;
using VitalCheck.Controllers;
using VitalCheck.Models;

namespace VitalCheck.Tests
{
    /// <summary>
    /// Verifies input validation requirements:
    /// - Email address format validation
    /// - Age boundaries (5 to 120)
    /// - Height constraints (50 cm to 260 cm)
    /// - Weight constraints (20 kg to 350 kg)
    /// - Client profile required fields
    /// - Assessment diagnostic boundaries
    /// </summary>
    public class InputValidationTests
    {
        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("coach.smith@vitalcheck.org", true)]
        [InlineData("alex123@domain.co.uk", true)]
        [InlineData("invalid-email", false)]
        [InlineData("user@", false)]
        [InlineData("@domain.com", false)]
        [InlineData("user@ domain.com", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidEmail_ValidatesCorrectly(string? email, bool expectedValid)
        {
            bool isValid = UserController.IsValidEmail(email!);
            Assert.Equal(expectedValid, isValid);
        }

        [Fact]
        public void ValidateClientInput_RejectsEmptyName()
        {
            var client = new ClientProfile
            {
                FullName = "   ",
                Age = 30,
                Height = 175,
                Weight = 70
            };

            bool isValid = ClientController.ValidateClientInput(client, out string err);
            Assert.False(isValid);
            Assert.Contains("name is required", err, System.StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(4)]   // Below minimum age 5
        [InlineData(121)] // Above maximum age 120
        public void ValidateClientInput_RejectsInvalidAge(int invalidAge)
        {
            var client = new ClientProfile
            {
                FullName = "Jane Doe",
                Age = invalidAge,
                Height = 165,
                Weight = 60
            };

            bool isValid = ClientController.ValidateClientInput(client, out string err);
            Assert.False(isValid);
            Assert.Contains("Age must be between 5 and 120", err);
        }

        [Theory]
        [InlineData(45)]  // Too short
        [InlineData(270)] // Too tall
        public void ValidateClientInput_RejectsInvalidHeight(decimal invalidHeight)
        {
            var client = new ClientProfile
            {
                FullName = "Jane Doe",
                Age = 25,
                Height = invalidHeight,
                Weight = 60
            };

            bool isValid = ClientController.ValidateClientInput(client, out string err);
            Assert.False(isValid);
            Assert.Contains("Height must be between 50 cm and 260 cm", err);
        }

        [Theory]
        [InlineData(15)]  // Too light
        [InlineData(400)] // Too heavy
        public void ValidateClientInput_RejectsInvalidWeight(decimal invalidWeight)
        {
            var client = new ClientProfile
            {
                FullName = "Jane Doe",
                Age = 25,
                Height = 170,
                Weight = invalidWeight
            };

            bool isValid = ClientController.ValidateClientInput(client, out string err);
            Assert.False(isValid);
            Assert.Contains("Weight must be between 20 kg and 350 kg", err);
        }

        [Fact]
        public void ValidateClientInput_AcceptsValidProfile()
        {
            var client = new ClientProfile
            {
                FullName = "John Doe",
                Age = 32,
                Sex = "Male",
                Height = 180,
                Weight = 78
            };

            bool isValid = ClientController.ValidateClientInput(client, out string err);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(err));
        }
    }
}
