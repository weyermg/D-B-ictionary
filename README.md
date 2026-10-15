# D/B Dictionary

ASP.NET Core API and React/TypeScript frontend for the Elasticsearch-backed dictionary.

## Run with Docker Compose

From the repository root:

```sh
docker compose up
```

Compose builds and starts the API and frontend. Open `http://localhost:3000`. The API is also available at `http://localhost:8080`.


To rebuild after changing source, use `docker compose up --build`. Stop the services with `docker compose down`.