# dotnet-nextjs-otel-poc

## Setup POC
 - create an env file in the root

```plain
DD_API_KEY=YOUR_API_KEY
DD_SITE=datadoghq.eu
DD_APM_DD_URL=https://trace.agent.datadoghq.eu
```

 - Run the Datadog agent using Docker Compose at the root of the project.

```bash
docker compose up -d
```
 - create an env.local file in the nextjs app root

```plain
NODE_OPTIONS=-r ./node_modules/dd-trace/init
DD_APPLICATION_ID=YOUR_DD_APPLICATION_ID
DD_CLIENT_TOKEN=YOUR_DD_CLIENT_TOKEN
DD_SITE=YOUR_DD_SITE
```
 - Install dependencies (only the first time) and run the Next.js app.

```bash
cd my-next-app
pnpm i
pnpm run dev
```

 - Run the .NET application.

```bash
cd my-api
dotnet run
```

 - Navigate to the Next.js app at http://localhost:2999/.
 - Check Datadog for traces.

### How it works

### Nextjs

Basic integration with Datadog can be achieved using the `dd-trace` npm package.

```bash
pnpm i --save dd-trace
```

The module is run using the NODE_OPTIONS environment variable in the .env.local file.

```plain
NODE_OPTIONS=-r ./node_modules/dd-trace/init
```

### Dotnet

 - If you have a Datadog agent running on your host, it is sufficient to set up launchProfile.json:

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5131",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "DD_TRACE_AGENT_URL": "http://localhost:8126",
        "DD_ENV": "localhost",
        "DD_SERVICE": "api",
        "DD_VERSION": "V1",
        "DD_LOGS_INJECTION": "true",
        "DD_DOGSTATSD_NON_LOCAL_TRAFFIC": "true",
        "DD_TRACE_DEBUG": "false",
        "DD_PROFILING_ENABLED": "false",
        "DD_RUNTIME_METRICS_ENABLED": "true",
        "CORECLR_ENABLE_PROFILING": "1",
        "CORECLR_PROFILER": "{846F5F1C-F9AE-4B07-969E-05C26BC060D8}"
      }
    }
  }
}

```

## Datadog RUM 

 - Install @datadog/browser-rum npm package

```bash
cd my-next-app
pnpm i --save @datadog/browser-rum
```

 - Create a component

```tsx
"use client";

// Necessary if using App Router to ensure this file runs on the client
import { datadogRum } from "@datadog/browser-rum";

datadogRum.init({
    applicationId: process.env.DD_APPLICATION_ID as string,
    clientToken: process.env.DD_CLIENT_TOKEN as string,
    site: process.env.DD_SITE as string,
    service: process.env.DD_SERVICE as string,
    env: process.env.DD_ENV as string,
    version: process.env.DD_VERSION as string,
    sessionSampleRate: 100,
    sessionReplaySampleRate: 100,
    trackUserInteractions: true,
    trackResources: true,
    trackLongTasks: true,
    // enableExperimentalFeatures: true,
    defaultPrivacyLevel: "mask-user-input",
    allowedTracingUrls: [
        { match: process.env.API_URL as string, propagatorTypes: [process.env.DD_RUM_TRACE_PROPAGATOR_TYPE as any]}
    ]
});

// datadogRum.startSessionReplayRecording();


export default function DatadogRumInit() {
    // Render nothing - this component is only included so that the init code
    // above will run client-side
    return null;
}
```
 - add some env variables
    - API_URL is used in the DatadogRumInit component to correlate RUM traces with API traces.
    - DD_RUM_TRACE_PROPAGATOR_TYPE=datadog - more information [here](https://docs.datadoghq.com/real_user_monitoring/platform/connect_rum_and_traces/?tab=browserrum#opentelemetry-support)

```plain
API_URL=http://localhost:5131
DD_RUM_TRACE_PROPAGATOR_TYPE=datadog
```

 - Replace the hardcoded URL in WeatherComponent with process.env.API_URL.

```javascript
const response = await fetch(process.env.API_URL + '/weatherforecast');
```
 - Note this setting: it injects headers into the fetch calls to correlate RUM traces with API traces.
```javascript
    allowedTracingUrls: [
        { match: process.env.API_URL as string, propagatorTypes: [process.env.DD_RUM_TRACE_PROPAGATOR_TYPE as any]}
    ]
```


 - export env variables in next.config.mjs 

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
    env: {
        DD_APPLICATION_ID: process.env.DD_APPLICATION_ID,
        DD_CLIENT_TOKEN: process.env.DD_CLIENT_TOKEN,
        DD_SITE: process.env.DD_SITE,
        DD_SERVICE: process.env.DD_SERVICE,
        DD_ENV: process.env.DD_ENV,
        DD_VERSION: process.env.DD_VERSION,
        API_URL: process.env.API_URL,
        DD_RUM_TRACE_PROPAGATOR_TYPE: process.env.DD_RUM_TRACE_PROPAGATOR_TYPE
    }
};

export default nextConfig;
```

 - Include the component in layout.tsx

```tsx
//...
import DatadogRumInit from "./components/DatadogRumInit";
//...
    <html lang="en">
      <body className={inter.className}>
        {children}
        <DatadogRumInit />
      </body>
    </html>
```

 - Restart the application.
 - Verify the configuration in the browser window console.

```javascript
window.DD_RUM.getInitConfiguration()
//{applicationId: 'YOUR_APP_ID', clientToken: 'YOUR_CLIENT_TOKEN', site: 'YOUR_DD_SITE', service: 'my-nextjs-app', env: 'localhost', …}
```


