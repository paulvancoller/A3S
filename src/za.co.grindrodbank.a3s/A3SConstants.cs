/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
namespace za.co.grindrodbank.a3s
{
    public static class A3SConstants
    {
        public const string TERMS_OF_SERVICE_HTML_FILE = "terms_of_service.html";
        public const string TERMS_OF_SERVICE_CSS_FILE = "terms_of_service.css";

        public const string CONSENT_OF_SERVICE_HTML_FILE = "consent_of_service.html";
        public const string CONSENT_OF_SERVICE_CSS_FILE = "consent_of_service.css";

        public const string CSS_STYLE_RULES_REGEX = @"([\.#][_A-Za-z0-9\-]+)[^}]*{[^}]*}";
        public const string CSS_STYLE_CLEAR_COMMENTS_REGEX = @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)";
    }
}
