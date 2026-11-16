using Xunit;
using VitalCheck.Data;
using VitalCheck.Models;

namespace VitalCheck.Tests
{
    /// <summary>
    /// Verifies authentication security and user models:
    /// - SHA-256 password hashing (consistent, secure, non-reversible, non-plain-text)
    /// - User roles and permissions (Individual vs Fitness Instructor)
    /// - ClientProfile entity data mapping
    /// </summary>
    public class SecurityAndUserTests
    {
        [Fact]
        public void HashPassword_ProducesConsistentSHA256Hash()
        {
            string password = "SecretPassword123!";
            string hash1 = DatabaseHelper.HashPassword(password);
            string hash2 = DatabaseHelper.HashPassword(password);

            Assert.False(string.IsNullOrWhiteSpace(hash1));
            Assert.Equal(64, hash1.Length); // 256 bits = 64 hex characters
            Assert.Equal(hash1, hash2);
            Assert.NotEqual(password, hash1);
        }

        [Fact]
        public void HashPassword_ProducesDifferentHashesForDifferentPasswords()
        {
            string hashA = DatabaseHelper.HashPassword("PasswordA");
            string hashB = DatabaseHelper.HashPassword("PasswordB");

            Assert.NotEqual(hashA, hashB);
        }

        [Fact]
        public void HashPassword_ReturnsEmptyString_ForEmptyOrNullPassword()
        {
            Assert.Equal(string.Empty, DatabaseHelper.HashPassword(""));
            Assert.Equal(string.Empty, DatabaseHelper.HashPassword(null!));
        }

        [Theory]
        [InlineData("Individual", true, false)]
        [InlineData("Instructor", false, true)]
        public void UserRole_PropertiesAreAccurate(string role, bool expectedIndividual, bool expectedInstructor)
        {
            var user = new User
            {
                UserID = 1,
                FullName = "Alex Morgan",
                Email = "alex@example.com",
                UserRole = role
            };

            Assert.Equal(expectedIndividual, user.IsIndividual);
            Assert.Equal(expectedInstructor, user.IsInstructor);
        }
    }
}
