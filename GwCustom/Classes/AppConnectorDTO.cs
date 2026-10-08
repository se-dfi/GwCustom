using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GwCustom.Classes {
    /// <summary>
    /// Request DTO, um nach Dokumenten in enaio zu suchen
    /// </summary>
    public class EnaioDocumentSearchRequestDTO {
        /// <summary>
        /// Suchfelder
        /// </summary>
        [JsonPropertyName("query")]
        public EnaioDocumentQueryDTO? Query { get; set; }


        /// <summary>
        /// Zusätzliche Suchfelder
        /// </summary>
        [JsonPropertyName("additionalQueries")]
        public EnaioAdditionalQueriesDTO? AdditionalQueries { get; set; }
    }


    /// <summary>
    /// Hauptobjekt für enaio Dokumente
    /// </summary>
    public class EnaioDocumentQueryDTO {
        /// <summary>
        /// Id des Objekttypen in enaio
        /// </summary>
        [JsonPropertyName("objectTypeId")]
        public string? ObjectTypeId { get; set; }


        /// <summary>
        /// Id des Objektes
        /// </summary>
        [JsonPropertyName("osid")]
        public string? OsId { get; set; }


        /// <summary>
        /// Konfiguration des Suchergebnisses
        /// </summary>
        [JsonPropertyName("result_config")]
        public EnaioResultConfigDTO? ResultConfig { get; internal set; }


        /// <summary>
        /// Suchfelder
        /// </summary>
        [JsonPropertyName("fields")]
        public EnaioSearchFieldsDTO? Fields { get; set; }
    }


    /// <summary>
    /// enthält weitere Suchbedingungen
    /// </summary>
    public class EnaioAdditionalQueriesDTO : List<EnaioDocumentQueryDTO> {
    }


    /// <summary>
    /// Konfiguration der Suchergebnisse
    /// </summary>
    public class EnaioResultConfigDTO {
        /// <summary>
        /// Liste der Felder, die im Ergebnis enthalten sein sollen
        /// </summary>
        [JsonPropertyName("fieldsschema")]
        public EnaioFieldsSchemaListDTO? Fieldsschema { get; set; }


        /// <summary>
        /// Modus des Ergebnisses
        /// </summary>
        [JsonPropertyName("fieldsschema_mode")]
        public string? FieldsSchemaMode { get; set; }


        /// <summary>
        /// Kenner, ob SystemFelder enthalten sein sollen
        /// </summary>
        [JsonPropertyName("systemFields")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public uint SystemFields { get; set; }


        /// <summary>
        /// Kenner, ob die Basisparameter enthalten sein sollen
        /// </summary>
        [JsonPropertyName("baseParameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public uint BaseParameters { get; set; }


        /// <summary>
        /// Kenner, ob die Datei-Eigenschaften enthalten sein sollen
        /// </summary>
        [JsonPropertyName("fileProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public uint FileProperties { get; set; }


        /// <summary>
        /// Kenner ob die Rechte enthalten sein sollen
        /// </summary>
        [JsonPropertyName("rights")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public uint Rights { get; set; }


        /// <summary>
        /// Maximale Anzahl der Ergebnisse
        /// </summary>
        [JsonPropertyName("maxhits")]
        public uint MaxHits { get; set; }



        /// <summary>
        /// Trefferoffset für das Blättern durch die Treffer
        /// </summary>
        [JsonPropertyName("offset")]
        public uint Offset { get; set; }
    }


    /// <summary>
    /// Felder für die Suche in Enaio
    /// </summary>
    public class EnaioSearchFieldsDTO : Dictionary<string, EnaioSearchValueDTO> {
        /// <summary>
        /// Erzeugt ein EnaioSearchValueDTO und fügt dieses zur Suche hinzu
        /// </summary>
        /// <param name="field">Feld, in dem gesucht wird</param>
        /// <param name="value">Wert, nach dem gesucht wird</param>
        /// <param name="negate">true, wenn es eine "NOT"-Bedingung sein soll</param>
        /// <returns></returns>
        public EnaioSearchValueDTO PutValueObject(string internalName, object? value, bool negate = false) {
            EnaioSearchValueDTO obj = new() { InternalName = internalName, Value = value, Negate = negate };
            Add($"Field{Count}", obj);
            return obj;
        }
    }


    /// <summary>
    /// Wert in einem DTO für die Suche in einem Feld
    /// </summary>
    public class EnaioSearchValueDTO : EnaioValueDTO {
        /// <summary>
        /// true, wenn es eine "NOT"-Bedingung sein soll
        /// </summary>
        [JsonPropertyName("negate")]
        public bool Negate { get; set; }
    }


    /// <summary>
    /// Der Enaio AppConnector will manchmal Werte in einem Objekt haben
    /// </summary>
    public class EnaioValueDTO {
        /// <summary>
        /// interner Name des Feldes
        /// </summary>
        [JsonPropertyName("internalName")]
        public string? InternalName { get; set; }


        /// <summary>
        /// Wert
        /// </summary>
        [JsonPropertyName("value")]
        public object? Value { get; set; }
    }


    /// <summary>
    /// enthält Werte der Konfiguration des Schemas des Ergebnisses
    /// </summary>
    public class EnaioFieldsSchemaListDTO : List<EnaioFieldsSchemaDTO> {
        /// <summary>
        /// Fügt einen neuen Eintrag hinzu oder führt die Eigenschaften mit einen bestehenden mit dem selben EnaioFieldsSchemaDTO zusammen.
        /// Beim Zusammenführen werden alle Werte des bestehenden überschrieben, wenn der neue Wert nicht null ist
        /// </summary>
        /// <param name="item">Neuer Eintrag</param>
        public void AddOrMerge(EnaioFieldsSchemaDTO item) {
            EnaioFieldsSchemaDTO? existing = Find(i => i.InternalName == item.InternalName);
            if (existing != null) {
                //Existiert bereits -> zusammenführen
                if (item.SortOrder != null) {
                    existing.SortOrder = item.SortOrder;
                }
                if (item.SortPos != null) {
                    existing.SortPos = item.SortPos;
                }
            } else {
                //Neu anlegen
                Add(item);
            }
        }
    }


    /// <summary>
    /// enthält einen Wert für die Konfiguration des Schemas des Ergebnisses
    /// </summary>
    /// <remarks>
    /// Konstruktor mit Konfiguration aus ResultConfiguration
    /// </remarks>
    /// <param name="internalName">interner Name des Feldes, welches im Ergebnis erscheinen soll</param>
    /// <param name="configData">Tupel aus Feldnamen, <see cref="SortOrder"/> und <see cref="SortPos"/></param>
    public class EnaioFieldsSchemaDTO(string internalName, Tuple<string, string, uint>? configData = null) {


        /// <summary>
        /// interner Name des Feldes, welches im Ergebnis erscheinen soll
        /// </summary>
        [JsonPropertyName("internalName")]
        public string InternalName { get; set; } = internalName;


        /// <summary>
        /// Reihenfolge aufsteigend oder absteigend
        /// </summary>
        [JsonPropertyName("sort_order")]
        public string? SortOrder { get; set; } = configData?.Item2;


        /// <summary>
        /// Position in der Sortierreihenfolge (größer 0)
        /// </summary>
        [JsonPropertyName("sort_pos")]
        public uint? SortPos { get; set; } = configData?.Item3;


        /// <inheritdoc/>
        public override string ToString() {
            return "EnaioFieldsSchemaDTO (InternalName: " + InternalName + "; SortOrder: " + SortOrder + "; SortPos: " + SortPos + ")";
        }
    }


    /// <summary>
    /// DTO, um eine Rückgabe von documents/search aus enaio zu lesen
    /// </summary>
    public class EnaioDocumentsSearchResponseDTO {
        /// <summary>
        /// ID des Dokumentes in Enaio als String (wird so von der API zurückgegeben)
        /// </summary>
        [JsonPropertyName("osid")]
        public string? OsIdString { get; set; }


        /// <summary>
        /// ID des Dokumentes in Enaio
        /// </summary>
        [JsonIgnore]
        public ulong OsId { get => ulong.Parse(string.IsNullOrEmpty(OsIdString) ? "0" : OsIdString); set => OsIdString = value.ToString(); }


        /// <summary>
        /// ID des Objekttyps
        /// </summary>
        [JsonPropertyName("objectTypeId")]
        public string? ObjectTypeId { get; set; }


        /// <summary>
        /// Objekttyp
        /// </summary>
        [JsonPropertyName("objectType")]
        public string? ObjectType { get; set; }


        /// <summary>
        /// Interner Name
        /// </summary>
        [JsonPropertyName("internalName")]
        public string? InternalName { get; set; }


        /// <summary>
        /// Anzeigename
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }


        /// <summary>
        /// EcmSimpleFields
        /// </summary>
        [JsonPropertyName("ecmSimpleFields")]
        public EnaioSimpleFieldCollectionDTO? EcmSimpleFields { get; set; }


        /// <summary>
        /// EcmTableFields
        /// </summary>
        [JsonPropertyName("ecmTableFields")]
        public EnaioTableFieldsDTO? EcmTableFields { get; set; }


        /// <summary>
        /// Basisparameter
        /// </summary>
        [JsonPropertyName("baseParameters")]
        public EnaioBaseParameterCollectionDTO? BaseParameters { get; set; }


        /// <summary>
        /// Systemfelder
        /// </summary>
        [JsonPropertyName("systemFields")]
        public EnaioBaseParameterCollectionDTO? SystemFields { get; set; }


        /// <summary>
        /// Übergeordneter Schrank
        /// </summary>
        [JsonPropertyName("parentCabinetType")]
        public string? ParentCabinetType { get; set; }


        /// <summary>
        /// Übergeordneter Schrank
        /// </summary>
        [JsonPropertyName("parentCabinetKey")]
        public string? ParentCabinetKey { get; set; }


        /// <summary>
        /// File-Properties
        /// </summary>
        [JsonPropertyName("fileProperties")]
        public EnaioBaseParameterCollectionDTO? FileProperties { get; set; }


        /// <summary>
        /// Flags
        /// </summary>
        [JsonPropertyName("objectFlagsValue")]
        public int? ObjectFlagsValue { get; set; }


        /// <summary>
        /// Varianten
        /// </summary>
        [JsonPropertyName("variantInformation")]
        public EnaioBaseParameterCollectionDTO? VariantInformation { get; set; }
    }


    /// <summary>
    /// Liste von EnaioSimpleFieldDTO
    /// </summary>
    public class EnaioSimpleFieldCollectionDTO : List<EnaioSimpleFieldDTO> {
        /// <summary>
        /// Gibt ein EnaioSimpleFieldDTO aus der Liste zurück oder bearbeitet den entsprechenden Eintrag
        /// </summary>
        /// <param name="internalName">Interner Name des Feldes</param>
        /// <returns></returns>
        public EnaioSimpleFieldDTO? this[string internalName] {
            get {
                return this.FirstOrDefault(item => item.InternalName == internalName);
            }

            set {
                //Index des evtl. existierenden Feldes suchen
                int existingIndex = FindIndex(item => item.InternalName == internalName);

                //Feld löschen, falls es bereits existiert (neuer Wert wird ggf. an der selben Position wieder eigefügt)
                if (existingIndex >= 0) {
                    RemoveAt(existingIndex);
                }

                if (value != null) {
                    EnaioSimpleFieldDTO newValue = value ?? new EnaioSimpleFieldDTO();
                    newValue.InternalName = internalName;

                    //Nur neu hinzufügen, wenn der Wert nicht auf null gesetzt wurde
                    if (existingIndex >= 0) {
                        //Der alte Wert wird ersetzt. Der neue muss also an der selben Position überschrieben werden
                        Insert(existingIndex, newValue); //kann eh nicht null sein, aber "!" funktioniert irgendwie nicht
                    } else {
                        //Wert war vorher schon nicht da. Also wird der neue einfach an die Liste angehängt
                        Add(newValue);
                    }
                }
            }
        }
    }


    /// <summary>
    /// EnaioTableFields enthält in untergeordnetem Array "value" eine Liste von EnaioSimplefields
    /// </summary>
    public class EnaioTableFieldsDTO : List<EnaioTableFieldDTO> {
    }
    public class EnaioTableFieldDTO {
        /// <summary>
        /// Wert des Feldes
        /// </summary>
        [JsonPropertyName("value")]
        public List<EnaioTableFieldContentDTO>? Value { get; set; }
    }
    public class EnaioTableFieldContentDTO {
        /// <summary>
        /// Wert des Feldes
        /// </summary>
        [JsonPropertyName("ecmSimpleFields")]
        public List<EnaioSimpleFieldDTO>? EcmSimpleFields { get; set; }
    }


    /// <summary>
    /// Informationen über ein Feld in Enaio und dessen Wert.
    /// Wird in der Enaio-API an verschiedenen Stellen genutzt.
    /// </summary>
    public struct EnaioSimpleFieldDTO {
        /// <summary>
        /// Wert des Feldes
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }


        /// <summary>
        /// Anzeigename des Feldes
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }


        /// <summary>
        /// Interner Name des Feldes
        /// </summary>
        [JsonPropertyName("internalName")]
        public string InternalName { get; set; }


        /// <summary>
        /// Typ des Feldes
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }


        /// <inheritdoc/>
        public override readonly string ToString() {
            return "EnaioSimpleFieldDTO " + InternalName + " (" + Type + "): " + Value;
        }
    }


    /// <summary>
    /// Sammlung von EnaioBaseParameterDTO. Wird in einigen Models von Enaio verwendet. Auf diesem Weg konnte ein eigener Indexer hinzugefügt werden, um die Werte leichter auslesen zu können.
    /// </summary>
    public class EnaioBaseParameterCollectionDTO : List<EnaioBaseParameterDTO> {
        /// <summary>
        /// Gibt den Wert eines EnaioBaseParameterDTO zurück
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string? this[string type] {
            get {
                return this.FirstOrDefault(item => item.Type == type)?.Value;
            }
        }
    }


    /// <summary>
    /// Key-Value Paar, das in verschiedenen Enaio-Schnittstellen verwendet wird
    /// </summary>
    public class EnaioBaseParameterDTO {
        /// <summary>
        /// Typ/Name des Feldes
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }


        /// <summary>
        /// Wert des Feldes
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }


        /// <inheritdoc/>
        public override string ToString() {
            return "EnaioBaseParameterDTO " + Type + ": " + Value;
        }
    }
}