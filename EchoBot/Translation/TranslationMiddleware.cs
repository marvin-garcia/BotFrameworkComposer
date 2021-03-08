// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Translation
{
    /// <summary>
    /// Middleware for translating text between the user and bot.
    /// Uses the Microsoft Translator Text API.
    /// </summary>
    public class TranslationMiddleware : IMiddleware
    {
        private readonly MicrosoftTranslator _translator;
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<string> _previousLanguage;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="translator">Translator implementation to be used for text translation.</param>
        /// <param name="languageStateProperty">State property for current language.</param>
        public TranslationMiddleware(MicrosoftTranslator translator, UserState userState)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            if (userState == null)
                throw new ArgumentNullException(nameof(userState));

            _userState = userState;           
            _previousLanguage = userState.CreateProperty<string>("LanguagePreference");
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
                throw new ArgumentNullException(nameof(turnContext));

            var shouldTranslate = await ShouldTranslateAsync(turnContext, cancellationToken);
            
            if (shouldTranslate)
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    string previousLanguage = await _previousLanguage.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage, cancellationToken) ?? TranslationSettings.DefaultLanguage;
                    string currentLanguage = await _translator.DetectLanguageAsync(turnContext.Activity.Text);
                    await _previousLanguage.SetAsync(turnContext, currentLanguage, cancellationToken);
                    await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

                    string translatedText = await _translator.TranslateAsync(turnContext.Activity.Text, previousLanguage, cancellationToken: cancellationToken);

                    string inTranslation = await _translator.TranslateAsync("in", previousLanguage, "en-us", cancellationToken: cancellationToken);
                    string meansTranslation = await _translator.TranslateAsync("means", previousLanguage, "en-us", cancellationToken: cancellationToken);

                    turnContext.Activity.Text = $"'{turnContext.Activity.Text}' {meansTranslation.ToLower()} '{translatedText}' {inTranslation.ToLower()} '{currentLanguage}'";
                }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> ShouldTranslateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                string previousLanguage = await _previousLanguage.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage, cancellationToken) ?? TranslationSettings.DefaultLanguage;
                string currentLanguage = await _translator.DetectLanguageAsync(turnContext.Activity.Text);
                return previousLanguage != currentLanguage;
            }
            else
                return false;
        }
    }
}
