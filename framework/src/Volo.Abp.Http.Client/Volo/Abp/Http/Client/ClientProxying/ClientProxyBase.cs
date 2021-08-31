﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;

namespace Volo.Abp.Http.Client.ClientProxying
{
    public class ClientProxyBase<TService> : ITransientDependency
    {
        public IAbpLazyServiceProvider LazyServiceProvider { get; set; }

        protected IHttpProxyExecuter HttpProxyExecuter => LazyServiceProvider.LazyGetRequiredService<IHttpProxyExecuter>();
        protected IClientProxyApiDescriptionFinder ClientProxyApiDescriptionFinder => LazyServiceProvider.LazyGetRequiredService<IClientProxyApiDescriptionFinder>();

        protected virtual async Task RequestAsync(string methodName, params object[] arguments)
        {
            await HttpProxyExecuter.MakeRequestAsync(await BuildHttpProxyExecuterContext(methodName, arguments));
        }

        protected virtual async Task<T> RequestAsync<T>(string methodName, params object[] arguments)
        {
            return await HttpProxyExecuter.MakeRequestAndGetResultAsync<T>(await BuildHttpProxyExecuterContext(methodName, arguments));
        }

        protected virtual async Task<HttpProxyExecuterContext> BuildHttpProxyExecuterContext(string methodName, params object[] arguments)
        {
            var actionDescriptionKey = $"{typeof(TService).FullName}.{methodName}";
            var action = await ClientProxyApiDescriptionFinder.FindActionAsync(actionDescriptionKey);

            return new HttpProxyExecuterContext(action, BuildArguments(action, arguments), typeof(TService));
        }

        protected virtual Dictionary<string, object> BuildArguments(ActionApiDescriptionModel action, object[] arguments)
        {
            var parameters = action.Parameters.GroupBy(x => x.NameOnMethod).Select(x => x.Key).ToList();
            var dict = new Dictionary<string, object>();

            for (var i = 0; i < parameters.Count; i++)
            {
                dict[parameters[i]] = arguments[i];
            }

            return dict;
        }
    }
}