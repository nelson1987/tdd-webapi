# tdd-webapi
### In Docker
Docker Start
```sh
docker compose up --build --remove-orphans --force-recreate -d
```
Docker Stop
```sh
docker compose down -v
```
### After Docker Up
In [pgAdmin](http://localhost:16543/login?next=/browser/)
User: postgres
Pass: postgres

### In PgAdmin
host: host.docker.internal
