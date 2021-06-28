// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EdFi.Admin.LearningStandards.Core.Models
{
    public class ChangesAvailableResponse : IChangesAvailableResponse
    {
        public ChangesAvailableResponse(IChangesAvailableInformation changesAvailableInformation)
            : this(null as string, changesAvailableInformation, HttpStatusCode.OK)
        {
        }

        public ChangesAvailableResponse(
            IResponse response,
            IChangesAvailableInformation changesAvailableInformation)
            : this(response.ErrorMessage, changesAvailableInformation, response.StatusCode)
        {
        }

        public ChangesAvailableResponse(string errorMessage, IChangesAvailableInformation changesAvailableInformation, HttpStatusCode statusCode)
        {
            ErrorMessage = errorMessage;
            ChangesAvailableInformation = changesAvailableInformation;

            if (ChangesAvailableInformation != null)
            {
                var sb = new StringBuilder();
                sb.AppendFormat(
                      "Changes available: {0}",
                      changesAvailableInformation.Available)
                  .AppendLine()
                  .AppendFormat(
                      "Current Sequence Id: {0}",
                      changesAvailableInformation.Current.Id)
                  .AppendLine()
                  .AppendFormat(
                      "Highest Available Sequence Id: {0}",
                      changesAvailableInformation.MaxAvailable.Id)
                  .AppendLine();

                Content = sb.ToString();
            }

            StatusCode = statusCode;
        }

        public bool IsSuccess => ErrorMessage == null;

        public string ErrorMessage { get; }
        public string Content { get; }
        public HttpStatusCode StatusCode { get; }
        public List<IResponse> InnerResponses => new List<IResponse>();
        public IChangesAvailableInformation ChangesAvailableInformation { get; }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage : Content;
        }
    }
}
