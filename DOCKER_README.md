# Docker Compose Setup

Этот проект использует Docker Compose для запуска всех необходимых сервисов.

## Сервисы

- **PostgreSQL 17** - основная база данных
- **Redis 7** - кэширование и сессии
- **DotNetCleanTemplate.Api** - основное приложение

## Быстрый старт

1. **Запуск всех сервисов:**
   ```bash
   docker-compose up -d
   ```

2. **Просмотр логов:**
   ```bash
   docker-compose logs -f
   ```

3. **Остановка сервисов:**
   ```bash
   docker-compose down
   ```

## Переменные окружения

Основные переменные окружения настраиваются в `.env` файле:

### Database
- `POSTGRES_EXTERNAL_PORT` - внешний порт PostgreSQL (по умолчанию: 5434)
- `POSTGRES_PORT` - внутренний порт PostgreSQL (по умолчанию: 5432)
- `POSTGRES_HOST` - хост PostgreSQL (по умолчанию: localhost)
- `POSTGRES_DATABASE` - имя базы данных (по умолчанию: DotNetCleanTemplate)
- `POSTGRES_USERNAME` - пользователь PostgreSQL (по умолчанию: postgres)
- `POSTGRES_PASSWORD` - пароль PostgreSQL (по умолчанию: postgres)

### Redis
- `REDIS_CONNECTION_STRING` - строка подключения к Redis (по умолчанию: localhost:6379,abortConnect=false)
- `REDIS_PORT` - порт Redis (по умолчанию: 6379)
- `REDIS_EXTERNAL_PORT` - внешний порт Redis (по умолчанию: 6380)

### JWT
- `JWT_KEY` - ключ для JWT токенов (по умолчанию: mysupersecretkey1234567890123456)
- `JWT_ISSUER` - издатель JWT токенов (по умолчанию: localhost)
- `JWT_AUDIENCE` - аудитория JWT токенов (по умолчанию: localhost)

### Init Data
- `INIT_ADMIN_PASSWORD` - пароль администратора по умолчанию (по умолчанию: Admin123!)

## Портфолио

- **API**: http://localhost:80
- **PostgreSQL**: localhost:5434 (внешний) → 5432 (внутренний)
- **Redis**: localhost:6380 (внешний) → 6379 (внутренний)

## Полезные команды

```bash
# Пересборка и запуск
docker-compose up --build -d

# Просмотр логов конкретного сервиса
docker-compose logs -f dotnetcleantemplate.api

# Подключение к PostgreSQL
docker-compose exec postgres psql -U postgres -d DotNetCleanTemplate

# Подключение к Redis
docker-compose exec redis redis-cli

# Очистка всех данных
docker-compose down -v
```

## Health Checks

Все сервисы имеют настроенные health checks:
- PostgreSQL проверяется командой `pg_isready`
- Redis проверяется командой `ping`
- API приложение ждет готовности зависимостей перед запуском

## Volumes

- `postgres_data` - данные PostgreSQL
- `redis_data` - данные Redis

## Networks

Все сервисы подключены к сети `dotnetcleantemplate-network` для изоляции.

## Пример .env файла

```env
# database
POSTGRES_EXTERNAL_PORT=5434
POSTGRES_PORT=5432
POSTGRES_HOST=localhost
POSTGRES_DATABASE=DotNetCleanTemplate
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=postgres

# redis
REDIS_CONNECTION_STRING=localhost:6379,abortConnect=false
REDIS_PORT=6379
REDIS_EXTERNAL_PORT=6380

#jwt
JWT_KEY=mysupersecretkey1234567890123456
JWT_ISSUER=localhost
JWT_AUDIENCE=localhost

#initData
INIT_ADMIN_PASSWORD=Admin123!
``` 