# Science Conferences API

ASP.NET Core 8, Entity Framework Core и PostgreSQL. При запуске через Docker используйте Compose из соседнего conf-frontend: он применяет миграции автоматически.

## Локальный запуск

Нужны .NET SDK 8 и запущенный PostgreSQL. Настройте строку подключения через .NET User Secrets (или переменную ConnectionStrings__DefaultConnection):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=science_conf_db;Username=postgres;Password=ТВОЙ_ПАРОЛЬ" --project ConferencePlatform.Api
dotnet tool restore
dotnet ef database update --project ConferencePlatform.Api
dotnet run --project ConferencePlatform.Api --launch-profile https
```

API слушает https://localhost:7239; в режиме Development есть Swagger (/swagger). Миграция DashboardAuthAndTracks добавляет уникальный email, недостающий признак верификации спикера и секции двух демонстрационных конференций. Существующие конференции и заявки сохраняются.

В этой учебной сборке роль Organizer можно выбрать при регистрации или активировать в своём аккаунте. Также можно задать OrganizerAccount__Email и OrganizerAccount__Password через окружение либо OrganizerAccount:Email и OrganizerAccount:Password через User Secrets. Сервис создаёт аккаунт с ролью Organizer, только если такого email ещё нет. Используйте отдельный email организатора и пароль длиной 8–128 символов. Пароль существующего аккаунта и его роль автоматически не меняются.

Токен действует два часа. Если нужно сохранять сессию между перезапусками API, настройте случайный Auth:SigningKey длиной не менее 32 байт через User Secrets или Auth__SigningKey в окружении. Иначе ключ генерируется на каждый запуск.

## Контракты для фронтенда

| Запрос | Результат |
| --- | --- |
| GET /api/conferences | Список конференций |
| POST /api/conferences | Принимает title, description, startDate, endDate, location, organizerId. Только Organizer |
| GET /api/tracks?conferenceId={id} | Секции указанной конференции |
| POST /api/tracks | Принимает conferenceId и name. Только организатор своей конференции |
| POST /api/auth/register | Принимает email, password, fullName, role (Listener или Organizer). Возвращает 201 с userId, fullName, role и access_token |
| POST /api/auth/login | Принимает email и password. Возвращает 200 с userId, fullName, role и access_token |
| POST /api/auth/become-organizer/{userId} | Меняет роль текущего пользователя и возвращает обновлённый токен |
| GET /api/auth/me | Профиль текущего пользователя с Bearer-токеном |
| POST /api/talk-submissions | Принимает title, abstract, speakerId и существующий trackId. Возвращает 201 |
| GET /api/talk-submissions | Все заявки. Можно указать status. Только Organizer |
| PATCH /api/talk-submissions/{id}/status | Принимает статус Approved или Rejected. Только Organizer; уже рассмотренная заявка возвращает 409 |

В закрытые запросы передавайте заголовок Authorization: Bearer <access_token>. Роль для закрытых операций проверяется сервером. Пароли хранятся в виде хешей.
