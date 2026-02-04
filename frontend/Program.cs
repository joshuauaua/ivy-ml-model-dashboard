CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
#if DEBUG
server.UseHotReload();
#endif
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

// Removed server.UseChrome(chromeSettings) to avoid conflict with manual SidebarLayout
// and to resolve the "Invalid AppId '/'" error coming from DefaultSidebarChrome.

await server.RunAsync();