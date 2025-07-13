# DotNetCleanTemplate

| Build Status | Tests | SonarCloud | Coverage | Last Commit |
|:------------:|:-----:|:----------:|:--------:|:-----------:|
| ![Build](https://github.com/mrleo1nid/DotNetCleanTemplate/actions/workflows/build.yml/badge.svg) | ![Tests](https://github.com/mrleo1nid/DotNetCleanTemplate/actions/workflows/test.yml/badge.svg) | [![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_DotNetCleanTemplate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_DotNetCleanTemplate) | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrleo1nid_DotNetCleanTemplate&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mrleo1nid_DotNetCleanTemplate) | ![Last Commit](https://img.shields.io/github/last-commit/mrleo1nid/DotNetCleanTemplate) |

## Описание

DotNetCleanTemplate — это универсальный шаблон для построения современных .NET приложений по принципам Clean Architecture, CQRS и DDD. Подходит для старта любого корпоративного или pet-проекта с разделением на слои, поддержкой MediatR, FastEndpoints, Entity Framework Core и другими современными технологиями.

## Архитектура

Проект реализует Clean Architecture и разделён на несколько слоёв:

- **DotNetCleanTemplate.Api** — API-слой на FastEndpoints, отвечает за взаимодействие с внешним миром.
- **DotNetCleanTemplate.Application** — слой приложения, реализует CQRS, использует MediatR, AutoMapper и паттерн Result.
- **DotNetCleanTemplate.Domain** — доменный слой, построен по принципам DDD (Domain-Driven Design).
- **DotNetCleanTemplate.Infrastructure** — инфраструктурный слой, использует Entity Framework Core и Cache Manager для работы с данными и кэшированием.
- **DotNetCleanTemplate.Shared** — общий слой, содержит вспомогательные компоненты (например, Humanizer).

## Технологии

- .NET 9
- FastEndpoints
- MediatR
- AutoMapper
- Entity Framework Core
- Cache Manager
- Humanizer
- CQRS
- DDD
- Clean Architecture

## Структура решения

```
DotNetCleanTemplate/
  DotNetCleanTemplate.Api/           # API слой
  DotNetCleanTemplate.Application/   # Слой приложения (CQRS, MediatR)
  DotNetCleanTemplate.Domain/        # Доменная логика (DDD)
  DotNetCleanTemplate.Infrastructure/# Инфраструктура (EF Core, Cache)
  DotNetCleanTemplate.Shared/        # Общие компоненты
```

## Использование как шаблона

1. Склонируйте репозиторий:
   ```bash
   git clone https://github.com/your-org/DotNetCleanTemplate.git
   ```
2. Переименуйте solution, проекты и namespaces под ваш продукт (или используйте как есть).
3. Настройте строки подключения и параметры в `appsettings.json`.
4. Запустите проект `DotNetCleanTemplate.Api`.
5. Разрабатывайте свой функционал, следуя принципам Clean Architecture.

## Быстрый старт

1. Откройте решение в Visual Studio, JetBrains Rider или через CLI.
2. Установите зависимости:
   ```bash
   dotnet restore
   ```
3. Соберите решение:
   ```bash
   dotnet build
   ```
4. Запустите API:
   ```bash
   dotnet run --project DotNetCleanTemplate.Api
   ```

## Контакты

- Автор шаблона: mrleo1nid

---

_Этот проект распространяется под лицензией MIT_
