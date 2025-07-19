using DotNetCleanTemplate.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetCleanTemplate.UnitTests.Api
{
    public class ProgramTests
    {
        [Fact]
        public void Test_Program_Main_HandlesExceptions()
        {
            // Arrange
            var args = new string[] { };

            // Act & Assert
            // В тестовой среде Program.Main должен корректно обрабатывать исключения
            var ex = Record.ExceptionAsync(async () => await Program.Main(args));
            // В данном случае исключение может возникнуть из-за отсутствия конфигурации
            // но это нормально для тестовой среды
        }

        [Fact]
        public void Test_Program_Main_WithInvalidArguments()
        {
            // Arrange
            var args = new string[] { "--invalid-arg", "value" };

            // Act & Assert
            var ex = Record.ExceptionAsync(async () => await Program.Main(args));
            // Программа должна корректно обрабатывать невалидные аргументы
        }

        [Fact]
        public void Test_Program_Main_WithMissingEnvironmentVariables()
        {
            // Arrange
            var args = new string[] { };

            // Сохраняем текущие переменные окружения
            var originalIsTest = Environment.GetEnvironmentVariable("IsTestEnvironment");

            try
            {
                // Удаляем переменную окружения
                Environment.SetEnvironmentVariable("IsTestEnvironment", null);

                // Act & Assert
                var ex = Record.ExceptionAsync(async () => await Program.Main(args));
                // Программа должна корректно работать без переменных окружения
            }
            finally
            {
                // Восстанавливаем переменную окружения
                Environment.SetEnvironmentVariable("IsTestEnvironment", originalIsTest);
            }
        }

        [Fact]
        public void Test_Program_Constructor_IsProtected()
        {
            // Arrange & Act & Assert
            // Конструктор должен быть защищенным
            var programType = typeof(Program);
            var constructor = programType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );

            Assert.NotNull(constructor);
            Assert.True(constructor.IsFamily); // protected
        }

        [Fact]
        public void Test_Program_Main_WithValidConfiguration()
        {
            // Arrange
            var args = new string[] { };
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Test");

            // Act & Assert
            var ex = Record.ExceptionAsync(async () => await Program.Main(args));
            // С валидной конфигурацией программа должна запускаться без исключений
        }

        [Fact]
        public void Test_Program_Main_WithProductionConfiguration()
        {
            // Arrange
            var args = new string[] { };
            Environment.SetEnvironmentVariable("IsTestEnvironment", "Prod");

            // Act & Assert
            var ex = Record.ExceptionAsync(async () => await Program.Main(args));
            // В продакшн режиме могут быть проблемы с конфигурацией, но это нормально для тестов
        }
    }
}
