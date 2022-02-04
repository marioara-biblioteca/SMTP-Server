using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Heijden.DNS;

using Org.BouncyCastle.Crypto;

using MimeKit;
using MimeKit.Cryptography;

namespace Common.SMTP 
{ 
    class ExamplePublicKeyLocator : DkimPublicKeyLocatorBase
    {
        readonly Dictionary<string, AsymmetricKeyParameter> cache;
        readonly Resolver resolver;
        public ExamplePublicKeyLocator()
        {
            cache = new Dictionary<string, AsymmetricKeyParameter>();

            resolver = new Resolver("8.8.8.8")
            {
                TransportType = TransportType.Tcp,
                UseCache = true,
                Retries = 3
            };
        }

        public AsymmetricKeyParameter DnsLookup(string domain, string selector)//, CancellationToken cancellationToken)
        {
            var query = selector + "._domainkey." + domain;
            AsymmetricKeyParameter pubkey;

            // checked if we've already fetched this key
            if (cache.TryGetValue(query, out pubkey))
                return pubkey;

            // make a DNS query
            var response = resolver.Query(query, QType.TXT);
            var builder = new StringBuilder();

            // combine the TXT records into 1 string buffer
            foreach (var record in response.RecordsTXT)
            {
                foreach (var text in record.TXT)
                    builder.Append(text);
            }

            var txt = builder.ToString();

            // DkimPublicKeyLocatorBase provides us with this helpful method.
            pubkey = GetPublicKey(txt);

            cache.Add(query, pubkey);

            return pubkey;
        }

        public override AsymmetricKeyParameter LocatePublicKey(string methods, string domain, string selector, CancellationToken cancellationToken = default(CancellationToken))
        {
        //    var methodList = methods.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        //    for (int i = 0; i < methodList.Length; i++)
        //    {
        //        if (methodList[i] == "dns/txt")
        //            return DnsLookup(domain, selector, cancellationToken);
        //    }
              throw new Exception("haha");
        //    throw new NotSupportedException(string.Format("{0} does not include any suported lookup methods.", methods));
        }

        public override Task<AsymmetricKeyParameter> LocatePublicKeyAsync(string methods, string domain, string selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new Exception("haha");
        //    return Task.Run(() => {
        //        return LocatePublicKey(methods, domain, selector, cancellationToken);
        //    }, cancellationToken);
        }
       
    }
}
