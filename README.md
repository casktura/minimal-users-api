# Minimal Users API

A minimal users API written in .NET (C#).

## Requirements

-   Visual Studio 2022 (Not tested on other version)
-   Docker Desktop

## Setup Instructions

1. Run `Docker Desktop`
2. Clone repository into your local drive.
3. Open solution with `Visual Studio 2022`.
4. Build whole solution and run `DockerCompose` project one time to start docker and run all dependencies (MariaDB).
5. Run `Update-Database` inside `Package Manager Console` with `Users.Library` project selected (also select `Users.Library` as startup project) to migrate database (MariaDb inside docker container).
6. Run `DockerCompose` to start the application.

## Notes

-   There are 2 roles in applications: USER and ADMIN.
    -   Only USER can be created using the API. For ADMIN, the Role column in Users table has to be adjusted manually inside database.
-   API permissions:
    -   **POST** `/login` and **POST** `/users` and be access by any one.
        -   It's for account creation and login.
    -   `/users/me` can be access by any USER to manage their own account data.
    -   The rest of `/users` can only access by ADMIN to manage all user account data.
