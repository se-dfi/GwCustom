using GwCustom.Classes;
using GwCustom.Models;
using GwCustomUI.Classes;
using RestSharp;
using RestSharp.Serializers.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Custom
{
    /// <summary>
    /// enaio AppConnector
    /// </summary>
    public static partial class AppConnector
    {
        [GeneratedRegex(SENTGUIDPATTERN)]
        private static partial Regex SentGuidRegex();

        [GeneratedRegex(@"(?<!^)\s*(?!$)")]
        private static partial Regex SubjectSearchRegex();

        /// <summary>
        /// URL und Timeout zum enaio AppConnector
        /// </summary>
        private static readonly RestClientOptions restoptions = new(CommonUtil.GetConfig().RestUrlEnaio)
        {
            Timeout = TimeSpan.FromMilliseconds(CommonUtil.GetConfig().RestTimeout)
        };

        /// <summary>
        /// JSON-Serialisierungsoptionen, thread-safe nach Erstverwendung
        /// </summary>
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };

        /// <summary>
        /// Shared RestSharp Client, wiederverwendet HTTP-Connection-Pool und vermeidet Socket-/Thread-Exhaustion
        /// </summary>
        private static readonly RestClient restClient = new(restoptions, configureSerialization: s => s.UseSystemTextJson(jsonSerializerOptions));

        /// <summary>
        /// Gemeinsamen RestSharp Client bereitstellen (RestClient ist thread-safe)
        /// </summary>
        /// <returns>RestSharp Client</returns>
        private static RestClient GetRestClient()
        {
            return restClient;
        }

        /// <summary>
        /// Verbindungstest zum enaio AppConnector mit Retry
        /// </summary>
        /// <returns>AppConnector erreichbar</returns>
        public static async Task<bool> CheckReachable()
        {
            // Request mit Fehlerunterdrückung und Rückgabe
            RestRequest request = new("api/serviceinfo/ping", Method.Get)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            RestResponse? response = null;
            try
            {
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        response = await restClient.GetAsync(request).ConfigureAwait(false);
                        break;
                    }
                    catch (HttpRequestException)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }
            catch
            {
                return false;
            }

            return response != null && (int)response.StatusCode < 400;
        }

        /// <summary>
        /// Setzen der Guid in enaio anhand enaio Id
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static async Task<bool> SetGuidByDocumentId(ulong id, string? guid)
        {
            // Abbruch ohne enaio Id oder mit ungültiger GUID
            if (id <= 0 || string.IsNullOrEmpty(guid) || !SentGuidRegex().IsMatch(guid))
            {
                return false;
            }

            // Aktualisierungsfelder
            EnaioSearchFieldsDTO updateFields = [];
            updateFields.PutValueObject(DMSFIELD_SENTGUID, guid);

            // Payload erstellen
            EnaioDocumentQueryDTO body = new()
            {
                ObjectTypeId = $"{CommonUtil.GetConfig().UniversalDocumentId}",
                Fields = updateFields
            };

            // Debug: Payload ausgeben
            // string payload = JsonSerializer.Serialize(body, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/update/{id}", Method.Post)
            {
                AlwaysMultipartFormData = true
            };
            request.AddUrlSegment("id", id);
            request.AddHeader("Accept", "*/*"); // essenziell
            request.AddJsonBody(body);
            RestResponse response = await client.PostAsync(request).ConfigureAwait(false);

            return response.StatusCode.Equals(HttpStatusCode.OK);
        }

        /// <summary>
        /// Dokument am überbegenen Standort oder in der Outbox erstellen, optional mit Datei
        /// </summary>
        /// <param name="message"></param>
        /// <param name="file"></param>
        /// <returns>Id des erstellten Dokuments, ansonsten 0</returns>
        /// <exception cref="Exception"></exception>
        public static async Task<ulong> CreateDocumentAsync(GwDocument gwDocument, DmsLocation? location, string file = "")
        {
            // Vorhandene Outbox ermitteln, ansonsten erstellen
            location ??= await GetFolderOutboxAsync().ConfigureAwait(false) ?? await CreateFolderOutboxAsync().ConfigureAwait(false);

            // Abbruch falls kein Standort vorhanden
            if (location == null)
            {
                throw new Exception(MultiLanguage.GetText("nodmslocation"));
            }

            // Payload erstellen
            DmsDocumentsInsertRequest body = new()
            {
                ObjectTypeId = CommonUtil.GetConfig().UniversalDocumentId.ToString(),
                MainTypeId = "6", // E-Mail Haupttyp, gilt auch für Termine
                Fields = new FieldsSection()
                {
                    Field1 = new InternalNameValue() { InternalName = DMSFIELD_MAILDATE, Value = gwDocument.CreationDate },
                    Field2 = new InternalNameValue() { InternalName = DMSFIELD_MAILTIME, Value = gwDocument.CreationTime },
                    Field3 = new InternalNameValue() { InternalName = DMSFIELD_MAILSUBJECT, Value = gwDocument.Subject },
                    Field4 = new InternalNameValue() { InternalName = DMSFIELD_MAILFROM, Value = gwDocument.MailAddressFrom },
                    Field5 = new InternalNameValue() { InternalName = DMSFIELD_MAILTO, Value = String.Join("; ", gwDocument.MailAddressesTo) },
                    Field6 = new InternalNameValue() { InternalName = DMSFIELD_MAILCC, Value = String.Join("; ", gwDocument.MailAddressesCc) },
                    Field7 = new InternalNameValue() { InternalName = DMSFIELD_SENTGUID, Value = gwDocument.SentGuid },
                    Field8 = new InternalNameValue() { InternalName = DMSFIELD_CREATOR, Value = Environment.UserName.ToUpper() }
                }
            };

            // Debug: Payload ausgeben
            // string payload = JsonSerializer.Serialize(body, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/insert/{locationid}", Method.Post)
            {
                AlwaysMultipartFormData = true
            };
            request.AddUrlSegment("locationid", location.Id.ToString());
            request.AddParameter("archivable", true, ParameterType.QueryString); // Dokument archivierbar setzen
            request.AddHeader("Accept", "*/*"); // essenziell
            request.AddJsonBody(body);
            if (!string.IsNullOrEmpty(file))
            {
                request.AddFile(new FileInfo(file).Name, file);
            }
            RestResponse response = await client.PostAsync(request).ConfigureAwait(false);

            ulong id = (response.StatusCode.Equals(HttpStatusCode.OK) && response.Content != null) ? ulong.Parse(response.Content ?? "0") : 0;

            return id;
        }

        /// <summary>
        /// Outbox des Benutzers ermitteln
        /// </summary>
        /// <returns>Outbox</returns>
        public static async Task<DmsLocation?> GetFolderOutboxAsync()
        {
            // Suchfelder
            EnaioSearchFieldsDTO searchFields = [];
            searchFields.PutValueObject(DMSFIELD_PROJECTNAME, Environment.UserName.ToUpper());
            searchFields.PutValueObject(DMSFIELD_OUTBOX, "1");

            // Payload erstellen
            EnaioDocumentSearchRequestDTO body = new()
            {
                Query = new EnaioDocumentQueryDTO()
                {
                    ObjectTypeId = $"{CommonUtil.GetConfig().UniversalFolderId}",
                    Fields = searchFields
                }
            };

            // Debug: Payload ausgeben
            // string payload = JsonSerializer.Serialize(body, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/search", Method.Post);
            request.AddJsonBody(body);
            List<EnaioDocumentsSearchResponseDTO>? response = await client.PostAsync<List<EnaioDocumentsSearchResponseDTO>>(request).ConfigureAwait(false);
            if (response != null && response.Count > 0)
            {
                // Falls mehrere gefunden dann nur die erste Outbox verwenden als Standort
                DmsLocation outbox = new()
                {
                    Id = response[0].OsId,
                    Client = response[0].EcmSimpleFields?[DMSFIELD_CLIENT]?.Value ?? string.Empty,
                    Type = response[0].EcmSimpleFields?[DMSFIELD_PROJECTTYPE]?.Value ?? string.Empty,
                    Number = response[0].EcmSimpleFields?[DMSFIELD_PROJECTNUMBER]?.Value ?? string.Empty,
                    Name = response[0].EcmSimpleFields?[DMSFIELD_PROJECTNAME]?.Value ?? string.Empty,
                    Outbox = (response[0].EcmSimpleFields?[DMSFIELD_OUTBOX]?.Value ?? string.Empty) == "1",
                    Inbox = (response[0].EcmSimpleFields?[DMSFIELD_INBOX]?.Value ?? string.Empty) == "1"
                };

                return outbox;
            }

            return null;
        }

        /// <summary>
        /// Outbox für Benutzer erstellen
        /// </summary>
        /// <returns>Outbox</returns>
        public static async Task<DmsLocation?> CreateFolderOutboxAsync()
        {
            // Payload erstellen
            DmsDocumentsInsertRequest body = new()
            {
                ObjectTypeId = $"{CommonUtil.GetConfig().UniversalFolderId}",
                MainTypeId = CommonUtil.GetConfig().UniversalFolderId.ToString(),
                Fields = new FieldsSection()
                {
                    Field1 = new InternalNameValue() { InternalName = DMSFIELD_PROJECTNAME, Value = Environment.UserName.ToUpper() },
                    Field2 = new InternalNameValue() { InternalName = DMSFIELD_OUTBOX, Value = "1" },
                    Field3 = new InternalNameValue() { InternalName = DMSFIELD_VISIBILITY, Value = Environment.UserName.ToUpper() + "(u)" }
                }
            };

            // Debug: Payload ausgeben
            // string payload = JsonSerializer.Serialize(body, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new($"api/documents/insert/{CommonUtil.GetConfig().UniversalFolderId}", Method.Post)
            {
                AlwaysMultipartFormData = true
            };
            request.AddHeader("Accept", "*/*"); // essenziell
            request.AddJsonBody(body);
            RestResponse response = await client.PostAsync(request).ConfigureAwait(false);
            if (response.StatusCode.Equals(HttpStatusCode.OK) && response.Content != null)
            {
                DmsLocation outbox = new()
                {
                    Id = ulong.Parse(response.Content ?? "0"),
                    Client = string.Empty,
                    Type = string.Empty,
                    Number = string.Empty,
                    Name = Environment.UserName.ToUpper(),
                    Outbox = true,
                    Inbox = false
                };

                return outbox;
            }

            return null;
        }

        /// <summary>
        /// DMS Dokumente per Id ermitteln
        /// Kann nur 0 oder 1 Ergebnisse liefern
        /// </summary>
        /// <param name="id">Id für die Suche</param>
        /// <param name="result_data">Indexdaten ermitteln</param>
        /// <param name="result_distribution">Verteilhistorie ermitteln</param>
        /// <param name="result_locations">Standorte ermitteln</param>
        /// <returns></returns>
        public static async Task<List<DmsDocument>> GetDocumentsByIdAsync(ulong id, bool result_data = false, bool result_distribution = false, bool result_locations = false)
        {
            List<DmsDocument> documents = [];

            // Abbruch bei ungültiger Id
            if (id <= 0)
            {
                return documents;
            }

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/search", Method.Post);
            request.AddJsonBody(GetSearchPayload(null, id.ToString(), result_data, result_distribution));
            List<EnaioDocumentsSearchResponseDTO>? response = await client.PostAsync<List<EnaioDocumentsSearchResponseDTO>>(request).ConfigureAwait(false);
            if (response != null && response.Count > 0)
            {
                documents.AddRange(await GetDocumentsFromResponse(response, result_locations));
            }

            return documents;
        }

        /// <summary>
        /// DMS Dokumente per GUID ermitteln
        /// Sollte nur 0 oder 1 Ergebnisse liefern, kann je nach Datenlage aber auch mehrere zur Folge haben
        /// </summary>
        /// <param name="guid">GUID für die Suche</param>
        /// <param name="result_data">Indexdaten ermitteln</param>
        /// <param name="result_distribution">Verteilhistorie ermitteln</param>
        /// <param name="result_locations">Standorte ermitteln</param>
        /// <returns></returns>
        public static async Task<List<DmsDocument>> GetDocumentsByGuidAsync(string? guid, bool result_data = false, bool result_distribution = false, bool result_locations = false)
        {
            List<DmsDocument> dmsDocuments = [];

            // Abbruch bei ungültiger GUID
            if (string.IsNullOrEmpty(guid) || !SentGuidRegex().IsMatch(guid))
            {
                return dmsDocuments;
            }

            // Suchfelder
            EnaioSearchFieldsDTO searchFields = [];
            searchFields.PutValueObject(DMSFIELD_SENTGUID, guid);

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/search", Method.Post);
            request.AddJsonBody(GetSearchPayload(searchFields, null, result_data, result_distribution));
            List<EnaioDocumentsSearchResponseDTO>? response = await client.PostAsync<List<EnaioDocumentsSearchResponseDTO>>(request).ConfigureAwait(false);
            if (response != null && response.Count > 0)
            {
                dmsDocuments.AddRange(await GetDocumentsFromResponse(response, result_locations));
            }

            return dmsDocuments;
        }

        /// <summary>
        /// DMS Dokumente per Indexdaten ermitteln
        /// Sollte nur 0 oder 1 Ergebnisse liefern, kann je nach Datenlage aber auch mehrere zur Folge haben
        /// </summary>
        /// <param name="data">Indexdaten für die Suche</param>
        /// <param name="exacttime">Zeitangabe einbeziehen</param>
        /// <param name="result_data">Indexdaten ermitteln</param>
        /// <param name="result_distribution">Verteilhistorie ermitteln</param>
        /// <param name="result_locations">Standorte ermitteln</param>
        /// <returns></returns>
        public static async Task<List<DmsDocument>> GetDocumentsByIndexdataAsync(GwDocument gwDocument, bool exacttime, bool result_data = false, bool result_distribution = false, bool result_locations = false)
        {
            List<DmsDocument> dmsDocuments = [];

            // Abbruch bei ungültigen Indexdaten
            if (gwDocument == null || string.IsNullOrEmpty(gwDocument.CreationDate) || (exacttime == true && string.IsNullOrEmpty(gwDocument.CreationTime)) || string.IsNullOrEmpty(gwDocument.MailAddressFrom))
            {
                return dmsDocuments;
            }

            // Suchfelder
            // Info: Datum und Uhrzeit würden der Erfassungszeit im DMS entsprechen und weichen von der Zeit in der E-Mail teilweise ab.
            // Absender, Empfänger und Kopieempfänger sind textuell abweichend formatiert gegenüber dem DMS und werden einzeln überprüft.
            // Die Suche ist somit ungenau, kann sowohl mehrere als auch keinen Treffer liefern, reicht aber für die meisten Fälle aus.
            EnaioSearchFieldsDTO searchFields = [];
            searchFields.PutValueObject(DMSFIELD_MAILDATE, gwDocument.CreationDate);
            if (exacttime)
            {
                searchFields.PutValueObject(DMSFIELD_MAILTIME, gwDocument.CreationTime);
            }

            // Betreffsuche: Whitespaces zusammenführen und durch Sterne ersetzen, Anfang und Ende ausschließen, Backslash entfernen
            string subject = gwDocument.Subject.Replace(@"\", "");
            subject = SubjectSearchRegex().Replace(subject, "*");
            while (subject.Contains("**"))
            {
                subject = subject.Replace("**", "*");
            }
            searchFields.PutValueObject(DMSFIELD_MAILSUBJECT, subject);

            // Absender und Empfänger
            searchFields.PutValueObject(DMSFIELD_MAILFROM, $"*{gwDocument.MailAddressFrom}*");
            foreach (string email in gwDocument.MailAddressesTo)
            {
                searchFields.PutValueObject(DMSFIELD_MAILTO, $"*{email}*");
            }
            foreach (string email in gwDocument.MailAddressesCc)
            {
                searchFields.PutValueObject(DMSFIELD_MAILCC, $"*{email}*");
            }

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/search", Method.Post);
            request.AddJsonBody(GetSearchPayload(searchFields, null, result_data, result_distribution));
            List<EnaioDocumentsSearchResponseDTO>? response = await client.PostAsync<List<EnaioDocumentsSearchResponseDTO>>(request).ConfigureAwait(false);
            if (response != null && response.Count > 0)
            {
                dmsDocuments.AddRange(await GetDocumentsFromResponse(response, result_locations));
            }

            return dmsDocuments;
        }

        /// <summary>
        /// Zusätzliche Suchfelder: In einem Ordner innerhalb SEELE Gersthofen aber nicht im Dublettenverzeichnis
        /// </summary>
        private static EnaioAdditionalQueriesDTO GetAdditionalQuery()
        {
            EnaioSearchFieldsDTO additionalFields = [];
            additionalFields.PutValueObject(DMSFIELD_PROJECTNAME, DMS_DUPLICATENAME, true);

            EnaioAdditionalQueriesDTO additionalQuery =
            [
                new EnaioDocumentQueryDTO() { ObjectTypeId = $"{CommonUtil.GetConfig().UniversalFolderId}", Fields = additionalFields }
            ];

            return additionalQuery;
        }

        /// <summary>
        /// Ergebnisfelder in Abhängigkeit von den Parametern erstellen
        /// </summary>
        /// <param name="result_data">Indexdaten ermitteln</param>
        /// <param name="result_distribution">Verteilhistorie ermitteln</param>
        /// <returns></returns>
        private static EnaioFieldsSchemaListDTO GetResultFields(bool result_data, bool result_distribution)
        {

            EnaioFieldsSchemaListDTO resultFields = [];
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_SENTGUID));
            if (result_data)
            {
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_MAILSUBJECT));
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_MAILFROM));
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_MAILTO));
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_MAILCC));
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_MAILDATE));
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_MAILTIME));
            }
            if (result_distribution)
            {
                resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_DISTRIBUTION));
            }

            return resultFields;
        }

        /// <summary>
        /// JSON-Payload für Documents-Search Request erstellen
        /// </summary>
        /// <param name="searchFields">Felder und Inhalt für die Suche</param>
        /// <param name="result_data">Indexdaten ermitteln</param>
        /// <param name="result_distribution">Verteilhistorie ermitteln</param>
        /// <returns></returns>
        private static EnaioDocumentSearchRequestDTO GetSearchPayload(EnaioSearchFieldsDTO? searchFields, string? osId, bool result_data, bool result_distribution)
        {
            // Payload erstellen
            EnaioDocumentSearchRequestDTO body = new()
            {
                Query = new EnaioDocumentQueryDTO()
                {
                    ObjectTypeId = CommonUtil.GetConfig().UniversalDocumentId.ToString(),
                    OsId = osId,
                    Fields = searchFields,
                    ResultConfig = new EnaioResultConfigDTO()
                    {
                        Fieldsschema = GetResultFields(result_data, result_distribution),
                        FieldsSchemaMode = "DEF",
                        SystemFields = 0,
                        BaseParameters = 0,
                        FileProperties = 0,
                        Rights = 0,
                        MaxHits = 0,
                        Offset = 0
                    }
                },
                AdditionalQueries = GetAdditionalQuery()
            };

            // Debug: Payload ausgeben
            // string payload = JsonSerializer.Serialize(body, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

            return body;
        }

        /// <summary>
        /// Ergebnisobjekt der AppConnector-Suche in DmsDocument-Objekte umwandeln
        /// </summary>
        /// <param name="response">Ergebnisobjekt der AppConnector Suche</param>
        /// <param name="result_locations">Standorte ermitteln</param>
        /// <returns></returns>
        private static async Task<List<DmsDocument>> GetDocumentsFromResponse(List<EnaioDocumentsSearchResponseDTO> response, bool result_locations)
        {
            List<DmsDocument> documents = [];

            foreach (EnaioDocumentsSearchResponseDTO item in response)
            {
                // Ergebnisobjekt erstellen
                DmsDocument document = new()
                {
                    Id = item.OsId,
                    SentGuid = item.EcmSimpleFields?[DMSFIELD_SENTGUID]?.Value,
                    CreationDate = item.EcmSimpleFields?[DMSFIELD_MAILDATE]?.Value ?? string.Empty,
                    CreationTime = item.EcmSimpleFields?[DMSFIELD_MAILTIME]?.Value ?? string.Empty,
                    Subject = item.EcmSimpleFields?[DMSFIELD_MAILSUBJECT]?.Value ?? string.Empty,
                    MailFrom = item.EcmSimpleFields?[DMSFIELD_MAILFROM]?.Value ?? string.Empty,
                    MailTo = item.EcmSimpleFields?[DMSFIELD_MAILTO]?.Value ?? string.Empty,
                    MailCc = item.EcmSimpleFields?[DMSFIELD_MAILCC]?.Value ?? string.Empty
                };

                // Verteilhistorie ermitteln
                foreach (var item1 in item.EcmTableFields ?? [])
                {
                    foreach (var item2 in item1.Value ?? [])
                    {
                        foreach (var item3 in item2.EcmSimpleFields ?? [])
                        {
                            if (item3.InternalName.Equals("feld1"))
                            {
                                if (!document.Distribution.Contains(item3.Value))
                                {
                                    document.Distribution.Add(item3.Value);
                                }
                            }
                        }
                    }
                }

                // Standorte ermitteln falls erforderlich
                if (result_locations)
                {
                    document.Locations = await GetLocationsByIdAsync(document.Id);
                }

                // Dokument in Rückgabeliste einfügen
                documents.Add(document);
            }

            return documents;
        }

        /// <summary>
        /// Alle Standorte eines Dokuments ermitteln
        /// </summary>
        /// <param name="id">Dokument-Id</param>
        /// <returns>Liste der Standorte</returns>
        public static async Task<List<DmsLocation>> GetLocationsByIdAsync(ulong id)
        {
            List<DmsLocation> locations = [];

            // Abbruch bei ungültiger Id
            if (id <= 0)
            {
                return locations;
            }

            // zusätzliche Suchfelder
            EnaioAdditionalQueriesDTO additionalQuery =
            [
                new EnaioDocumentQueryDTO() { ObjectTypeId = $"{CommonUtil.GetConfig().UniversalDocumentId}", OsId = $"{id}" }
            ];

            // Ergebnisfelder
            EnaioFieldsSchemaListDTO resultFields = [];
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_CLIENT));
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_PROJECTTYPE));
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_PROJECTNUMBER));
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_PROJECTNAME));
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_CLIENT));
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_OUTBOX));
            resultFields.AddOrMerge(new EnaioFieldsSchemaDTO(DMSFIELD_INBOX));

            // Payload erstellen
            EnaioDocumentSearchRequestDTO body = new()
            {
                Query = new EnaioDocumentQueryDTO()
                {
                    ObjectTypeId = CommonUtil.GetConfig().UniversalFolderId.ToString(),
                    ResultConfig = new EnaioResultConfigDTO()
                    {
                        Fieldsschema = resultFields,
                        FieldsSchemaMode = "DEF",
                        SystemFields = 0,
                        BaseParameters = 0,
                        FileProperties = 0,
                        Rights = 0,
                        Offset = 0
                    }
                },
                AdditionalQueries = additionalQuery
            };

            // Debug: Payload ausgeben
            // string payload = JsonSerializer.Serialize(body, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

            // Request und Rückgabe
            RestClient client = GetRestClient();
            RestRequest request = new("api/documents/search", Method.Post);
            request.AddJsonBody(body);
            List<EnaioDocumentsSearchResponseDTO>? response = await client.PostAsync<List<EnaioDocumentsSearchResponseDTO>>(request).ConfigureAwait(false);
            if (response != null && response.Count > 0)
            {
                foreach (EnaioDocumentsSearchResponseDTO match in response)
                {
                    // Standortobjekt erzeugen
                    DmsLocation location = new()
                    {
                        Id = match.OsId
                    };

                    // Feldinhalte aus Suchergebnis übernehmen
                    EnaioSimpleFieldCollectionDTO? simpleFields = match.EcmSimpleFields;
                    if (simpleFields != null)
                    {
                        location.Client = simpleFields.Find(item => item.InternalName == DMSFIELD_CLIENT).Value ?? string.Empty;
                        location.Type = simpleFields.Find(item => item.InternalName == DMSFIELD_PROJECTTYPE).Value ?? string.Empty;
                        location.Number = simpleFields.Find(item => item.InternalName == DMSFIELD_PROJECTNUMBER).Value ?? string.Empty;
                        location.Name = simpleFields.Find(item => item.InternalName == DMSFIELD_PROJECTNAME).Value ?? string.Empty;
                        location.Outbox = (simpleFields.Find(item => item.InternalName == DMSFIELD_OUTBOX).Value ?? string.Empty) == "1";
                        location.Inbox = (simpleFields.Find(item => item.InternalName == DMSFIELD_INBOX).Value ?? string.Empty) == "1";
                    }

                    locations.Add(location);
                }
            }

            return locations;
        }
    }
}