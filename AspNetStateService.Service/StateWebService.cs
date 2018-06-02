using System;
using System.Fabric;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AspNetStateService.Service.Interfaces;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cogito.IO;
using FileAndServe.Autofac;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace AspNetStateService.Service
{

    [RegisterAs(typeof(StateWebService))]
    public class StateWebService
    {

        readonly ILifetimeScope parent;
        readonly StatelessServiceContext serviceContext;

        ILifetimeScope scope;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceContext"></param>
        /// <param name="parent"></param>
        public StateWebService(ILifetimeScope parent, StatelessServiceContext serviceContext = null)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.serviceContext = serviceContext;
        }

        /// <summary>
        /// Registers framework dependencies.
        /// </summary>
        /// <param name="services"></param>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new AutofacServiceProvider(scope = parent.BeginLifetimeScope(builder => builder.Populate(services)));
        }

        /// <summary>
        /// Gets the <see cref="ActorId"/> for the given request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ActorId GetActorId(HttpContext context)
        {
            return new ActorId(WebUtility.UrlDecode(context.Request.Path.Value.TrimStart('/')));
        }

        /// <summary>
        /// Gets an actor proxy to actor for the given state request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        async Task<IStateObjectActor> GetActorProxy(HttpContext context)
        {
            var actorId = GetActorId(context);
            var fabctx = await FabricRuntime.GetActivationContextAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
            return ActorProxy.Create<IStateObjectActor>(actorId);
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime)
        {
            app.Use(async (ctx, next) =>
            {
                switch (ctx.Request.Method)
                {
                    case "GET" when ctx.Request.Headers["Exclusive"] == "Acquire":
                        await GetExclusive(ctx, await GetActorProxy(ctx));
                        return;
                    case "GET" when ctx.Request.Headers["Exclusive"] == "Release":
                        await ReleaseExclusive(ctx, await GetActorProxy(ctx));
                        return;
                    case "GET":
                        await Get(ctx, await GetActorProxy(ctx));
                        return;
                    case "PUT":
                        await Set(ctx, await GetActorProxy(ctx));
                        return;
                    case "DELETE":
                        await Remove(ctx, await GetActorProxy(ctx));
                        return;
                    case "HEAD":
                        await ResetTimeout(ctx, await GetActorProxy(ctx));
                        return;
                }

                await next();
            });

            applicationLifetime.ApplicationStopped.Register(() => scope.Dispose());
        }

        /// <summary>
        /// Gets the Lock-Cookie request header value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        uint? GetLockCookie(HttpContext context) => uint.TryParse(context.Request.Headers["Lock-Cookie"], out uint l) ? (uint?)l : null;

        /// <summary>
        /// Gets the Timeout request header value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        TimeSpan? GetTimeout(HttpContext context) => uint.TryParse(context.Request.Headers["Timeout"], out uint l) ? (TimeSpan?)TimeSpan.FromMinutes(l) : null;

        /// <summary>
        /// Gets the Exclusive request header value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string GetExclusive(HttpContext context) => context.Request.Headers["Exclusive"];

        /// <summary>
        /// Gets the ExtraFlags request header value.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        uint? GetExtraFlags(HttpContext context) => uint.TryParse(context.Request.Headers["ExtraFlags"], out uint l) ? (uint?)l : null;

        /// <summary>
        /// Gets the data form the request body.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<byte[]> GetDataAsync(HttpContext context) => context.Request.Body.ReadAllBytesAsync();

        /// <summary>
        /// Configures a response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        void SetResponse(HttpContext context, Response response)
        {
            context.Response.ContentLength = 0;

            switch (response.Status)
            {
                case ResponseStatus.Ok:
                    context.Response.StatusCode = 200;
                    break;
                case ResponseStatus.Locked:
                    context.Response.StatusCode = 423;
                    break;
                case ResponseStatus.NotFound:
                    context.Response.StatusCode = 404;
                    break;
                case ResponseStatus.BadRequest:
                    context.Response.StatusCode = 400;
                    break;
            }

            if (response.Timeout != null)
                context.Response.Headers["Timeout"] = ((int)response.Timeout.Value.TotalMinutes).ToString();

            if (response.LockCookie != null)
                context.Response.Headers["LockCookie"] = response.LockCookie.Value.ToString();

            if (response.LockCreate != null)
                context.Response.Headers["LockDate"] = response.LockCreate.Value.Ticks.ToString();

            if (response.LockAge != null)
                context.Response.Headers["LockAge"] = ((int)response.LockAge.Value.TotalSeconds).ToString();
        }

        /// <summary>
        /// Configures a response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        void SetResponse(HttpContext context, DataResponse response)
        {
            SetResponse(context, (Response)response);

            if (response.ActionFlags != null)
                context.Response.Headers["ActionFlags"] = response.ActionFlags.Value.ToString();

            if (response.Data != null)
            {
                context.Response.ContentLength = response.Data.Length;
                context.Response.Body.WriteAsync(response.Data, 0, response.Data.Length);
            }
        }

        public async Task Get(HttpContext context, IStateObjectActor actor)
        {
            SetResponse(context, await actor.Get());
        }

        public async Task GetExclusive(HttpContext context, IStateObjectActor actor)
        {
            SetResponse(context, await actor.GetExclusive());
        }

        public async Task Set(HttpContext context, IStateObjectActor actor)
        {
            SetResponse(context, await actor.Set(GetLockCookie(context), await GetDataAsync(context), GetExtraFlags(context), GetTimeout(context)));
        }

        public async Task ReleaseExclusive(HttpContext context, IStateObjectActor actor)
        {
            SetResponse(context, await actor.ReleaseExclusive((uint)GetLockCookie(context)));
        }

        public async Task Remove(HttpContext context, IStateObjectActor actor)
        {
            SetResponse(context, await actor.Remove((uint?)GetLockCookie(context)));
        }

        public async Task ResetTimeout(HttpContext context, IStateObjectActor actor)
        {
            SetResponse(context, await actor.ResetTimeout());
        }

    }

}
