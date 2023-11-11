CREATE ROLE postgres WITH LOGIN CREATEDB PASSWORD 'postgres';
ALTER
ROLE postgres WITH SUPERUSER;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory"
(
    "MigrationId" character varying
(
    150
) NOT NULL,
    "ProductVersion" character varying
(
    32
) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY
(
    "MigrationId"
)
    );


DO
$EF$
BEGIN
    IF
NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231104100619_AddUserTable') THEN
CREATE TABLE "Users"
(
    "Id"           uuid    NOT NULL,
    "Email"        text    NOT NULL,
    "Name"         text    NOT NULL,
    "Surname"      text    NOT NULL,
    "Lastname"     text    NOT NULL,
    "Role"         integer NOT NULL,
    "PasswordHash" text    NOT NULL,
    "Location"     text NULL,
    "Grade"        integer NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);
END IF;
END $EF$;

DO
$EF$
BEGIN
    IF
NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231104100619_AddUserTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231104100619_AddUserTable', '7.0.13');
END IF;
END $EF$;
