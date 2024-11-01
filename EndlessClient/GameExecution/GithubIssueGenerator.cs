using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;

namespace EndlessClient.GameExecution
{
    public static class GithubIssueGenerator
    {
        public static void FileIssue(Exception ex)
        {
            OpenBrowser(BuildIssueUrl(ex));
        }

        private static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url); // Not tested
            }
        }

        private static string BuildIssueUrl(Exception ex)
        {
            var sb = new StringBuilder("https://github.com/ethanmoffat/EndlessClient/issues/new?");

            string crashMethod;
            try
            {
                crashMethod = ex.InnerException == null
                    ? ex.StackTrace.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]
                     : ex.InnerException.StackTrace.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                crashMethod = crashMethod[..crashMethod.IndexOf('(')];
            }
            catch
            {
                crashMethod = string.Empty;
            }

            var title = $"Unhandled+Client+Exception{(crashMethod == "" ? "" : $" in {crashMethod}")}";
            sb.Append($"title={UrlEncoder.Default.Encode(title)}");

            sb.Append("&labels=bug,userreport");
            sb.Append("&assignees=ethanmoffat");

            var body = @$"Thank you for your report! Please fill in the following information to help me better triage/resolve this issue.
**What were you doing when the exception occurred?**
<Answer>

**What server were you playing on?**
<Answer>

**Has this crash occurred for you before?**
<Answer>

**Expected behavior (other than it not crashing)?**
<Answer>

**Diagnostic Information**

{ex}
";
            var encoded = UrlEncoder.Default.Encode(body);
            sb.Append($"&body={encoded}");

            return sb.ToString();
        }
    }
}
