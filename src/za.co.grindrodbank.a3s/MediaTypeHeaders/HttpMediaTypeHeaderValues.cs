/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using Microsoft.Net.Http.Headers;

namespace za.co.grindrodbank.a3s.MediaTypeHeaders
{
    static internal class MediaTypeHeaderValues
    {
        public static readonly MediaTypeHeaderValue ApplicationYaml
            = MediaTypeHeaderValue.Parse("application/x-yaml").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextYaml
            = MediaTypeHeaderValue.Parse("text/yaml").CopyAsReadOnly();
    }
}
