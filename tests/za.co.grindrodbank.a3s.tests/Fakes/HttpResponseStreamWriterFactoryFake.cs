using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace za.co.grindrodbank.a3s.tests.Fakes
{
    public class HttpResponseStreamWriterFactoryFake : IHttpResponseStreamWriterFactory
    {
        public const int DefaultBufferSize = 16 * 1024;

        public TextWriter CreateWriter(Stream stream, Encoding encoding)
        {
            return new HttpResponseStreamWriter(stream, encoding, DefaultBufferSize);
        }
    }
}
