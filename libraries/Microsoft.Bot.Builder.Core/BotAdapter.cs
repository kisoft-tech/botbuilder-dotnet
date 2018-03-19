﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public abstract class BotAdapter
    {
        protected readonly MiddlewareSet _middlewareSet = new MiddlewareSet();

        public BotAdapter() : base()
        {
        }

        /// <summary>
        /// Register middleware with the bot
        /// </summary>
        /// <param name="middleware"></param>
        public BotAdapter Use(IMiddleware middleware)
        {
            _middlewareSet.Use(middleware);
            return this;
        }

        /// <summary>
        /// implement send activities to the conversation
        /// </summary>        
        /// <param name="activities">Set of activities being sent</param>
        /// <returns>Array of ResourcesResponse containing the Ids of the sent activities. For
        /// most bots, these Ids are server-generated and enable Update and Delete to be 
        /// called against the remote resources.</returns>
        public abstract Task<ResourceResponse[]> SendActivity(IBotContext context, Activity[] activities);

        /// <summary>
        /// Implement updating an activity in the conversation
        /// </summary>        
        /// <param name="activity">New replacement activity. The activity should already have it's ID information populated. </param>
        /// <returns></returns>
        /// <returns>ResourcesResponses containing the Id of the sent activity. For
        /// most bots, this Id is server-generated and enables future Update and Delete calls
        /// against the remote resources.</returns>
        public abstract Task<ResourceResponse> UpdateActivity(IBotContext context, Activity activity);

        /// <summary>
        /// Implement deleting an activity in the conversation
        /// </summary>
        /// <param name="reference">Conversation reference of the activity being deleted.  </param>
        /// <returns></returns>
        public abstract Task DeleteActivity(IBotContext context, ConversationReference reference);


        /// <summary>
        /// Called by base class to run pipeline around a context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected async Task RunPipeline(IBotContext context, Func<IBotContext, Task> callback = null, CancellationTokenSource cancelToken = null)
        {
            BotAssert.ContextNotNull(context);
            
            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Request != null)
            {
                await _middlewareSet.ReceiveActivityWithStatus(context, callback).ConfigureAwait(false);
            }
            else
            {
                // call back to caller on proactive case
                if (callback != null)
                {
                    await callback(context).ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual async Task CreateConversation(string channelId, Func<IBotContext, Task> callback)
        {
            throw new NotImplementedException("Adapter does not support CreateConversation with this arguments");
        }

        /// <summary>
        /// Create proactive context around conversation reference
        /// All middleware pipelines will be processed
        /// </summary>
        /// <param name="reference">reference to create context around</param>
        /// <param name="callback">callback where you can continue the conversation</param>
        /// <returns>task when completed</returns>
        public virtual Task ContinueConversation(ConversationReference reference, Func<IBotContext, Task> callback)
        {
            var context = new BotContext(this, reference.GetPostToBotMessage());
            return RunPipeline(context, callback);
        }
    }
}
