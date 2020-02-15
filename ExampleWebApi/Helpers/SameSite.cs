using Microsoft.AspNetCore.Http;

namespace ExampleWebApi.Helpers
{
    // Safari on iOS 12 and macOS 10.15 Mojave doesn't support a SameSite mode of None and treats this as Strict.
    // This bug is fixed in iOS 13 and macOS Catalina.
    // 
    // As a workaround, the following code detects whether the browser correctly supports SameSite=None and, 
    // if it doesn't, no SameSite mode is included.
    //
    // The code is based on that at:
    // https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
    //
    // The code may be configured as part of the CookieOptions at Starup. For example:
    //
    // services.Configure<CookiePolicyOptions>(options =>
    // {
    //     options.OnAppendCookie = cookieContext => SameSite.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    //     options.OnDeleteCookie = cookieContext => SameSite.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    // });

    public static class SameSite
    {
        public static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                if (!BrowserSupportsSameSiteNone(httpContext.Request.Headers["User-Agent"].ToString()))
                {
                    // Unspecified - no SameSite will be included in the Set-Cookie.
                    options.SameSite = (SameSiteMode)(-1);
                }
            }
        }

        private static bool BrowserSupportsSameSiteNone(string userAgent)
        {
            // iOS 12 browsers don't support SameSite=None.
            if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            {
                return false;
            }

            // macOS 10.14 Mojave browsers don't support SameSite=None.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return false;
            }

            // Old versions of Chrome don't support SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return false;
            }

            return true;
        }
    }
}
