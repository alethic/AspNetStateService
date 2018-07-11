using System;
using System.Net;
using System.Threading.Tasks;

using AspNetStateService.Interfaces;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;
using Cogito.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetStateService.AspNetCore
{

    /// <summary>
    /// Base ASP.Net Core Web Service implementation around a <see cref="IStateObject"/>.
    /// </summary>
    [RegisterAs(typeof(StateWebService))]
    public class StateWebService
    {

        readonly ILifetimeScope parent;
        readonly Lazy<IStateObjectProvider> stateObjectProvider;

        ILifetimeScope scope;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="stateObjectProvider"></param>
        public StateWebService(ILifetimeScope parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.stateObjectProvider = new Lazy<IStateObjectProvider>(() => scope.Resolve<IStateObjectProvider>());
        }

        /// <summary>
        /// Registers framework dependencies.
        /// </summary>
        /// <param name="services"></param>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var p = new AutofacServiceProvider(scope = parent.BeginLifetimeScope(builder => builder.Populate(services)));
            var f = scope.Resolve<ILoggerFactory>();
            return p;
        }

        /// <summary>
        /// Gets a reference to the <see cref="IStateObject"/> implementation.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected Task<IStateObject> GetStateObjectAsync(HttpContext context)
        {
            var str = context.Request.Path.Value.TrimStart('/');
            var uri = WebUtility.UrlDecode(str);
            return stateObjectProvider.Value.GetStateObjectAsync(uri);
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
                var state = await GetStateObjectAsync(ctx);
                if (state == null)
                    throw new InvalidOperationException("Could not resolve state object for request.");

                switch (ctx.Request.Method)
                {
                    case "GET" when ctx.Request.Headers["Exclusive"] == "Acquire":
                        await GetExclusive(ctx, state);
                        return;
                    case "GET" when ctx.Request.Headers["Exclusive"] == "Release":
                        await ReleaseExclusive(ctx, state);
                        return;
                    case "GET":
                        await Get(ctx, state);
                        return;
                    case "PUT":
                        await Set(ctx, state);
                        return;
                    case "DELETE":
                        await Remove(ctx, state);
                        return;
                    case "HEAD":
                        await ResetTimeout(ctx, state);
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

            if (response.LockTime != null)
                context.Response.Headers["LockDate"] = response.LockTime.Value.Ticks.ToString();

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

        public async Task Get(HttpContext context, IStateObject actor)
        {
            SetResponse(context, await actor.Get());
        }

        public async Task GetExclusive(HttpContext context, IStateObject actor)
        {
            SetResponse(context, await actor.GetExclusive());
        }

        public async Task Set(HttpContext context, IStateObject actor)
        {
            SetResponse(context, await actor.Set(GetLockCookie(context), await GetDataAsync(context), GetExtraFlags(context), GetTimeout(context)));
        }

        public async Task ReleaseExclusive(HttpContext context, IStateObject actor)
        {
            SetResponse(context, await actor.ReleaseExclusive((uint)GetLockCookie(context)));
        }

        public async Task Remove(HttpContext context, IStateObject actor)
        {
            SetResponse(context, await actor.Remove((uint?)GetLockCookie(context)));
        }

        public async Task ResetTimeout(HttpContext context, IStateObject actor)
        {
            SetResponse(context, await actor.ResetTimeout());
        }

    }

}
