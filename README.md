# Petstore API Testing (.NET: xUnit & NUnit)

Автоматизированное тестирование REST API [Swagger Petstore v2](https://petstore.swagger.io/) (`https://petstore.swagger.io/v2`) с помощью **xUnit** и **NUnit**. В каждом тестовом проекте реализовано по **38 тест-кейсов** для разделов `Pet` (16), `Store` (11) и `User` (11).

## Структура проекта

- `Petstore.XUnitTests/` — проект с тестами на xUnit (`IClassFixture`, `[Fact]`, `[Theory]` + `[InlineData]`)
- `Petstore.NUnitTests/` — проект с тестами на NUnit (`[TestFixture]`, `[OneTimeSetUp]`, `[Test]`, `[TestCase]`)
- `Analysis.md` — таблицы Анализа №1 (количественные показатели) и Анализа №2 (качественная оценка)
- `Questions.md` — ответы на контрольные вопросы
- `screenshots/` — скриншоты результатов прогона тестов

## Запуск из терминала

```powershell
dotnet restore
dotnet test Petstore.XUnitTests/Petstore.XUnitTests.csproj --logger "console;verbosity=normal"
dotnet test Petstore.NUnitTests/Petstore.NUnitTests.csproj --logger "console;verbosity=normal"
```

## Скриншоты результатов

### xUnit

![Результаты xUnit](screenshots/xunit-results.png)

### NUnit

![Результаты NUnit](screenshots/nunit-results.png)

## Документы по лабораторной

- [Анализ результатов (Analysis.md)](Analysis.md)
- [Контрольные вопросы (Questions.md)](Questions.md)
