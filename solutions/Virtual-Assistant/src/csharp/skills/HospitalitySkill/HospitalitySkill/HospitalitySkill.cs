﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using HospitalitySkill.Dialogs.Main;
using HospitalitySkill.Dialogs.Main.Resources;
using HospitalitySkill.Dialogs.Shared.Resources;
using HospitalitySkill.ServiceClients;
using HospitalitySkill.Dialogs.CheckIn;
using HospitalitySkill.Dialogs.CheckOut;
using HospitalitySkill.Dialogs.OrderFood;
using HospitalitySkill.Dialogs.RequestServices;

namespace HospitalitySkill
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class HospitalitySkill : IBot
    {
        private readonly SkillConfigurationBase _services;
        private readonly ResponseManager _responseManager;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly IBotTelemetryClient _telemetryClient;
        private IServiceManager _serviceManager;
        private DialogSet _dialogs;
        private bool _skillMode;

        public HospitalitySkill(SkillConfigurationBase services,
            ConversationState conversationState,
            UserState userState,
            IBotTelemetryClient telemetryClient,
            bool skillMode = false,
            ResponseManager responseManager = null,
            IServiceManager serviceManager = null)
        {
            _skillMode = skillMode;
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _serviceManager = serviceManager ?? new ServiceManager();

            if (responseManager == null)
            {
                responseManager = new ResponseManager(
                    new IResponseIdCollection[]
                    {
                        new MainResponses(),
                        new SharedResponses(),
                        new CheckInDialogResponses(),
                        new CheckOutDialogResponses(),
                        new OrderFoodDialogResponses(),
                        new RequestServicesDialogResponses()
                    }, _services.LocaleConfigurations.Keys.ToArray());
            }

            _responseManager = responseManager;
            _dialogs = new DialogSet(_conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            _dialogs.Add(new MainDialog(_services, _responseManager, _conversationState, _userState, _telemetryClient, _serviceManager, _skillMode));
        }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="TaskItem"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dc = await _dialogs.CreateContextAsync(turnContext);

            if (dc.ActiveDialog != null)
            {
                var result = await dc.ContinueDialogAsync();
            }
            else
            {
                await dc.BeginDialogAsync(nameof(MainDialog));
            }
        }
    }
}