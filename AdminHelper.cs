using System.Security.Principal;

namespace GenshinImpactAutoMusic;

internal static class AdminHelper
{
    public static bool IsElevated() => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
}
