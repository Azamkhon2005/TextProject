# TextProject [КР 2]
### Бузуруков Саидазамхон, БПИ 236
## Архитектура системы:
Система построена на основе микросервисной архитектуры для обеспечения гибкости, масштабируемости и независимого развертывания компонентов.
Основные компоненты архитектуры:
### API Gateway:
Технология: Ocelot.
Ответственность: Единая точка входа для всех клиентских запросов. Отвечает исключительно за маршрутизацию запросов к соответствующим нижестоящим микросервисам. Обеспечивает единый фасад для системы.

### File Storing Service (Сервис Хранения Файлов):
Технологии: ASP.NET Core Web API, PostgreSQL, Локальное файловое хранилище (или облачное).

Функциональность:
- Прием и сохранение текстовых файлов (.txt).
- Хранение метаданных файлов (имя, размер, тип, хэш контента, путь к физическому файлу) в базе данных PostgreSQL.
- Вычисление хэша содержимого для каждого загружаемого файла.
- Предоставление API для загрузки файлов и получения их содержимого по уникальному идентификатору.
- Каждый файл при загрузке получает уникальный ID. Если загружается файл, контент которого полностью идентичен ранее загруженному (проверка по хэшу), новый файл физически не сохраняется, но возвращается информация, указывающая на это (например, флаг isNewContent=false). Примечание: после последних изменений, если проверка на плагиат по хэшу убрана из этого сервиса и каждый файл считается новым, этот пункт нужно скорректировать.
### File Analysis Service (Сервис Анализа Файлов):
Технологии: ASP.NET Core Web API, PostgreSQL.

Функциональность:
- Получение содержимого файла от FileStoringService по его ID.
- Проведение анализа текста: подсчет количества абзацев, слов, символов.
- Хранение результатов анализа в своей базе данных PostgreSQL, связывая их с ID файла.
- Предоставление API для запуска анализа и получения его результатов.
- Учет флага isNewContent (полученного при загрузке файла), чтобы отметить результат анализа как относящийся к контенту, который является 100% дубликатом ранее виденного.
(Опционально, было удалено) Взаимодействие с внешним API для генерации облака слов.
### Базы данных:
Каждый сервис (FileStoringService и FileAnalysisService) использует свою собственную, изолированную базу данных PostgreSQL. Это соответствует принципам микросервисной архитектуры "база данных на сервис".
### Файловое хранилище:
Используется FileStoringService для физического хранения загруженных файлов. Может быть реализовано как локальная папка на сервере или как облачное хранилище (например, Azure Blob Storage, AWS S3).

## Спецификация API 
Для каждого сервиса (FileStoringService.Api и FileAnalysisService.Api) настроена поддержка OpenAPI (Swagger). Спецификацию можно получить, обратившись к Swagger UI каждого сервиса при их запуске:
- File Storing Service Swagger UI: http://localhost:5001/swagger/index.html (в среде разработки)
- File Analysis Service Swagger UI: http://localhost:5002/swagger/index.html (или https://localhost:7002/swagger/index.html в среде разработки)

## Инструкции по Запуску и Развертыванию
## Требования:
- .NET SDK (версия, указанная в проектах, например, .NET 8.0).
- PostgreSQL сервер.
## Настройка баз данных:
Создайте две базы данных в PostgreSQL: одну для FileStoringService (textscanner_filestoring_dev) и одну для FileAnalysisService (textscanner_fileanalysis_dev).
## Запуск сервисов:
Можно настроить Visual Studio на запуск нескольких проектов (ApiGateway, FileStoringService.Api, FileAnalysisService.Api).
Либо запустить каждый сервис из командной строки с помощью dotnet run в соответствующей папке проекта.

### Пример работы сервисов:
## FileStoringService
![image](https://github.com/user-attachments/assets/505e3fdf-353f-44f1-8936-ad1fe8d8aa4d)
## FileAnalysisService
![image](https://github.com/user-attachments/assets/04d742e7-80c8-4b73-b785-dc072b60a746)

