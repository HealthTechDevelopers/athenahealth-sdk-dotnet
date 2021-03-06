﻿using Newtonsoft.Json;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace AthenaHealth.Sdk.Models.Response
{
    public class Diagnose : DiagnoseForEncounter
    {
        [JsonProperty("supportslaterality")]
        public bool SupportsLaterality { get; set; }

        [JsonProperty("ranking")]
        public int Ranking { get; set; }

        [JsonProperty("laterality")]
        public string Laterality { get; set; }
    }
}
