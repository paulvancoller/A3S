/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace za.co.grindrodbank.a3s.Models
{
    public class TermsOfServiceModel : AuditableModel
    {
        public Guid Id { get; set; }
        public string AgreementName { get; set; }
        public string Version { get; set; }
        public byte[] AgreementFile { get; set; }

        public List<TeamModel> Teams { get; set; }
        public List<TermsOfServiceUserAcceptanceModel> TermsOfServiceAcceptances { get; set; }

        [NotMapped]
        public string HtmlContents { get; set; }

        [NotMapped]
        public string CssContents { get; set; }
    }
}
