using System;
using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using MailKit.Net.Proxy;
using MailKit.Security;

public class ImapHandlerOutlook
{
    public static async Task<bool> ReturnEmailAsync(string email, string password, string proxy)
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Email and password cannot be empty.");
                return false;
            }

            using (var imapClient = new ImapClient())
            {
                var proxyHost = proxy.Split(':')[0];
                var proxyPort = proxy.Split(':')[1];
                var proxyUsername = proxy.Split(':')[2];
                var proxyPassword = proxy.Split(':')[3];
                //Console.WriteLine("Email: " + email + " Password: " + password);
                // Console.WriteLine("Host: " + proxyHost + " Port: " + proxyPort);
                // Console.WriteLine("Proxy Information From Server: " + proxyUsername + ":" + proxyPassword);
                imapClient.Timeout = 10000;
                imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                imapClient.ProxyClient = new HttpProxyClient(proxyHost, int.Parse(proxyPort), new NetworkCredential(proxyUsername, proxyPassword));

                await imapClient.ConnectAsync("outlook.office365.com", 993, SecureSocketOptions.SslOnConnect).ConfigureAwait(false);

                imapClient.AuthenticationMechanisms.Remove("XOAUTH2");

                await imapClient.AuthenticateAsync(email, password).ConfigureAwait(false);

                Console.WriteLine("VALID!");

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return false;
        }
    }
}
