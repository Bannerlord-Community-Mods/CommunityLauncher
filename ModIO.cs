using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommunityLauncher.ModIO
{
    public partial class ModList
    {
        [JsonProperty("data")] public List<Mod> Data { get; set; }
        [JsonProperty("result_count")] public long ResultCount { get; set; }
        [JsonProperty("result_offset")] public long ResultOffset { get; set; }
        [JsonProperty("result_limit")] public long ResultLimit { get; set; }
        [JsonProperty("result_total")] public long ResultTotal { get; set; }
    }

    public partial class Mod
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("game_id")] public long GameId { get; set; }
        [JsonProperty("status")] public long Status { get; set; }
        [JsonProperty("visible")] public long Visible { get; set; }
        [JsonProperty("submitted_by")] public SubmittedBy SubmittedBy { get; set; }
        [JsonProperty("date_added")] public long DateAdded { get; set; }
        [JsonProperty("date_updated")] public long DateUpdated { get; set; }
        [JsonProperty("date_live")] public long DateLive { get; set; }
        [JsonProperty("maturity_option")] public long MaturityOption { get; set; }
        [JsonProperty("logo")] public Logo Logo { get; set; }
        [JsonProperty("homepage_url")] public Uri HomepageUrl { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("name_id")] public string NameId { get; set; }
        [JsonProperty("summary")] public string Summary { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("description_plaintext")] public string DescriptionPlaintext { get; set; }
        [JsonProperty("metadata_blob")] public string MetadataBlob { get; set; }
        [JsonProperty("profile_url")] public Uri ProfileUrl { get; set; }
        [JsonProperty("media")] public Media Media { get; set; }
        [JsonProperty("modfile")] public Modfile Modfile { get; set; }
        [JsonProperty("metadata_kvp")] public List<MetadataKvp> MetadataKvp { get; set; }
        [JsonProperty("tags")] public List<Tag> Tags { get; set; }
        [JsonProperty("stats")] public Stats Stats { get; set; }
    }

    public partial class Logo
    {
        [JsonProperty("filename")] public string Filename { get; set; }
        [JsonProperty("original")] public Uri Original { get; set; }
        [JsonProperty("thumb_320x180")] public Uri Thumb320X180 { get; set; }
        [JsonProperty("thumb_640x360")] public Uri Thumb640X360 { get; set; }
        [JsonProperty("thumb_1280x720")] public Uri Thumb1280X720 { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("youtube")] public List<Uri> Youtube { get; set; }
        [JsonProperty("sketchfab")] public List<Uri> Sketchfab { get; set; }
        [JsonProperty("images")] public List<Image> Images { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("filename")] public string Filename { get; set; }
        [JsonProperty("original")] public Uri Original { get; set; }
        [JsonProperty("thumb_320x180")] public Uri Thumb320X180 { get; set; }
    }

    public partial class MetadataKvp
    {
        [JsonProperty("metakey")] public string Metakey { get; set; }
        [JsonProperty("metavalue")] [JsonConverter(typeof(ParseStringConverter))] public long Metavalue { get; set; }
    }

    public partial class Modfile
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("mod_id")] public long ModId { get; set; }
        [JsonProperty("date_added")] public long DateAdded { get; set; }
        [JsonProperty("date_scanned")] public long DateScanned { get; set; }
        [JsonProperty("virus_status")] public long VirusStatus { get; set; }
        [JsonProperty("virus_positive")] public long VirusPositive { get; set; }
        [JsonProperty("virustotal_hash")] public string VirustotalHash { get; set; }
        [JsonProperty("filesize")] public long Filesize { get; set; }
        [JsonProperty("filehash")] public Filehash Filehash { get; set; }
        [JsonProperty("filename")] public string Filename { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("changelog")] public string Changelog { get; set; }
        [JsonProperty("metadata_blob")] public string MetadataBlob { get; set; }
        [JsonProperty("download")] public Download Download { get; set; }
    }

    public partial class Download
    {
        [JsonProperty("binary_url")] public Uri BinaryUrl { get; set; }
        [JsonProperty("date_expires")] public long DateExpires { get; set; }
    }

    public partial class Filehash
    {
        [JsonProperty("md5")] public string Md5 { get; set; }
    }

    public partial class Stats
    {
        [JsonProperty("mod_id")] public long ModId { get; set; }
        [JsonProperty("popularity_rank_position")] public long PopularityRankPosition { get; set; }
        [JsonProperty("popularity_rank_total_mods")] public long PopularityRankTotalMods { get; set; }
        [JsonProperty("downloads_total")] public long DownloadsTotal { get; set; }
        [JsonProperty("subscribers_total")] public long SubscribersTotal { get; set; }
        [JsonProperty("ratings_total")] public long RatingsTotal { get; set; }
        [JsonProperty("ratings_positive")] public long RatingsPositive { get; set; }
        [JsonProperty("ratings_negative")] public long RatingsNegative { get; set; }
        [JsonProperty("ratings_percentage_positive")] public long RatingsPercentagePositive { get; set; }
        [JsonProperty("ratings_weighted_aggregate")] public double RatingsWeightedAggregate { get; set; }
        [JsonProperty("ratings_display_text")] public string RatingsDisplayText { get; set; }
        [JsonProperty("date_expires")] public long DateExpires { get; set; }
    }

    public partial class SubmittedBy
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("name_id")] public string NameId { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("date_online")] public long DateOnline { get; set; }
        [JsonProperty("avatar")] public Avatar Avatar { get; set; }
        [JsonProperty("timezone")] public string Timezone { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("profile_url")] public Uri ProfileUrl { get; set; }
    }

    public partial class Avatar
    {
        [JsonProperty("filename")] public string Filename { get; set; }
        [JsonProperty("original")] public Uri Original { get; set; }
        [JsonProperty("thumb_50x50")] public Uri Thumb50X50 { get; set; }
        [JsonProperty("thumb_100x100")] public Uri Thumb100X100 { get; set; }
    }

    public partial class Tag
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("date_added")] public long DateAdded { get; set; }
    }

    public partial class ModList
    {
        public static ModList FromJson(string json) => JsonConvert.DeserializeObject<ModList>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ModList self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}