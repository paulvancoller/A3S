/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
namespace za.co.grindrodbank.a3sidentityserver.Quickstart.UI
{
    public class ConsentOfServiceViewModel : ConsentOfServiceInputModel
    {
        public string HtmlContents { get; set; }
        public string CssContents { get; set; }
        public int AgreementCount { get; set; }
        public string AgreementName { get; set; }
    }
}
