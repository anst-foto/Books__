# GitHub Actions Workflows

## Workflows

### 1. CI Pipeline (`ci.yml`)
Выполняется на каждый push и pull request в branches `main` и `develop`.

**Действия:**
- Проверка кода из репозитория
- Установка .NET SDK 10.0
- Восстановление зависимостей
- Сборка проекта в Release конфигурации
- Запуск тестов (если проект тестов существует)

### 2. Docker Build and Push (`docker-publish.yml`)
Выполняется при push изменений в `backend/Books.WebAPI/` или при изменении Dockerfile.

**Действия:**
- Сборка Docker образа
- Отправка в GitHub Container Registry (ghcr.io)
- Автоматическое тегирование образов

**Теги образа:**
- `branch-name` — тег основной ветки
- `sha-<commit>` — тег по SHA commit
- `latest` — только для main ветки
- Semantic versioning теги (если используются git tags)

## Настройка

### Требования
- GitHub репозиторий
- GitHub Actions включены (включены по умолчанию)

### Автоматическая конфигурация
Workflows используют `GITHUB_TOKEN` для аутентификации в GitHub Container Registry. Никаких дополнительных secrets не требуется.

## Использование образа

После успешной сборки образ доступен в GitHub Container Registry:

```bash
# Вход в реестр
docker login ghcr.io -u USERNAME -p TOKEN

# Скачивание образа
docker pull ghcr.io/YOUR-USERNAME/books-webapi:latest

# Использование в docker-compose
WEBAPI_IMAGE=ghcr.io/YOUR-USERNAME/books-webapi
WEBAPI_VERSION=latest
docker compose up -d
```

### Получение TOKEN
1. Перейди в Settings → Developer settings → Personal access tokens
2. Создай новый token с правами `read:packages`
3. Используй этот token для входа

## Просмотр результатов

1. Перейди в репозиторий → Actions
2. Выбери нужный workflow
3. Просмотри логи и результаты
