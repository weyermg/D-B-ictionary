# D/B Dictionary

ASP.NET Core API and React/TypeScript frontend for the Elasticsearch-backed dictionary.

## Run with Docker Compose

From the repository root:

```sh
docker compose up
```

Compose builds and starts the API and frontend. Open `http://localhost:3000`. The API is also available at `http://localhost:8080`.

The backend connects to the Elasticsearch VMs configured in `ELASTICSEARCH_NODES`. The default is the three local VM addresses used by the previous app. Override them in the shell or a root `.env` file:

```sh
ELASTICSEARCH_NODES=http://10.0.0.6:9200,http://10.0.0.28:9200,http://10.0.0.109:9200
```

To rebuild after changing source, use `docker compose up --build`. Stop the services with `docker compose down`.

The API ensures the `dictionary-words` index exists and inserts the sample `cat` document using a stable ID. Existing index contents are not deleted on startup.