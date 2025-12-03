namespace Smdb.Csr;

using System;
using System.Net;
using System.Collections;
using Shared;
using Shared.Http;
using Smdb.Core.Movies;
using Smdb.Core.Db;
using Smdb.Api.Movies;  // This is crucial - for MoviesController and MoviesRouter

// aliases so legacy names used in this project resolve to CLR/Shared types
using HttpRequest = System.Net.HttpListenerRequest;
using HttpResponse = System.Net.HttpListenerResponse;
using RouteProperties = System.Collections.Hashtable;

public class App : HttpServer
{
    public App()
    {
    }

    public override void Init()
    {
        router.Use(HttpUtils.StructuredLogging);
        router.Use(HttpUtils.CentralizedErrorHandling);
        router.Use(HttpUtils.AddResponseCorsHeaders);
        router.Use(HttpUtils.DefaultResponse);
        router.Use(HttpUtils.ParseRequestUrl);
        router.Use(HttpUtils.ParseRequestQueryString);
        router.Use(HttpUtils.ServeStaticFiles);
        router.UseSimpleRouteMatching();

        // Set up the API (Movie routes)
        var db = new MemoryDatabase();
        var movieRepo = new MemoryMovieRepository(db);
        var movieServ = new DefaultMovieService(movieRepo);
        var movieCtrl = new MoviesController(movieServ);
        var movieRouter = new MoviesRouter(movieCtrl);  // This needs Smdb.Api.Movies namespace
        var apiRouter = new HttpRouter();

        // Mount the API routes under /api/v1
        router.UseRouter("/api/v1", apiRouter);
        apiRouter.UseRouter("/movies", movieRouter);

        router.MapGet("/", LandingPageIndexRedirect);
        router.MapGet("/movies", MoviesPageIndexRedirect);
    }

    public static async Task LandingPageIndexRedirect(HttpRequest req, HttpResponse res, RouteProperties props, Func<Task> next)
    {
        res.Redirect("/index.html");
        await next();
    }

    public static async Task MoviesPageIndexRedirect(HttpRequest req, HttpResponse res, RouteProperties props, Func<Task> next)
    {
        res.Redirect("/movies/index.html");
        await next();
    }
}