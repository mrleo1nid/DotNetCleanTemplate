# Clean Architecture API Template

Современный шаблон для создания .NET API с использованием Clean Architecture, CQRS, DDD, FastEndpoints и MediatR.

## 📊 Статус проекта

[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square&logo=.net)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE.txt)
[![Last Commit](https://img.shields.io/github/last-commit/mrleo1nid/DotNetCleanTemplate?style=flat-square&logo=github)](https://github.com/mrleo1nid/DotNetCleanTemplate/commits/main)
[![NuGet Version](https://img.shields.io/nuget/v/DotNetCleanTemplate?style=flat-square&logo=nuget)](https://www.nuget.org/packages/DotNetCleanTemplate/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/build.yml?style=flat-square&logo=github-actions)](https://github.com/mrleo1nid/DotNetCleanTemplate/actions/workflows/build.yml)
[![Tests Status](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/tests.yml?style=flat-square&logo=github-actions)](https://github.com/mrleo1nid/DotNetCleanTemplate/actions/workflows/tests.yml)
[![SonarCloud Analysis](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/sonarcloud.yml?style=flat-square&logo=sonarcloud)](https://github.com/mrleo1nid/DotNetCleanTemplate/actions/workflows/sonarcloud.yml)
[![NuGet Publish](https://img.shields.io/github/actions/workflow/status/mrleo1nid/DotNetCleanTemplate/nuget-publish.yml?style=flat-square&logo=nuget)](https://github.com/mrleo1nid/DotNetCleanTemplate/actions/workflows/nuget-publish.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/quality_gate?project=mrleo1nid_DotNetCleanTemplate&style=flat-square)](https://sonarcloud.io/project/overview?id=mrleo1nid_DotNetCleanTemplate)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_DotNetCleanTemplate&metric=coverage&style=flat-square)](https://sonarcloud.io/project/overview?id=mrleo1nid_DotNetCleanTemplate)

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
