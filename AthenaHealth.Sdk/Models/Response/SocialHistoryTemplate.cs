﻿using AthenaHealth.Sdk.Models.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AthenaHealth.Sdk.Models.Response
{
    public class SocialHistoryTemplate
    {
        /// <summary>
        /// List of questions contained by this template
        /// </summary>
        [JsonProperty(PropertyName = "questions")]
        public Question[] Questions { get; set; }

        /// <summary>
        /// ID for this social history template
        /// </summary>
        [JsonProperty(PropertyName = "templateid")]
        public int Id { get; set; }

        /// <summary>
        /// Name for this social history template
        /// </summary>
        [JsonProperty(PropertyName = "templatename")]
        public string TemplateName { get; set; }

        public class Question
        {
            [JsonProperty(PropertyName = "options")]
            private IList<IDictionary<string, string>> _options;

            /// <summary>
            /// Unique ID for this question within this template.
            /// </summary>
            [JsonProperty(PropertyName = "questionid")]
            public int Id { get; set; }

            /// <summary>
            /// Unique key for this question, can exist in multiple templates.
            /// </summary>
            [JsonProperty(PropertyName = "key")]
            public string Key { get; set; }

            /// <summary>
            /// Human readable question
            /// </summary>
            [JsonProperty(PropertyName = "question")]
            public string QuestionText { get; set; }

            /// <summary>
            /// Display ordering for this question within this template
            /// </summary>
            [JsonProperty(PropertyName = "ordering")]
            public int? Ordering { get; set; }

            /// <summary>
            /// Input type for this question. Valid values are DROPDOWN, NUMERIC, FREETEXT, YESNO, and DATE
            /// </summary>
            [JsonProperty(PropertyName = "inputtype")]
            public InputTypeEnum InputType { get; set; }

            /// <summary>
            /// Indicate category of question MENTAL, FUNCTIONAL etc.
            /// </summary>
            [JsonProperty(PropertyName = "socialhistorystatus")]
            public string SocialHistoryStatus { get; set; }

            /// <summary>
            /// If the inputtype is DROPDOWN, this contains the list of key => value selections.
            /// </summary>
            public IDictionary<string, string> Options { get; set; }

            [OnDeserialized]
            internal void OnDeserializedMethod(StreamingContext context)
            {
                Options = _options.SelectMany(x => x)
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}