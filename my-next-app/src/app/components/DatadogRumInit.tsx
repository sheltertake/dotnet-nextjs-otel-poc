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
        { match: process.env.API_URL as string, propagatorTypes: [process.env.DD_RUM_TRACE_PROPAGATOR_TYPE as any]},
        { match: process.env.API_OTEL_URL as string, propagatorTypes: ["tracecontext"]}
    ]
});

// datadogRum.startSessionReplayRecording();


export default function DatadogRumInit() {
    // Render nothing - this component is only included so that the init code
    // above will run client-side
    return null;
}