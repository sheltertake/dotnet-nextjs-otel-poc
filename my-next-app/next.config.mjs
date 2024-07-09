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
