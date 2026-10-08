using System.Text.Json.Serialization;

namespace GwCustom.Classes {
    public class DmsDocumentsInsertRequest
    {
        [JsonPropertyName("objectTypeId")]
        public string? ObjectTypeId { get; set; }

        [JsonPropertyName("mainTypeId")]
        public string? MainTypeId { get; set; }

        [JsonPropertyName("fields")]
        public object? Fields { get; set; }
    }


    public class FieldsSection
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field1 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field2 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field3 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field4 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field5 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field6 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field7 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field8 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field9 { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Field10 { get; set; } = null;
    }


    /// <summary>
    /// Suche nach bestimmtem Wert
    /// </summary>
    public class InternalNameValue
    {
        [JsonPropertyName("internalName")]
        public string? InternalName { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}