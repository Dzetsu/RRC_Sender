create schema items;

create table items.items(
    id serial primary key,
    nameitem text not null
);

insert into items.items (nameitem) values ('Cola');
insert into items.items (nameitem) values ('Fanta');
insert into items.items (nameitem) values ('Milk');

create table items.itemsoutbox(
    id bigserial primary key,
    username text not null,
    name text not null,
    amount bigint not null,
    token text not null,
    status char not null default 'n'
);

create table items.orderitems(
    id bigserial primary key,
    username text not null,
    name text not null,
    amount bigint not null,
    token text not null
);

create schema storage;

create table storage."storageItems"(
    id serial primary key,
    name text not null,
    amount bigint not null default 0
);

insert into storage."storageItems" (name, amount) values ('Cola', 30);
insert into storage."storageItems" (name, amount) values ('Fanta', 100);
insert into storage."storageItems" (name, amount) values ('Milk', 100);
