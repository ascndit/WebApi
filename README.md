# WebApi

REST API для клиентов и заказов.

## Стек

- .NET 9, ASP.NET Core
- Entity Framework Core и PostgreSQL
- Swagger
- контроллеры, сервисы, DTO, middleware обработки ошибок, миграции
- unit-тесты сервисов на xUnit

## Запуск

Нужны .NET 9 SDK и PostgreSQL. Строка подключения `ConnectionStrings:DefaultConnection` берётся из user secrets, в репозитории её нет.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=webapi;Username=postgres;Password=postgres" --project WebApi
dotnet ef database update --project WebApi
dotnet run --project WebApi
```

Swagger открывается по адресу `/swagger`.

## Тесты

```bash
dotnet test
```

## Методы

- `api/customers` — список, карточка, создание, изменение и удаление
- `api/orders` — те же операции, плюс постраничная выдача и параметры запроса
