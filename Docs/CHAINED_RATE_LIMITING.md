# Цепные ограничители (Chained Rate Limiters)

## Описание

В проекте реализованы цепные ограничители с использованием `PartitionedRateLimiter.CreateChained()`, которые позволяют применять несколько ограничителей одновременно к одному запросу. Все ограничители выполняются последовательно, и запрос будет отклонен, если любой из них превысит лимит.

## Конфигурация

### Настройки в appsettings.json

```json
{
  "RateLimiting": {
    "QueueLimit": 10,
    "UseIpPartition": true,
    "UseApiKeyPartition": true,
    "ApiKeyHeaderName": "X-API-Key",
    "IpPermitLimit": 4,
    "IpWindowSeconds": 12,
    "ApiKeyPermitLimit": 20,
    "ApiKeyWindowSeconds": 30
  }
}
```

### Параметры конфигурации

- **QueueLimit** - максимальное количество запросов в очереди
- **UseIpPartition** - включить ограничение по IP адресу
- **UseApiKeyPartition** - включить ограничение по API ключу
- **ApiKeyHeaderName** - название заголовка для API ключа
- **IpPermitLimit** - количество разрешенных запросов для IP (в окне)
- **IpWindowSeconds** - размер окна в секундах для IP ограничителя
- **ApiKeyPermitLimit** - количество разрешенных запросов для API ключа (в окне)
- **ApiKeyWindowSeconds** - размер окна в секундах для API ключа ограничителя

## Типы ограничителей

### 1. Ограничитель по IP адресу

Применяется ко всем запросам с одного IP адреса. Определяет IP адрес через:
- Заголовок `X-Forwarded-For`
- Заголовок `X-Real-IP`
- `HttpContext.Connection.RemoteIpAddress`

### 2. Ограничитель по API ключу

Применяется к запросам с API ключом в заголовке. Если API ключ отсутствует, используется ключ `"no_api_key"`.

## Принцип работы

1. **Последовательное выполнение**: Все включенные ограничители выполняются последовательно
2. **Любое превышение лимита**: Запрос отклоняется, если любой из ограничителей превышает свой лимит
3. **Разные настройки**: Каждый тип ограничителя может иметь свои настройки лимитов и окон

## Пример использования

### Тестовый endpoint

```bash
# Тест без API ключа (только IP ограничение)
curl http://localhost:5033/api/test/rate-limit

# Тест с API ключом (IP + API ключ ограничения)
curl -H "X-API-Key: your-api-key" http://localhost:5033/api/test/rate-limit
```

### Сценарии ограничений

1. **Только IP ограничение** (`UseIpPartition: true`, `UseApiKeyPartition: false`)
   - 120 запросов в 60 секунд с одного IP

2. **Только API ключ ограничение** (`UseIpPartition: false`, `UseApiKeyPartition: true`)
   - 120 запросов в 60 секунд с одним API ключом

3. **Оба ограничения** (`UseIpPartition: true`, `UseApiKeyPartition: true`)
   - Должен пройти оба ограничения: 120  запросов в 60 секунд с IP И 120 запросов в 60 секунд с API ключом

## Обработка ошибок

При превышении лимита возвращается:
- HTTP статус: `429 Too Many Requests`
- Заголовок `Retry-After` с количеством секунд до следующего разрешенного запроса
- Сообщение: "Too many requests. Please try again later."

## Реализация

Основная логика находится в `ApplicationBootstrapper.AddRateLimiting()`:

```csharp
// Создаем цепные ограничители
var limiters = new List<PartitionedRateLimiter<HttpContext>>();

// Добавляем ограничители в зависимости от настроек
if (rateLimitingSettings.UseIpPartition)
{
    limiters.Add(PartitionedRateLimiter.Create<HttpContext, string>(...));
}

if (rateLimitingSettings.UseApiKeyPartition)
{
    limiters.Add(PartitionedRateLimiter.Create<HttpContext, string>(...));
}

// Устанавливаем глобальный ограничитель как цепочку
options.GlobalLimiter = PartitionedRateLimiter.CreateChained(limiters.ToArray());
```

## Преимущества

1. **Гибкость**: Можно комбинировать разные типы ограничений
2. **Производительность**: Эффективная реализация с использованием встроенных возможностей .NET
3. **Настраиваемость**: Каждый тип ограничителя имеет свои параметры
4. **Масштабируемость**: Легко добавлять новые типы ограничителей