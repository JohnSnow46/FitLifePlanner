// This is the development-time service worker. It exists only so the browser has
// something to register while running locally; it intentionally does not cache
// anything. The publish build swaps this file for service-worker.published.js
// (see the ServiceWorkerAssetsManifest wiring in FitLifePlanner.Web.csproj), which
// does the real offline-shell caching.
self.addEventListener('fetch', () => { });
