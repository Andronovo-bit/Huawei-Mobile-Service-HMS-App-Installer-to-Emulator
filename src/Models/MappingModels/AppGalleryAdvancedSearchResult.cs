using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Models.MappingModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ComponentData
    {
        public int animation { get; set; }
        public int customDisplayField { get; set; }
        public int customDisplayField1 { get; set; }
        public int isShowInstallBtn { get; set; }
        public string wordDsId { get; set; }
    }

    public class DataList
    {
        public string sha256 { get; set; }
        public string ZONE { get; set; }
        public string detailId { get; set; }
        public string memo { get; set; }
        public int targetSDK { get; set; }
        public string listId { get; set; }
        public int jumpToGpOnGMSDevice { get; set; }
        public int gmsSupportFlag { get; set; }
        public int detailType { get; set; }
        public string score { get; set; }
        public int styleType { get; set; }
        public string needInstallFilter { get; set; }
        public string downurl { get; set; }
        public string price { get; set; }
        public string intro { get; set; }
        public string gmsUrl { get; set; }
        public int isGradeAdapt { get; set; }
        public string ID { get; set; }
        public int state { get; set; }
        public HmsSdkVersion hmsSdkVersion { get; set; }
        public string describeType { get; set; }
        public string package { get; set; }
        public int freeDays { get; set; }
        public int showAdTag { get; set; }
        public int alphaTestTimestamp { get; set; }
        public string appVersionName { get; set; }
        public string downCountDesc { get; set; }
        public int showDisclaimer { get; set; }
        public int ctype { get; set; }
        public string size { get; set; }
        public string bundleSize { get; set; }
        public string COMNUM { get; set; }
        public int minAge { get; set; }
        public string name { get; set; }
        public int displayField { get; set; }
        public int pinned { get; set; }
        public int isIconRectangle { get; set; }
        public object originalPrice { get; set; }
        public int submitType { get; set; }
        public int detailStyle { get; set; }
        public string sizeDesc { get; set; }
        public string icon { get; set; }
        public int customDisplayField { get; set; }
        public int prizeState { get; set; }
        public int isStandalone { get; set; }
        public int customDisplay { get; set; }
        public string kindName { get; set; }
        public object freeDesc { get; set; }
        public int btnDisable { get; set; }
        public int webApp { get; set; }
        public string fullSize { get; set; }
        public int jointOperation { get; set; }
        public object localOriginalPrice { get; set; }
        public string logSource { get; set; }
        public int nonAdaptType { get; set; }
        public ExIcons exIcons { get; set; }
        public string stars { get; set; }
        public int videoFlag { get; set; }
        public string tagName { get; set; }
        public string versionCode { get; set; }
        public int orderVersionCode { get; set; }
        public int packingType { get; set; }
        public int genShortcutForWebApp { get; set; }
        public List<object> relatedApps { get; set; }
        public int installConfig { get; set; }
        public string appid { get; set; }
        public int maple { get; set; }
        public PrivilegedRight privilegedRight { get; set; }
        public object gplinkPkgName { get; set; }
        public object localPrice { get; set; }
        public string md5 { get; set; }
        public string downloadRecommendUri { get; set; }
        public string wish4notfoundcard { get; set; }
        public string desc { get; set; }
        public string nonAdaptIcon { get; set; }
    }

    public class EnhanceIcon
    {
        public string enhanceClickedIcon { get; set; }
        public string enhanceIcon { get; set; }
        public int iconSizeType { get; set; }
        public int showTimeBegin { get; set; }
        public int showTimeEnd { get; set; }
        public int type { get; set; }
    }

    public class ExIcons
    {
    }

    public class HmsSdkVersion
    {
        public string accountSdkVersion { get; set; }
        public string hmsSdkVersion { get; set; }
        public string iapSdkVersion { get; set; }
    }

    public class Layout
    {
        public int maxRows { get; set; }
        public int layoutId { get; set; }
        public string layoutName { get; set; }
    }

    public class LayoutDatum
    {
        public int swipeDownRefresh { get; set; }
        public int closable { get; set; }
        public int isInstalledFilter { get; set; }
        public ComponentData componentData { get; set; }
        public int? negativeFeedback { get; set; }
        public int layoutId { get; set; }
        public string layoutName { get; set; }
        public string listId { get; set; }

        [JsonProperty("dataList-type")]
        public int dataListtype { get; set; }
        public int uninstalledFilter { get; set; }
        public List<DataList> dataList { get; set; }
        public int isUpdatableFilter { get; set; }
        public string name { get; set; }
        public string maxDisplayTime { get; set; }
    }

    public class PrivilegedRight
    {
        public string deferredDeeplink { get; set; }
        public int installExp { get; set; }
        public int promotionFlag { get; set; }
    }

    public class AppGalleryAdvancedSearchResult
    {
        public string statKey { get; set; }
        public object titleType { get; set; }
        public List<LayoutDatum> layoutData { get; set; }
        public int layoutFrom { get; set; }
        public List<TabInfo> tabInfo { get; set; }
        public List<object> sortInfo { get; set; }
        public string returnTabId { get; set; }
        public int hasNextPage { get; set; }
        public int count { get; set; }
        public string engineerVersion { get; set; }
        public object categoryName { get; set; }
        public List<Layout> layout { get; set; }
        public int isSupSearch { get; set; }
        public int supportResort { get; set; }
        public List<object> defaultTabInfo { get; set; }
        public string name { get; set; }
        public int totalPages { get; set; }
        public int titleIconType { get; set; }
        public int contentType { get; set; }
        public int marginTop { get; set; }
        public int rtnCode { get; set; }
        public string rspKey { get; set; }
    }

    public class TabInfo
    {
        public string tabId { get; set; }
        public string statKey { get; set; }
        public string tabEnName { get; set; }
        public string tabName { get; set; }
        public string returnTabId { get; set; }
        public int swipeDownRefresh { get; set; }
        public string engineerVersion { get; set; }
        public int actionBarStyle { get; set; }
        public EnhanceIcon enhanceIcon { get; set; }
        public string realTabId { get; set; }
        public int hasChild { get; set; }
        public string funFlag { get; set; }
        public int style { get; set; }
        public int isSupShake { get; set; }
        public int titleIconType { get; set; }
        public object popWindow { get; set; }
        public string contentType { get; set; }
        public int marginTop { get; set; }
        public int fixedSort { get; set; }
    }


}
