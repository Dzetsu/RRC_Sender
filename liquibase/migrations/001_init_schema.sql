--liquibase formatted sql

--changeset you:001

create schema items;

create table items.items(
    id serial primary key,
    nameitem text not null
);

insert into items.items (nameitem) values ('Cola');
insert into items.items (nameitem) values ('Fanta');
insert into items.items (nameitem) values ('Milk');

create table items.orders_outbox(
    id bigserial primary key,
    username text not null,
    name text not null,
    amount bigint not null,
    token text not null,
    status int not null default 0
);

create table items.orders(
    id bigserial primary key,
    username text not null,
    name text not null,
    amount bigint not null,
    token text not null,
    status int not null default 0
);

create schema storage;

create table storage.storage_items(
    id serial primary key,
    name text not null,
    amount bigint not null default 0
);

insert into storage.storage_items (name, amount) values ('Cola', 30);
insert into storage.storage_items (name, amount) values ('Fanta', 100);
insert into storage.storage_items (name, amount) values ('Milk', 100);

create schema telegram_bot;

create table telegram_bot.telegram_messages(
    id int primary key,    
    name_item text not null,
    amount bigint not null,
    status bool not null
);
