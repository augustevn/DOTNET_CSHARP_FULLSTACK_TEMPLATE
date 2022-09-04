

## Database

First time with MSSQL image (accept EULA):
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<pw-in-docker-compose>" -p "127.0.0.1:1433:1433" --name MSSQL_PETSOCIAL -d mcr.microsoft.com/mssql/server:2017-latest
```

Commands:

```
docker-compose -f ./Docker/docker-compose.yml -p petsocial up -d
docker-compose -f ./Docker/docker-compose.yml -p petsocial stop
```

Connect:

```
User=sa
Host=127.0.0.1
Database=petsocial
```
Database `petsocial` becomes available after first database update. Before that, the only existing database is `master`.

## Caching

Run local Redis instance in Docker:

```
docker run -p 6379:6379 redis
```


Sources:

- https://docs.docker.com/compose/aspnet-mssql-compose/
- https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/database-server-container
- https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-configure-environment-variables?view=sql-server-2017