﻿#region
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotLiquid;
using DotLiquid.Util;
using VirtoCommerce.Web.Views.Engines.Liquid;
using VirtoCommerce.Web.Views.Engines.Liquid.Extensions;
using VirtoCommerce.Web.Views.Engines.Liquid.ViewEngine.Extensions;
using System.Threading;
using Tag = VirtoCommerce.Web.Models.Tagging.Tag;

#endregion

namespace VirtoCommerce.Web.Models.Filters
{
    public class ModelFilters
    {
        private static readonly Regex TagSyntax = R.B(R.Q(@"([A-Za-z0-9]+)_([A-Za-z0-9].+)"));
        private static readonly Regex WordReplaceSyntax = R.B(R.Q(@"[^\w\s-]"));
        private static readonly Regex WordReplaceSyntax2 = R.B(R.Q(@"[\s-]+"));
        //private static readonly Regex WordReplaceSyntax = R.B(R.Q(@"[^a-z0-9]+/i"));

        private static readonly Lazy<CultureInfo[]> _cultures = new Lazy<CultureInfo[]>(
            CreateCultures,
            LazyThreadSafetyMode.ExecutionAndPublication);

        private static CultureInfo[] CreateCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        }

        public static Language Culture(string input)
        {
            var culture = CultureInfo.CreateSpecificCulture(input);
            var lang = new Language { Name = culture.Name, NativeName = culture.NativeName, DisplayName = culture.DisplayName, EnglishName = culture.EnglishName };
            return lang;
        }

        public static string EditCustomerAddressLink(string input, string id)
        {
            return string.Format(
                "<a href=\"#\" onclick=\"Shopify.CustomerAddress.toggleForm({0});return false\">{1}</a>", id, input);
        }

        public static string DeleteCustomerAddressLink(string input, string id)
        {
            return string.Format(
                "<a href=\"#\" onclick=\"Shopify.CustomerAddress.destroy({0}, 'Are you sure you wish to delete this address?');return false\">{1}</a>", id, input);
        }

        public static string CustomerLoginLink(string input)
        {
            var path = VirtualPathUtility.ToAbsolute("~/account/login");

            return String.Format("<a href=\"{0}\" id=\"customer_login_link\">{1}</a>", path, input);
        }

        /* sasha: this doesn't work in templates when integers are used for minus
        public static decimal Minus(decimal input, decimal parameter)
        {
            return input - parameter;
        }
        */

        public static string CustomerLogoutLink(string input)
        {
            var path = VirtualPathUtility.ToAbsolute("~/account/logoff");

            return String.Format("<a href=\"{0}\" id=\"customer_logout_link\">{1}</a>", path, input);
        }

        public static string CustomerRegisterLink(string input)
        {
            var path = VirtualPathUtility.ToAbsolute("~/account/register");

            return String.Format("<a href=\"{0}\" id=\"customer_register_link\">{1}</a>", path, input);
        }

        public static string Money(object input)
        {
            if (input == null)
            {
                return null;
            }

            decimal val;

            if (input is int)
            {
                var inputString = ((int)input).ToString("D3");
                var parsedDecimal = String.Format(
                    "{0}{1}{2}",
                    inputString.Substring(0, inputString.Length - 2),
                    CultureInfo.GetCultureInfo("en-US").NumberFormat.CurrencyDecimalSeparator,
                    inputString.Substring(inputString.Length - 2));
                val = decimal.Parse(parsedDecimal);
            }
            else
            {
                val = (decimal)input;
            }

            //decimal val = Convert.ToDecimal(input, CultureInfo.GetCultureInfo("en-US"));

            string currency = SiteContext.Current.Shop.Currency;

            var culture = _cultures.Value.FirstOrDefault(c => new RegionInfo(c.Name).ISOCurrencySymbol.Equals(currency, StringComparison.OrdinalIgnoreCase));

            return val.ToString("C", culture.NumberFormat);
        }

        public static string AssetUrl(string input)
        {
            if (input == null)
            {
                return null;
            }

            return AssetUrl(input.EndsWith(".jpeg") ? "~/images/" : "~/themes/assets/", String.Format("{0}?theme={1}", input, SiteContext.Current.Theme));
        }

        public static string ShopifyAssetUrl(object input)
        {
            return AssetUrl("~/global/assets/", input.ToString());
        }

        /* sasha: there is already a method to strip html, this is not good enough method
        public static string StripHtml(string input)
        {
            return HttpUtility.HtmlDecode(input);
        }
        */

        public static string GlobalAssetUrl(string input)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                throw new InvalidOperationException("The link tag can only be used within a valid HttpContext");
            }

            var httpContextBase = new HttpContextWrapper(httpContext);
            var routeData = new RouteData();
            var requestContext = new RequestContext(httpContextBase, routeData);

            var urlHelper = new UrlHelper(requestContext);

            return urlHelper.Content(String.Format("~/global/assets/{0}", input));
        }

        #region Public Methods and Operators
        public static string ImgTag(object input, string alt = "", string css = "")
        {
            return input == null ? null : GetImageTag(GetImageUrl(input), alt, css);
        }

        public static string ImgUrl(object input, string type)
        {
            return input == null ? null : AssetUrl(GetImageUrl(input));
        }

        public static string LinkTo(string input, string link, string title = "")
        {
            if (String.IsNullOrEmpty(link))
            {
                link = VirtualPathUtility.ToAbsolute("~/");
            }

            return String.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", link, title, input);
        }

        public static string LinkToSwitchLanguage(Context context, object input)
        {
            var url = VirtualPathUtility.ToAbsolute(String.Format("~/{0}", input));
            return url;
        }

        public static string LinkToAddTag(Context context, object input, object tag)
        {
            return LinkToTag(context, input, tag);
        }

        public static string LinkToRemoveTag(Context context, object input, object tag)
        {
            var relativeUri = HttpContext.Current.Request.Url;
            if (tag == null)
            {
                return String.Format("<a title=\"Remove all tags\" href=\"{1}\">{0}</a>", input, relativeUri.LocalPath);
            }

            if (tag is Tag)
            {
                return LinkToRemoveTag(context, input, tag as Tag);
            }

            var match = TagSyntax.Match(tag.ToString());

            if (match.Success)
            {
                var field = match.Groups[1].Value;
                var tagName = match.Groups[2].Value;

                var count = 0;
                if (match.Groups.Count == 4)
                {
                    count = Int32.Parse(match.Groups[3].Value);
                }

                var url = UpdateWithTags(context, relativeUri, String.Format("{0}_{1}", field, tagName), true);

                return String.Format(
                    "<a title=\"Remove tag {1}\" href=\"{0}\">{1}{2}</a>",
                    url,
                    input,
                    count == 0 ? "" : String.Format(" ({0})", count));
            }

            return input.ToString();
        }

        public static string LinkToRemoveTag(Context context, object input, Tag tag)
        {
            var field = tag.Field;
            var tagName = tag.Label;
            var val = tag.Value;
            var count = tag.Count;

            var relativeUri = HttpContext.Current.Request.Url;
            var url = UpdateWithTags(context, relativeUri, String.Format("{0}_{1}", field, val), true);

            return String.Format(
                "<a title=\"Remove tag {1}\" href=\"{0}\">{1}{2}</a>",
                url,
                tagName,
                count == 0 ? "" : String.Format(" ({0})", count));
        }

        public static string LinkToTag(Context context, object input, object tag)
        {
            var relativeUri = HttpContext.Current.Request.Url;

            if (tag == null)
            {
                return String.Format("<a title=\"Remove all tags\" href=\"{1}\">{0}</a>", input, relativeUri.LocalPath);
            }

            if (tag is Tag)
            {
                return LinkToTag(context, input, tag as Tag);
            }

            // parse "brand_sony (12)" and convert it to filter of type brand=sony
            // var syntax = new Regex(@"([A-Za-z0-9]+)_([A-Za-z0-9]+)\s\(([0-9]+)\)");
            var match = TagSyntax.Match(tag.ToString());

            if (match.Success)
            {
                var field = match.Groups[1].Value;
                var tagName = match.Groups[2].Value;

                var count = 0;
                if (match.Groups.Count == 4)
                {
                    count = Int32.Parse(match.Groups[3].Value);
                }

                var url = UpdateWithTags(context, relativeUri, String.Format("{0}_{1}", field, tagName));

                return String.Format(
                    "<a title=\"Show products matching tag {1}\" href=\"{0}\">{1}{2}</a>",
                    url,
                    tagName,
                    count == 0 ? "" : String.Format(" ({0})", count));
            }
            return string.Format(
                "<a title=\"Show products matching tag {0}\" href=\"{1}{0}\">{0}</a>",
                tag,
                relativeUri.LocalPath);
        }

        public static string LinkToTag(Context context, object input, Tag tag)
        {
            var field = tag.Field;
            var tagName = tag.Label;
            var val = tag.Value;
            var count = tag.Count;

            var relativeUri = HttpContext.Current.Request.Url;
            var url = UpdateWithTags(context, relativeUri, String.Format("{0}_{1}", field, val));

            return string.Format(
                "<a title=\"Show products matching tag {1}\" href=\"{0}\">{1}{2}</a>",
                url,
                tagName,
                count == 0 ? "" : String.Format(" ({0})", count));
        }

        public static string PaymentTypeImgUrl(string input)
        {
            return GetImageUrl(AssetUrl(String.Format("cc-{0}.png", input)));
        }

        public static string ProductImgUrl(object input, string type)
        {
            return ImgUrl(input, type);
        }

        public static string Within(string input, object collection)
        {
            var url = String.Empty;
            if (input.StartsWith("/products/"))
            {
                url = VirtualPathUtility.ToAbsolute(String.Format("~{0}{1}", collection, input));
            }
            else // URL already includes category and full site path
            {
                url = VirtualPathUtility.ToAbsolute(String.Format("~/{0}", input));
            }

            return url;
        }

        /// <summary>
        /// Date filter which is aware of locale settings for date formats.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date(object input, string format)
        {
            if (!String.IsNullOrEmpty(format) && !format.Contains("%")) // special formats that can be defined in settings
            {
                var loc = String.Format("date_formats.{0}", format);
                var newFormat = TranslationFilter.T(loc);
                if (newFormat != loc)
                    format = newFormat;

                if (format == "long")
                {
                    format = "%d %b %Y %X";
                }
            }

            return StandardFilters.Date(input, format);
        }

        public static string Camelize(string input)
        {
            return StandardFilters.Capitalize(input);
        }

        public static string Handleize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // TODO: combine into one regex
            var replacedString = WordReplaceSyntax.Replace(input.ToLower(), ""); // remove special characters
            replacedString = WordReplaceSyntax2.Replace(replacedString, "-"); // add "-" instead of spaces
            return replacedString;
        }
        #endregion

        #region Methods
        private static string AssetUrl(string prefix, string input)
        {
            if (input.StartsWith("http"))
            {
                return input;
            }

            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                throw new InvalidOperationException("The link tag can only be used within a valid HttpContext");
            }

            var httpContextBase = new HttpContextWrapper(httpContext);
            var routeData = new RouteData();
            var requestContext = new RequestContext(httpContextBase, routeData);

            var urlHelper = new UrlHelper(requestContext);

            return urlHelper.ContentAbsolute(String.Format("{0}{1}", prefix, input));
        }

        private static string GetImageTag(string src, string alt, string css)
        {
            return String.Format("<img src=\"{0}\" alt=\"{1}\" class=\"{2}\" />", src, alt, css);
        }

        private static string GetImageUrl(object input)
        {
            if (input is Product)
            {
                return (input as Product).FeaturedImage.Src;
            }
            if (input is Image)
            {
                return (input as Image).Src;
            }
            if (input is Variant)
            {
                return (input as Variant).Image.Src;
            }
            if (input is Collection)
            {
                return (input as Collection).Image.Src;
            }
            if (input is LineItem)
            {
                var lineItem = input as LineItem;
                return lineItem.Image;
            }
            return input.ToString();
        }

        private static string UpdateWithTags(Context context, Uri requestUri, string tag, bool remove = false)
        {
            var helper = Url;
            if (remove)
            {
                return helper.RemoveParameterUrl(requestUri.PathAndQuery, "tags", tag);
            }
            else
            {
                return helper.SetParameter(requestUri.PathAndQuery, "tags", tag, false);
            }
        }
        #endregion

        private static UrlHelper Url
        {
            get
            {
                var httpContext = HttpContext.Current;
                if (httpContext == null)
                {
                    throw new InvalidOperationException("The link tag can only be used within a valid HttpContext");
                }

                var httpContextBase = new HttpContextWrapper(httpContext);
                var routeData = new RouteData();
                var requestContext = new RequestContext(httpContextBase, routeData);

                var urlHelper = new UrlHelper(requestContext);
                return urlHelper;
            }
        }
    }
}