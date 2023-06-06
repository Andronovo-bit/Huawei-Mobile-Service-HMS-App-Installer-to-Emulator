using Newtonsoft.Json;

namespace HuaweiHMSInstaller.Models.MappingModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Authority
    {
        public List<object> list { get; set; }
        public string name { get; set; }
    }

    public class CommentList
    {
        public string avatar { get; set; }
        public string commentInfo { get; set; }
        public string commentTime { get; set; }
        public string nickName { get; set; }
        public string stars { get; set; }
    }

    public class ComponentDataDetail
    {
        public int animation { get; set; }
        public int customDisplayField { get; set; }
        public int customDisplayField1 { get; set; }
        public int isShowInstallBtn { get; set; }
        public string wordDsId { get; set; }
    }

    public class Conceal
    {
        public string name { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class DataListDetail
    {
        public string tariffDesc { get; set; }
        public int followState { get; set; }
        public List<LabelName> labelNames { get; set; }
        public List<object> safeLabels { get; set; }
        public string icoUri { get; set; }
        public string starDesc { get; set; }
        public int jointOperation { get; set; }
        public int nonAdaptType { get; set; }
        public ExIcons exIcons { get; set; }
        public object stars { get; set; }
        public int sectionId { get; set; }
        public List<string> labels { get; set; }
        public string gradeIcon { get; set; }
        public int ctype { get; set; }
        public int appType { get; set; }
        public string intro { get; set; }
        public int supportWatch { get; set; }
        public string gradeDesc { get; set; }
        public int minAge { get; set; }
        public string name { get; set; }
        public int gradeCount { get; set; }
        public int hiddenStars { get; set; }
        public HmsSdkVersionDetail hmsSdkVersion { get; set; }
        public int? isFavoriteApp { get; set; }
        public string sha256 { get; set; }
        public string detailId { get; set; }
        public int? targetSDK { get; set; }
        public int? obbSize { get; set; }
        public string versionName { get; set; }
        public int? shareType { get; set; }
        public int? detailType { get; set; }
        public int? gmsSupportFlag { get; set; }
        public string price { get; set; }
        public string gmsUrl { get; set; }
        public int? isGradeAdapt { get; set; }
        public string package { get; set; }
        public object productId { get; set; }
        public int? freeDays { get; set; }
        public object trackId { get; set; }
        public string shareUri { get; set; }
        public int? commentCount { get; set; }
        public int? commentStatus { get; set; }
        public int? showDisclaimer { get; set; }
        public int? size { get; set; }
        public int? bundleSize { get; set; }
        public object originalPrice { get; set; }
        public string sizeDesc { get; set; }
        public string icon { get; set; }
        public int? shellApkVer { get; set; }
        public List<object> dependentedApps { get; set; }
        public string editorDescribe { get; set; }
        public string shareContent { get; set; }
        public string relatedDetailId { get; set; }
        public object freeDesc { get; set; }
        public object riskTips { get; set; }
        public int? btnDisable { get; set; }
        public int? fullSize { get; set; }
        public object localOriginalPrice { get; set; }
        public int? buttonTextType { get; set; }
        public int? isExt { get; set; }
        public string appoid { get; set; }
        public string versionCode { get; set; }
        public string originalUri { get; set; }
        public int? packingType { get; set; }
        public List<object> relatedApps { get; set; }
        public int? installConfig { get; set; }
        public string portalUrl { get; set; }
        public string appid { get; set; }
        public object maple { get; set; }
        public object localPrice { get; set; }
        public string md5 { get; set; }
        public List<string> images { get; set; }
        public List<VideoList> videoList { get; set; }
        public List<string> imageCompress { get; set; }
        public string imageTag { get; set; }
        public string title { get; set; }
        public string releaseDate { get; set; }
        public string version { get; set; }
        public string devVipType { get; set; }
        public string developer { get; set; }
        public int? style { get; set; }
        public string body { get; set; }
        public object verticalimg { get; set; }
        public object horizonalimg { get; set; }
        public int? maxRows { get; set; }
        public string subTitle { get; set; }
        public int? hasNextPage { get; set; }
        public object fontcolor { get; set; }
        public List<List> list { get; set; }
        public string appIntro { get; set; }
        public Conceal conceal { get; set; }
        public string appTariff { get; set; }
        public string webSite { get; set; }
        public Authority authority { get; set; }
        public string webSiteTile { get; set; }
        public string logUri { get; set; }
        public GradeInfo gradeInfo { get; set; }
        public int? scoredBy { get; set; }
        public string score { get; set; }
        public string downloads { get; set; }
        public string commentDetailId { get; set; }
        public string downloadUnit { get; set; }
        public int? hasAward { get; set; }
    }

    public class GradeInfo
    {
        public string gradeContentDesc { get; set; }
        public string gradeDesc { get; set; }
        public string gradeIcon { get; set; }
        public string gradeInteractiveDesc { get; set; }
    }

    public class GradeList
    {
        public int rating { get; set; }
        public int ratingCounts { get; set; }
    }

    public class HmsSdkVersionDetail
    {
        public string accountSdkVersion { get; set; }
        public string hmsSdkVersion { get; set; }
        public string iapSdkVersion { get; set; }
    }

    public class LabelName
    {
        public string name { get; set; }
        public int type { get; set; }
    }

    public class LayoutDetail
    {
        public int maxRows { get; set; }
        public int layoutId { get; set; }
        public string layoutName { get; set; }
    }

    public class LayoutDetailDetail
    {
        public int swipeDownRefresh { get; set; }
        public int closable { get; set; }
        public int isInstalledFilter { get; set; }
        public ComponentDataDetail componentData { get; set; }
        public object negativeFeedback { get; set; }
        public int layoutId { get; set; }
        public string layoutName { get; set; }
        public string listId { get; set; }

        [JsonProperty("dataList-type")]
        public int dataListtype { get; set; }
        public int uninstalledFilter { get; set; }
        public List<DataListDetail> dataList { get; set; }
        public int isUpdatableFilter { get; set; }
        public string maxDisplayTime { get; set; }
        public string name { get; set; }
        public string detailId { get; set; }
    }

    public class List
    {
        public List<CommentList> commentList { get; set; }
        public int rated { get; set; }
        public string score { get; set; }
        public List<GradeList> gradeList { get; set; }
        public string logSource { get; set; }
        public string stars { get; set; }
        public string sha256 { get; set; }
        public string ZONE { get; set; }
        public string detailId { get; set; }
        public string memo { get; set; }
        public int? targetSDK { get; set; }
        public string listId { get; set; }
        public int? jumpToGpOnGMSDevice { get; set; }
        public int? gmsSupportFlag { get; set; }
        public int? detailType { get; set; }
        public int? styleType { get; set; }
        public string needInstallFilter { get; set; }
        public object downurl { get; set; }
        public string price { get; set; }
        public string intro { get; set; }
        public object gmsUrl { get; set; }
        public int? isGradeAdapt { get; set; }
        public string ID { get; set; }
        public int? state { get; set; }
        public string describeType { get; set; }
        public string aliasName { get; set; }
        public string package { get; set; }
        public int? freeDays { get; set; }
        public int? showAdTag { get; set; }
        public int? alphaTestTimestamp { get; set; }
        public string appVersionName { get; set; }
        public string downCountDesc { get; set; }
        public int? showDisclaimer { get; set; }
        public int? ctype { get; set; }
        public int? size { get; set; }
        public int? bundleSize { get; set; }
        public string COMNUM { get; set; }
        public int? minAge { get; set; }
        public string name { get; set; }
        public int? displayField { get; set; }
        public int? pinned { get; set; }
        public int? isIconRectangle { get; set; }
        public object originalPrice { get; set; }
        public int? submitType { get; set; }
        public int? detailStyle { get; set; }
        public string sizeDesc { get; set; }
        public string icon { get; set; }
        public int? customDisplayField { get; set; }
        public int? prizeState { get; set; }
        public int? isStandalone { get; set; }
        public int? customDisplay { get; set; }
        public string kindName { get; set; }
        public object freeDesc { get; set; }
        public int? btnDisable { get; set; }
        public int? webApp { get; set; }
        public int? fullSize { get; set; }
        public object localOriginalPrice { get; set; }
        public int? nonAdaptType { get; set; }
        public ExIcons exIcons { get; set; }
        public int? videoFlag { get; set; }
        public string tagName { get; set; }
        public string versionCode { get; set; }
        public int? orderVersionCode { get; set; }
        public int? packingType { get; set; }
        public int? genShortcutForWebApp { get; set; }
        public List<object> relatedApps { get; set; }
        public int? installConfig { get; set; }
        public string appid { get; set; }
        public int? maple { get; set; }
        public PrivilegedRightDetail privilegedRight { get; set; }
        public object gplinkPkgName { get; set; }
        public object localPrice { get; set; }
        public string md5 { get; set; }
    }

    public class PrivilegedRightDetail
    {
        public string deferredDeeplink { get; set; }
        public int installExp { get; set; }
        public int promotionFlag { get; set; }
    }

    public class AppGalleryAppDetailResult
    {
        public string statKey { get; set; }
        public string titleType { get; set; }
        public List<LayoutDetailDetail> layoutData { get; set; }
        public int layoutFrom { get; set; }
        public List<object> sortInfo { get; set; }
        public string returnTabId { get; set; }
        public int hasNextPage { get; set; }
        public int count { get; set; }
        public string engineerVersion { get; set; }
        public List<LayoutDetail> layout { get; set; }
        public int isSupSearch { get; set; }
        public string name { get; set; }
        public int totalPages { get; set; }
        public int titleIconType { get; set; }
        public int contentType { get; set; }
        public int marginTop { get; set; }
        public int rtnCode { get; set; }
        public string rspKey { get; set; }
    }

    public class VideoList
    {
        public string duration { get; set; }
        public string horizontalVideoPoster { get; set; }
        public string videoWideAndHeight { get; set; }
        public string videoUrl { get; set; }
        public string logSource { get; set; }
        public string videoFrom { get; set; }
        public string videoId { get; set; }
        public string logId { get; set; }
        public string sp { get; set; }
        public string videoPoster { get; set; }
        public int videoIndex { get; set; }
        public int videoTag { get; set; }
    }


}
