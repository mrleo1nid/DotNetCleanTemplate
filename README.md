# Clean Architecture API Template

Современный шаблон для создания .NET API с использованием Clean Architecture, CQRS, DDD, FastEndpoints и MediatR.

## 📊 Статус проекта

| Бейдж | Описание |
|-------|----------|
| ![NuGet](https://img.shields.io/nuget/v/DotNetCleanTemplate) | Версия пакета в NuGet |
| ![NuGet Downloads](https://img.shields.io/nuget/dt/DotNetCleanTemplate) | Количество загрузок |
| ![Build](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/build.yml) | Статус сборки |
| ![Tests](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/tests.yml) | Статус тестов |
| ![SonarCloud](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/sonarcloud.yml) | Анализ качества кода |
| ![NuGet Publish](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/nuget-publish.yml) | Публикация в NuGet |
| ![License](https://img.shields.io/github/license/mrleo1nid/DotNetCleanTemplate) | Лицензия |
| ![.NET](https://img.shields.io/badge/.NET-9.0-blue) | Версия .NET |
| ![SonarCloud Quality Gate](https://sonarcloud.io/api/project_badges/quality_gate?project=mrleo1nid_DotNetCleanTemplate) | Качество кода |
| ![SonarCloud Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_DotNetCleanTemplate&metric=coverage) | Покрытие тестами |

## 📦 NuGet Template Package

Шаблон также доступен как NuGet пакет для быстрой установки:

```bash
dotnet new install DotNetCleanTemplate
```

После установки можно создавать новые проекты:

```bash
dotnet new cleanapi -n MyProject
```

## 🚀 Быстрый старт

### Установка шаблона

```bash
# Локальная установка
git clone <repository-url>
cd DotNetCleanTemplate
dotnet new install .

# Или установка из NuGet (после публикации)
dotnet new install DotNetCleanTemplate
```

### Создание нового проекта

```bash
# Базовое использование
dotnet new cleanapi -n MyProject

# С дополнительными параметрами
dotnet new cleanapi -n MyProject --include-tests false
```

## 🏗️ Архитектура

Этот шаблон следует принципам Clean Architecture с четким разделением на слои:

- **Domain Layer** - бизнес-логика и доменные модели
- **Application Layer** - use cases и CQRS с MediatR
- **Infrastructure Layer** - внешние зависимости (БД, кэш, внешние API)
- **API Layer** - веб-интерфейс с FastEndpoints

## 🎯 Особенности

- **Clean Architecture** - строгое разделение слоев
- **CQRS** - Command Query Responsibility Segregation с MediatR
- **DDD** - Domain-Driven Design с агрегатами и value objects
- **FastEndpoints** - современный веб-фреймворк
- **Entity Framework Core** - ORM для работы с БД
- **JWT Authentication** - аутентификация и авторизация
- **Redis Caching** - кэширование
- **Health Checks** - проверка состояния приложения
- **Rate Limiting** - ограничение частоты запросов
- **Structured Logging** - структурированное логирование
- **Unit & Integration Tests** - полное покрытие тестами

## 📁 Структура проекта

```
MyProject/
├── MyProject.Api/              # API слой (FastEndpoints)
├── MyProject.Application/      # Слой приложения (CQRS, MediatR)
├── MyProject.Domain/           # Доменный слой (DDD)
├── MyProject.Infrastructure/   # Инфраструктурный слой (EF Core, Redis)
├── MyProject.Shared/           # Общие DTO и модели
└── Tests/
    ├── MyProject.UnitTests/    # Модульные тесты
    └── MyProject.IntegrationTests/ # Интеграционные тесты
```

## ⚙️ Параметры шаблона

| Параметр | Тип | По умолчанию | Описание |
|----------|-----|--------------|----------|
| `ProjectName` | string | - | Имя проекта (обязательный) |
| `IncludeTests` | bool | true | Включить тестовые проекты |

## 🔧 После создания проекта

1. **Обновите конфигурацию:**
   - Отредактируйте `appsettings.json` в папке `configs/`
   - Настройте строки подключения к БД
   - Обновите JWT секреты

2. **Запустите миграции:**
```bash
dotnet ef database update --project MyProject.Infrastructure --startup-project MyProject.Api
```

3. **Запустите приложение:**
```bash
dotnet run --project MyProject.Api
```

4. **Проверьте Swagger:**
   - Откройте `https://localhost:7001/swagger`

## 🧪 Тестирование

```bash
# Запуск всех тестов
dotnet test

# Запуск unit тестов
dotnet test Tests/MyProject.UnitTests/

# Запуск integration тестов
dotnet test Tests/MyProject.IntegrationTests/
```

## 📚 Документация

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS](https://martinfowler.com/bliki/CQRS.html)
- [DDD](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [FastEndpoints](https://fast-endpoints.com/)
- [MediatR](https://github.com/jbogard/MediatR)
- [NuGet Publishing Guide](Docs/NUGET_PUBLISHING.md)

## 🤝 Поддержка

Если у вас есть вопросы или проблемы:

1. Создайте issue в репозитории
2. Проверьте документацию
3. Изучите примеры в папке `Features`

## 📄 Лицензия

MIT License
