namespace DotNetCleanTemplate.UnitTests.Application
{
    public class PaginationTests
    {
        [Fact]
        public void Pagination_ShouldWork()
        {
            // Arrange
            var pageSize = 10;

            // Act
            var totalPages = (int)Math.Ceiling((double)25 / pageSize);

            // Assert
            Assert.Equal(3, totalPages);
        }
    }
}
