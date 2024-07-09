# dotnet-nextjs-otel-poc

create an env file in the root

```plain
DD_API_KEY=YOUR_API_KEY
DD_SITE=datadoghq.eu
DD_APM_DD_URL=https://trace.agent.datadoghq.eu
```

create a 

run docker compose

```bash
docker compose up -d
```

install deps (only first time) and run next js app

```bash
cd my-next-app
pnpm i
pnpm run dev
```

run dotnet 

```bash
cd my-api
dotnet run
```