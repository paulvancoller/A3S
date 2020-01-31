using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace za.co.grindrodbank.a3s.tests.Fakes
{
    public class HttpRequestStreamReaderFactoryFake : IHttpRequestStreamReaderFactory
    {
        public TextReader CreateReader(Stream stream, Encoding encoding)
        {
            return new HttpRequestStreamReader(stream, encoding);
        }
    }
}
