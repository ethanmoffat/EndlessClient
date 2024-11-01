using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text;
using System;

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

            sb.Append("labels=bug,userreport");
            sb.Append($"&title=Unhandled+Client+Exception+Needs+Triage");
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

*Diagnostic Information added automatically. Please do not delete!*

**Exception message**: {ex.Message}
**Stack Trace**:
```
{ex.StackTrace}
```";

            if (ex.InnerException != null)
            {
                body += @$"

**Inner Exception**: {ex.InnerException.Message}
**Stack Trace**:
```
{ex.InnerException.StackTrace}
```
";
            }

            var encoded = UrlEncoder.Default.Encode(body);
            sb.Append($"&body={encoded}");

            return sb.ToString();
        }
    }
}
