# RRC_Sender
Данный проект является учебным, имитирующий бэкенд-систему обработки заказов.

## Цель проекта
Отработка практических навыков работы с:
- **Apache Kafka** (асинхронная коммуникация между сервисами)
- **Docker & Docker Compose** (контейнеризация)
- **PostgreSQL & Liquibase** (управление схемой БД)
- **Паттернами:** Choreography и Transactional Outbox

## Архитектура
Система состоит из трёх независимых сервисов:
1. **RRC_Sender** — приём заказов (документировано Swagger UI)
2. **Storage** — проверка наличия товара
3. **Notification** — отправка уведомлений в Telegram

## Технологический стек
- **Коммуникация:** Apache Kafka
- **База данных:** PostgreSQL + Liquibase для миграций
- **Контейнеризация:** Docker, Docker Compose
- **API Документация:** Swagger UI
- **Паттерны:** Choreography, Transactional Outbox
