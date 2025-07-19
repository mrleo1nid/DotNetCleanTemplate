# Clean Architecture API Template

Современный шаблон для создания .NET API с использованием Clean Architecture, CQRS, DDD, FastEndpoints и MediatR.

## 🏗️ Архитектура

Этот шаблон следует принципам Clean Architecture с четким разделением на слои:

- **Domain Layer** - бизнес-логика и доменные модели
- **Application Layer** - use cases и CQRS с MediatR
- **Infrastructure Layer** - внешние зависимости (БД, кэш, внешние API)
- **API Layer** - веб-интерфейс с FastEndpoints

## 🚀 Технологии

- **.NET 9** - последняя версия .NET
- **FastEndpoints** - современный веб-фреймворк
- **MediatR** - реализация CQRS и медиатора
- **Entity Framework Core** - ORM для работы с БД
- **Mapster** - быстрый маппинг объектов
- **FluentValidation** - валидация данных
- **JWT Bearer** - аутентификация
- **Redis** - кэширование
- **Serilog** - структурированное логирование
- **Health Checks** - мониторинг состояния
- **Rate Limiting** - ограничение частоты запросов

## 📁 Структура проекта

```
ProjectName/
├── ProjectName.Api/              # API слой
│   ├── Endpoints/                # FastEndpoints
│   ├── Handlers/                 # Middleware
│   ├── DependencyExtensions/     # DI конфигурация
│   └── Program.cs               # Точка входа
├── ProjectName.Application/      # Слой приложения
│   ├── Features/                # CQRS команды и запросы
│   ├── Behaviors/               # Pipeline behaviors
│   ├── Services/                # Application services
│   ├── Interfaces/              # Application contracts
│   ├── Mapping/                 # Mapster конфигурация
│   └── Validation/              # FluentValidation
├── ProjectName.Domain/           # Доменный слой
│   ├── Entities/                # Доменные сущности
│   ├── ValueObjects/            # Value objects
│   ├── Repositories/            # Repository interfaces
│   ├── Services/                # Domain services
│   └── Common/                  # Общие доменные элементы
├── ProjectName.Infrastructure/   # Инфраструктурный слой
│   ├── Persistent/              # EF Core контекст и репозитории
│   ├── Services/                # Infrastructure services
│   ├── Configurations/          # Конфигурации
│   └── Migrations/              # EF Core миграции
├── ProjectName.Shared/           # Общие элементы
│   ├── DTOs/                    # Data Transfer Objects
│   └── Common/                  # Общие модели
└── Tests/
    ├── ProjectName.UnitTests/    # Модульные тесты
    └── ProjectName.IntegrationTests/ # Интеграционные тесты
```

## 🎯 Особенности

### Clean Architecture
- Строгое разделение слоев
- Dependency Inversion Principle
- Независимость домена от внешних зависимостей

### CQRS с MediatR
- Разделение команд и запросов
- Pipeline behaviors для cross-cutting concerns
- Легкое тестирование

### DDD
- Агрегаты и value objects
- Доменные события
- Богатая доменная модель

### FastEndpoints
- Высокая производительность
- Минимальный overhead
- Современный API

### Тестирование
- Полное покрытие unit тестами
- Integration тесты с Testcontainers
- Mock и stub стратегии

## 🔧 Конфигурация

Шаблон поддерживает различные конфигурации через параметры:

- Выбор базы данных (SQL Server/PostgreSQL)
- Включение/отключение компонентов
- Настройка аутентификации
- Конфигурация кэширования

## 📚 Документация

После создания проекта:

1. Обновите строки подключения в `appsettings.json`
2. Настройте JWT секреты
3. Запустите миграции
4. Изучите примеры в папке `Features`

## 🤝 Вклад в проект

Приветствуются pull requests и issues для улучшения шаблона! 