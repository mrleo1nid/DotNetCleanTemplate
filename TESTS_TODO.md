# Список тестов для полного покрытия кода

## Общая статистика покрытия
- **Общее покрытие строк**: 95.4% (2865 из 3002 строк)
- **Покрытие веток**: 85.6% (334 из 390 веток)
- **Всего тестов**: 397 (все прошли успешно)

---

## 1. DotNetCleanTemplate.Api (92.9% покрытие)

### ApplicationBootstrapper (83.6% покрытие)

#### Конфигурация и инициализация
- [x] `Test_InitializeConfiguration_ThrowsException_WhenConfigurationIsInvalid`
- [x] `Test_InitializeConfiguration_WithMissingConfigFiles`
- [x] `Test_InitializeConfiguration_WithInvalidJsonFormat`

#### JWT аутентификация
- [x] `Test_AddJwtAuth_ThrowsException_WhenJwtSettingsNotConfigured`
- [x] `Test_AddJwtAuth_ThrowsException_WhenJwtKeyIsEmpty`
- [x] `Test_AddJwtAuth_ThrowsException_WhenJwtIssuerIsEmpty`
- [x] `Test_AddJwtAuth_ThrowsException_WhenJwtAudienceIsEmpty`

#### База данных
- [x] `Test_AddDatabase_ThrowsException_WhenConnectionStringNotSet`
- [x] `Test_AddDatabase_ThrowsException_WhenConnectionStringIsInvalid`
- [x] `Test_AddDatabase_WithValidConnectionString`

#### Rate Limiting
- [x] `Test_AddRateLimiting_WithIpPartitionOnly`
- [x] `Test_AddRateLimiting_WithApiKeyPartitionOnly`
- [x] `Test_AddRateLimiting_WithGlobalLimiterOnly`
- [x] `Test_AddRateLimiting_WithBothPartitions`
- [x] `Test_AddRateLimiting_ThrowsException_WhenSettingsNotConfigured`
- [x] `Test_AddRateLimiting_WithCustomRejectionHandler`
- [x] `Test_AddRateLimiting_WithDifferentWindowSizes`
- [x] `Test_AddRateLimiting_WithQueueLimit`

### Program.cs (75% покрытие)
- [x] `Test_Program_Main_HandlesExceptions`
- [x] `Test_Program_Main_WithInvalidArguments`
- [x] `Test_Program_Main_WithMissingEnvironmentVariables`

### HttpContextHelper (97.3% покрытие)
- [x] `Test_GetClientIpAddress_WithXForwardedForHeader`
- [x] `Test_GetClientIpAddress_WithMultipleXForwardedForHeaders`
- [x] `Test_GetClientIpAddress_WithInvalidIpAddress`
- [x] `Test_GetClientIpAddress_WithPrivateIpAddress`

### ErrorHandlingMiddleware (100% покрытие, но 50% веток)
- [x] `Test_ErrorHandlingMiddleware_WithDevelopmentEnvironment`
- [x] `Test_ErrorHandlingMiddleware_WithProductionEnvironment`
- [x] `Test_ErrorHandlingMiddleware_WithDifferentExceptionTypes`

---

## 2. DotNetCleanTemplate.Application (96.3% покрытие)

### ApplicationInitDataService (0% покрытие)
- [ ] `Test_ApplicationInitDataService_InitializeAsync_CallsUnderlyingService`
- [ ] `Test_ApplicationInitDataService_InitializeAsync_HandlesExceptions`
- [ ] `Test_ApplicationInitDataService_InitializeAsync_WithCancellationToken`

### ApplicationMigrationService (0% покрытие)
- [ ] `Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_CallsUnderlyingService`
- [ ] `Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_HandlesExceptions`
- [ ] `Test_ApplicationMigrationService_ApplyMigrationsIfEnabledAsync_WithCancellationToken`

### UserService (92% покрытие)
- [ ] `Test_UserService_CreateUserAsync_WhenUserAlreadyExists`
- [ ] `Test_UserService_CreateUserAsync_WhenRepositoryThrowsException`
- [ ] `Test_UserService_CreateUserAsync_WithInvalidUserData`
- [ ] `Test_UserService_FindByEmailAsync_WhenRepositoryThrowsException`

### Behaviors
- [ ] `Test_CachingBehavior_WithCacheHit`
- [ ] `Test_CachingBehavior_WithCacheMiss`
- [ ] `Test_CachingBehavior_WithCacheException`
- [ ] `Test_MetricsBehavior_WithSuccessfulRequest`
- [ ] `Test_MetricsBehavior_WithFailedRequest`
- [ ] `Test_PerformanceBehaviour_WithSlowRequest`
- [ ] `Test_PerformanceBehaviour_WithFastRequest`

---

## 3. DotNetCleanTemplate.Domain (95.1% покрытие)

### Email ValueObject (95.4% покрытие)
- [ ] `Test_Email_Constructor_WithConsecutiveDots_ThrowsException`
- [ ] `Test_Email_Constructor_WithInvalidRegexPattern`
- [ ] `Test_Email_Constructor_WithSpecialCharacters`
- [ ] `Test_Email_Constructor_WithInternationalDomain`
- [ ] `Test_Email_Constructor_WithSubdomain`

### PasswordHash ValueObject (93.3% покрытие)
- [ ] `Test_PasswordHash_Constructor_WithInvalidHashFormat`
- [ ] `Test_PasswordHash_ValidateHash_WithInvalidHash`
- [ ] `Test_PasswordHash_Constructor_WithEmptyHash`
- [ ] `Test_PasswordHash_Constructor_WithNullHash`

### Entity и ValueObject базовые классы
- [ ] `Test_Entity_Equals_WithDifferentTypes`
- [ ] `Test_Entity_Equals_WithNull`
- [ ] `Test_ValueObject_Equals_WithDifferentTypes`
- [ ] `Test_ValueObject_Equals_WithNull`

### RefreshToken Entity
- [ ] `Test_RefreshToken_IsExpired_WithExpiredToken`
- [ ] `Test_RefreshToken_IsExpired_WithValidToken`
- [ ] `Test_RefreshToken_IsRevoked_WithRevokedToken`
- [ ] `Test_RefreshToken_IsRevoked_WithValidToken`

---

## 4. DotNetCleanTemplate.Infrastructure (96% покрытие)

### UnitOfWork (29.1% покрытие)
- [ ] `Test_UnitOfWork_ExecuteInTransactionAsync_WithExistingTransaction`
- [ ] `Test_UnitOfWork_ExecuteInTransactionAsync_WithNewTransaction`
- [ ] `Test_UnitOfWork_ExecuteInTransactionAsync_WhenActionThrowsException_RollsBack`
- [ ] `Test_UnitOfWork_ExecuteInTransactionAsync_WithDifferentIsolationLevels`
- [ ] `Test_UnitOfWork_ExecuteInTransactionAsync_WithReadUncommitted`
- [ ] `Test_UnitOfWork_ExecuteInTransactionAsync_WithSerializable`
- [ ] `Test_UnitOfWork_SaveChangesAsync_WithConcurrentModifications`

### BaseRepository (67.7% покрытие)
- [ ] `Test_BaseRepository_AddOrUpdateAsync_WhenEntityExists`
- [ ] `Test_BaseRepository_AddOrUpdateAsync_WhenEntityDoesNotExist`
- [ ] `Test_BaseRepository_DeleteAsync_WithNonExistentEntity`
- [ ] `Test_BaseRepository_UpdateAsync_WithDetachedEntity`
- [ ] `Test_BaseRepository_GetByIdAsync_WithNonExistentId`
- [ ] `Test_BaseRepository_ExistsAsync_WithComplexPredicate`
- [ ] `Test_BaseRepository_GetAllAsync_WithEmptyResult`
- [ ] `Test_BaseRepository_CountAsync_WithPredicate`

### RefreshTokenRepository (71.4% покрытие)
- [ ] `Test_RefreshTokenRepository_FindByToken_WithInvalidToken`
- [ ] `Test_RefreshTokenRepository_FindByToken_WithExpiredToken`
- [ ] `Test_RefreshTokenRepository_RevokeToken_WithNonExistentToken`
- [ ] `Test_RefreshTokenRepository_FindByToken_WithRevokedToken`
- [ ] `Test_RefreshTokenRepository_GetExpiredTokens_WithNoExpiredTokens`
- [ ] `Test_RefreshTokenRepository_GetExpiredTokens_WithMultipleExpiredTokens`

### UserLockoutRepository (92.8% покрытие)
- [ ] `Test_UserLockoutRepository_FindByUserId_WithNonExistentUser`
- [ ] `Test_UserLockoutRepository_UpdateLockout_WithNewLockout`
- [ ] `Test_UserLockoutRepository_GetExpiredLockouts_WithNoExpiredLockouts`
- [ ] `Test_UserLockoutRepository_GetExpiredLockouts_WithMultipleExpiredLockouts`

### ExpiredTokenCleanupService (89.3% покрытие)
- [ ] `Test_ExpiredTokenCleanupService_StartAsync_WhenServiceDisabled`
- [ ] `Test_ExpiredTokenCleanupService_StartAsync_WhenNoTokensToClean`
- [ ] `Test_ExpiredTokenCleanupService_StartAsync_WhenExceptionOccurs`
- [ ] `Test_ExpiredTokenCleanupService_StartAsync_WithMultipleExpiredTokens`
- [ ] `Test_ExpiredTokenCleanupService_StartAsync_WithConcurrentAccess`

### HealthCheckService (91.3% покрытие)
- [ ] `Test_HealthCheckService_CheckHealthAsync_WhenDatabaseUnavailable`
- [ ] `Test_HealthCheckService_CheckHealthAsync_WhenCacheUnavailable`
- [ ] `Test_HealthCheckService_CheckHealthAsync_WhenAllServicesUnavailable`
- [ ] `Test_HealthCheckService_CheckHealthAsync_WithSlowDatabase`
- [ ] `Test_HealthCheckService_CheckHealthAsync_WithCacheTimeout`

### UserLockoutCleanupService (83.6% покрытие)
- [ ] `Test_UserLockoutCleanupService_StartAsync_WhenServiceDisabled`
- [ ] `Test_UserLockoutCleanupService_StartAsync_WhenNoLockoutsToClean`
- [ ] `Test_UserLockoutCleanupService_StartAsync_WhenExceptionOccurs`
- [ ] `Test_UserLockoutCleanupService_StartAsync_WithMultipleExpiredLockouts`
- [ ] `Test_UserLockoutCleanupService_StartAsync_WithConcurrentAccess`

### CacheService (100% покрытие, но 92.8% веток)
- [ ] `Test_CacheService_GetAsync_WithCacheMiss`
- [ ] `Test_CacheService_SetAsync_WithExpiration`
- [ ] `Test_CacheService_RemoveAsync_WithNonExistentKey`
- [ ] `Test_CacheService_GetAsync_WithCacheException`

---

## 5. DotNetCleanTemplate.Shared (98% покрытие)

### Result<T> (95% покрытие)
- [ ] `Test_Result_ImplicitConversion_FromValue`
- [ ] `Test_Result_ImplicitConversion_FromError`
- [ ] `Test_Result_WithValue_WhenValueIsNull`
- [ ] `Test_Result_WithError_WhenErrorIsNull`
- [ ] `Test_Result_IsSuccess_WithSuccessResult`
- [ ] `Test_Result_IsFailure_WithFailureResult`

### Error (100% покрытие)
- [ ] `Test_Error_Constructor_WithNullCode`
- [ ] `Test_Error_Constructor_WithNullMessage`
- [ ] `Test_Error_Equals_WithDifferentErrors`

---

## 6. Интеграционные тесты

### Rate Limiting
- [ ] `Test_RateLimiting_WithIpPartition`
- [ ] `Test_RateLimiting_WithApiKeyPartition`
- [ ] `Test_RateLimiting_WithGlobalLimiter`
- [ ] `Test_RateLimiting_WithChainedLimiters`
- [ ] `Test_RateLimiting_WithRetryAfterHeader`
- [ ] `Test_RateLimiting_WithConcurrentRequests`

### Error Handling
- [ ] `Test_ErrorHandling_WithDifferentExceptionTypes`
- [ ] `Test_ErrorHandling_WithValidationExceptions`
- [ ] `Test_ErrorHandling_WithAuthenticationExceptions`
- [ ] `Test_ErrorHandling_WithAuthorizationExceptions`
- [ ] `Test_ErrorHandling_WithDatabaseExceptions`

### User Lockout
- [ ] `Test_UserLockout_WithMultipleFailedAttempts`
- [ ] `Test_UserLockout_WithSuccessfulLoginAfterLockout`
- [ ] `Test_UserLockout_WithLockoutExpiration`
- [ ] `Test_UserLockout_WithConcurrentLoginAttempts`

### Token Management
- [ ] `Test_TokenCleanup_WithExpiredTokens`
- [ ] `Test_TokenCleanup_WithRevokedTokens`
- [ ] `Test_TokenCleanup_WithMixedTokenStates`
- [ ] `Test_RefreshToken_WithValidToken`
- [ ] `Test_RefreshToken_WithExpiredToken`
- [ ] `Test_RefreshToken_WithRevokedToken`

### Health Checks
- [ ] `Test_HealthCheck_WithUnhealthyServices`
- [ ] `Test_HealthCheck_WithPartiallyHealthyServices`
- [ ] `Test_HealthCheck_WithAllHealthyServices`
- [ ] `Test_HealthCheck_WithServiceTimeouts`

### Database Operations
- [ ] `Test_Database_Migration_WithInvalidMigration`
- [ ] `Test_Database_Migration_WithConcurrentMigrations`
- [ ] `Test_Database_Transaction_WithRollback`
- [ ] `Test_Database_Transaction_WithCommit`
- [ ] `Test_Database_Concurrent_Access`

### Initialization
- [ ] `Test_InitData_WithInvalidConfiguration`
- [ ] `Test_InitData_WithExistingData`
- [ ] `Test_InitData_WithPartialData`
- [ ] `Test_InitData_WithConcurrentInitialization`

### Authentication Flow
- [ ] `Test_Authentication_CompleteFlow`
- [ ] `Test_Authentication_WithInvalidCredentials`
- [ ] `Test_Authentication_WithLockedUser`
- [ ] `Test_Authentication_WithExpiredToken`
- [ ] `Test_Authentication_WithInvalidToken`

---

## 7. Тесты производительности и стресс-тесты

### Производительность
- [ ] `Test_Performance_WithHighConcurrentRequests`
- [ ] `Test_Performance_WithLargeDataSets`
- [ ] `Test_Performance_WithComplexQueries`
- [ ] `Test_Performance_WithCacheOperations`

### Стресс-тесты
- [ ] `Test_Stress_WithRateLimiting`
- [ ] `Test_Stress_WithDatabaseConnections`
- [ ] `Test_Stress_WithMemoryUsage`
- [ ] `Test_Stress_WithConcurrentTransactions`

### Нагрузочные тесты
- [ ] `Test_Load_With1000ConcurrentUsers`
- [ ] `Test_Load_With10000RequestsPerSecond`
- [ ] `Test_Load_WithLargePayloads`
- [ ] `Test_Load_WithLongRunningOperations`

---

## 8. Тесты безопасности

### Аутентификация и авторизация
- [ ] `Test_Security_WithInvalidJwtToken`
- [ ] `Test_Security_WithExpiredJwtToken`
- [ ] `Test_Security_WithTamperedJwtToken`
- [ ] `Test_Security_WithInsufficientPermissions`
- [ ] `Test_Security_WithRoleEscalation`

### Валидация входных данных
- [ ] `Test_Security_WithSqlInjectionAttempts`
- [ ] `Test_Security_WithXssAttempts`
- [ ] `Test_Security_WithPathTraversalAttempts`
- [ ] `Test_Security_WithLargePayloads`

### Rate Limiting Security
- [ ] `Test_Security_WithBypassRateLimitingAttempts`
- [ ] `Test_Security_WithSpoofedIpAddresses`
- [ ] `Test_Security_WithFakeApiKeys`

---

## Приоритеты реализации

### 🔴 Высокий приоритет (критичные для безопасности и стабильности)
- [ ] Тесты для ApplicationBootstrapper (конфигурация)
- [ ] Тесты для UnitOfWork (транзакции)
- [ ] Тесты для rate limiting
- [ ] Тесты для обработки ошибок
- [ ] Тесты безопасности

### 🟡 Средний приоритет (важные для функциональности)
- [ ] Тесты для репозиториев
- [ ] Тесты для сервисов очистки
- [ ] Тесты для health checks
- [ ] Интеграционные тесты

### 🟢 Низкий приоритет (улучшение покрытия)
- [ ] Тесты для декораторов
- [ ] Тесты для value objects
- [ ] Тесты производительности
- [ ] Стресс-тесты

---

## Прогресс выполнения

**Всего тестов**: 25 / 200+ (418 тестов всего в проекте)
**Покрытие строк**: 95.4% → Цель: 100%
**Покрытие веток**: 85.6% → Цель: 100%

### Выполненные блоки:
- ✅ ApplicationBootstrapper (все тесты)
- ✅ Program.cs (все тесты)
- ✅ HttpContextHelper (все тесты)
- ✅ ErrorHandlingMiddleware (все тесты)

---

## Заметки

- Тесты должны использовать Testcontainers для интеграционных тестов
- Все тесты должны быть изолированными и не зависеть друг от друга
- Использовать моки для внешних зависимостей
- Добавить тесты для edge cases и boundary conditions
- Включить тесты для обработки исключений
- Добавить тесты для concurrent scenarios 