using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat
{
    internal partial class Assistant
    {
        internal readonly List<Object> k_ObjectAttachments = new();
        internal readonly List<LogData> k_ConsoleAttachments = new();

        CancellationTokenSource m_ContextCancelToken;

        internal bool HasNullAttachments(List<Object> contextAttachment)
        {
            if (contextAttachment == null)
                return false;

            return contextAttachment.Any(obj => obj == null);
        }

        public void SetObjectAttachments(params Object[] objects)
        {
            k_ObjectAttachments.Clear();
            if (objects == null)
            {
                return;
            }

            for (var i = 0; i < objects.Length; i++)
            {
                k_ObjectAttachments.Add(objects[i]);
            }
        }

        public void ClearContext()
        {
            k_ObjectAttachments.Clear();
            k_ConsoleAttachments.Clear();
        }

        /// <summary>
        /// Get the context string from the selected objects and selected console logs.
        /// </summary>
        /// <param name="maxLength"> The string length limitation. </param>
        /// <param name="contextBuilder"> The context builder reference for temporary context string creation. </param>
        /// <returns></returns>
        internal void GetAttachedContextString(ref ContextBuilder contextBuilder, bool stopAtLimit = false)
        {
            // Grab any selected objects
            var attachment = GetValidAttachment(k_ObjectAttachments);
            if (attachment.Count > 0)
            {
                foreach (var currentObject in attachment)
                {
                    var objectContext = new UnityObjectContextSelection();
                    objectContext.SetTarget(currentObject);

                    contextBuilder.InjectContext(objectContext,true);

                    if (stopAtLimit && contextBuilder.PredictedLength > MuseChatConstants.PromptContextLimit)
                    {
                        break;
                    }
                }
            }

            // Grab any console logs
            if (k_ConsoleAttachments != null)
            {
                foreach (var currentLog in k_ConsoleAttachments)
                {
                    var consoleContext = new ConsoleContextSelection();
                    consoleContext.SetTarget(currentLog);
                    contextBuilder.InjectContext(consoleContext, true);

                    if (stopAtLimit && contextBuilder.PredictedLength > MuseChatConstants.PromptContextLimit)
                    {
                        break;
                    }
                }
            }
        }

        internal async Task<EditorContextReport> GetContextModel(MuseConversationId conversationId, int maxLength, string prompt,
            CancellationToken cancellationToken, bool enableSmartContext = true)
        {
            // Initialize all context, if any context has changed, add it all
            var contextBuilder = new ContextBuilder();
            GetAttachedContextString(ref contextBuilder);

            // Add retrieved project settings
            if (enableSmartContext)
            {
                try
                {
#if MUSE_INTERNAL
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    InternalLog.Log("Started V2 Smart Context Extraction");
#endif

                    EditorContextReport editorContext = contextBuilder.BuildContext(maxLength);
                    var smartContextResponse = await m_Backend.SendSmartContext(conversationId, prompt, editorContext);

#if MUSE_INTERNAL
                    stopwatch.Stop();
                    InternalLog.Log(
                        $"Time taken for smart context call: {stopwatch.Elapsed}");
                    OnSmartContextCallDone?.Invoke(stopwatch.Elapsed, smartContextResponse);
                    stopwatch.Restart();
#endif

                    UnityDataUtils.CachePackageData(false);

                    for (var i = 0; !UnityDataUtils.PackageDataReady() && i < 10; i++)
                    {
                        await Task.Delay(10);
                    }

                    var smartContextMaxLength = maxLength;

                    var deduplicatedCalls = FunctionCall.Deduplicate(smartContextResponse.FunctionCalls).ToList();

                    ContextUtils.MergeSceneHierarchyExtractorCalls(deduplicatedCalls);

                    foreach (FunctionCall call in deduplicatedCalls)
                    {
#if MUSE_INTERNAL
                        InternalLog.Log(
                            $"Received Function {call.Function} with parameters {string.Join(", ", call.Parameters)}");
#endif

                        if (!MuseChatState.SmartContextToolbox.TryRunToolByName(call.Function, call.Parameters.ToArray(),
                                smartContextMaxLength, out IContextSelection result))
                        {
                            continue;
                        }

                        if (result == null)
                        {
                            continue;
                        }

                        // We don't know if the downsized or full payload will be used, use lower one for now:
                        smartContextMaxLength = Math.Max(0, smartContextMaxLength - result.DownsizedPayload?.Length ?? 0);

#if MUSE_INTERNAL
                        var info =
                            $"Called Function {call.Function} with parameters {string.Join(", ", call.Parameters)} and extracted the following:\n\n{result.Payload}";
                        InternalLog.Log(info);
                        stopwatch.Stop();
                        InternalLog.Log(
                            $"Time taken for smart context extraction: {stopwatch.Elapsed}");
                        OnSmartContextExtracted?.Invoke(stopwatch.Elapsed, call);
                        stopwatch.Restart();
#endif
                        contextBuilder.InjectContext(result, false);
                    }

                    if (smartContextResponse.FunctionCalls.Count == 0)
                    {
                        InternalLog.Log("No Smart Context Functions were called");
                    }
                }
                catch (Exception e)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    InternalLog.Log($"Failed to get smart context: {e.Message} {e.StackTrace}");
                }
            }

            var finalContext = contextBuilder.BuildContext(maxLength);

            InternalLog.Log($"Final Context ({contextBuilder.PredictedLength} character):\n\n {finalContext.ToJson()}");

            return finalContext;
        }

        internal List<Object> GetValidAttachment(List<Object> contextAttachments)
        {
            if (contextAttachments == null)
                return new List<Object>();

            if (contextAttachments.Any(obj => obj == null))
                return contextAttachments.Where(obj => obj != null).ToList();

            return contextAttachments;
        }
    }
}
