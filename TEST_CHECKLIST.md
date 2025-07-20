# Чек-лист тестов для полного покрытия строк

## Статистика
- **Текущее покрытие**: 78.73%
- **Целевое покрытие**: 90%+
- **Всего тестовых классов**: 36
- **Всего тестовых методов**: ~200

## Приоритет 1: Критические (0% покрытия) 🔴

### Валидаторы
- [x] `AssignRoleToUserDtoValidatorTests` (5 методов)
- [x] `LoginRequestDtoValidatorTests` (7 методов)
- [x] `LoginResponseDtoValidatorTests` (4 метода)
- [x] `RefreshTokenResponseDtoValidatorTests` (3 метода)
- [x] `RegisterUserDtoValidatorTests` (9 методов)
- [x] `UserWithRolesDtoValidatorTests` (7 методов)

### Сервисы инициализации
- [x] `ApplicationInitDataServiceTests` (3 метода)
- [x] `ApplicationMigrationServiceTests` (3 метода)

### Доменные Value Objects
- [x] `EmailValueObjectTests` (9 методов)
- [x] `PasswordHashValueObjectTests` (7 методов)
- [x] `UserNameValueObjectTests` (9 методов)
- [x] `RoleNameValueObjectTests` (9 методов)

### Базовые доменные классы
- [x] `EntityTests` (6 методов)
- [x] `AggregateRootTests` (5 методов)
- [x] `ValueObjectTests` (5 методов)

### Репозитории
- [x] `BaseRepositoryTests` (9 методов) - Интеграционный тест с Testcontainers ✅
- [x] `RefreshTokenRepositoryTests` (4 метода) - Интеграционный тест с Testcontainers ✅
- [ ] `RoleRepositoryTests` (2 метода)
- [ ] `UserLockoutRepositoryTests` (4 метода)
- [ ] `UserRoleRepositoryTests` (6 методов)

### Сервисы очистки
- [ ] `ExpiredTokenCleanupServiceTests` (5 методов)
- [ ] `UserLockoutCleanupServiceTests` (5 методов)

## Приоритет 2: Средний (25-75% покрытия) 🟡

### Обработчики команд
- [ ] `LoginCommandHandlerTests` (5 методов)
- [ ] `RegisterUserCommandHandlerTests` (4 метода)
- [ ] `RefreshTokenCommandHandlerTests` (5 методов)
- [ ] `AssignRoleToUserCommandHandlerTests` (4 метода)

### Сервисы пользователей
- [ ] `UserServiceTests` (7 методов)
- [ ] `UserLockoutServiceTests` (7 методов)
- [ ] `RoleServiceTests` (4 метода)

### HTTP обработчики
- [ ] `HttpContextHelperTests` (6 методов)
- [ ] `UserLockoutMiddlewareTests` (5 методов)

## Приоритет 3: Низкий (>75% покрытия) 🟢

### Edge cases
- [ ] `ApplicationBootstrapperTests` (4 метода)
- [ ] `CacheServiceTests` (3 метода)
- [ ] `PasswordHasherTests` (3 метода)
- [ ] `TokenServiceTests` (3 метода)

## Прогресс выполнения

### Неделя 1: Критические тесты
- [x] Валидаторы (6/6) - 100%
- [x] Сервисы инициализации (2/2) - 100%
- [x] Доменные Value Objects (4/4) - 100%
- [x] Базовые доменные классы (3/3) - 100%

### Неделя 2: Сервисы и репозитории
- [x] Сервисы инициализации (2/2) - 100% ✅
- [x] Репозитории (2/5) - 40% ✅ (в процессе)
- [ ] Сервисы очистки (2/2) - 0%

### Неделя 3: Обработчики и сервисы
- [ ] Обработчики команд (4/4) - 0%
- [ ] Сервисы пользователей (3/3) - 0%
- [ ] HTTP обработчики (2/2) - 0%

### Неделя 4: Edge cases и финализация
- [ ] Edge cases (4/4) - 0%
- [ ] Дополнительные интеграционные тесты
- [ ] Проверка покрытия и оптимизация

## Метрики успеха

### Качество тестов
- [ ] Все тесты проходят успешно
- [ ] Нет дублирующихся тестов
- [ ] Все edge cases покрыты
- [ ] Используются Testcontainers для интеграционных тестов

### Покрытие
- [ ] Покрытие строк: 90%+ (текущее: 78.73%)
- [ ] Покрытие веток: 80%+ (текущее: 51.72%)
- [ ] Время выполнения тестов: < 60 секунд

### Архитектура
- [ ] Соблюдение принципов Clean Architecture
- [ ] Использование CQRS и MediatR
- [ ] Правильное использование Result pattern
- [ ] Тестирование всех слоев архитектуры

## Быстрые команды

```bash
# Запуск всех тестов
dotnet test

# Запуск тестов с покрытием
dotnet test --collect:"XPlat Code Coverage"

# Запуск только unit тестов
dotnet test Tests/DotNetCleanTemplate.UnitTests/

# Запуск только integration тестов
dotnet test Tests/DotNetCleanTemplate.IntegrationTests/

# Генерация отчета покрытия
reportgenerator -reports:coverage/*/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
```

## Примечания

1. **Валидаторы** - самый высокий приоритет, так как имеют 0% покрытия
2. **Доменные объекты** - критически важны для архитектуры
3. **Репозитории** - требуют интеграционных тестов с Testcontainers
4. **Edge cases** - последний приоритет, но важны для надежности

## Контакты для вопросов

- Архитектура: Clean Architecture, CQRS, MediatR
- Тестирование: xUnit, Moq, Testcontainers
- Покрытие: XPlat Code Coverage, ReportGenerator 