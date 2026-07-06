namespace PatentAttorneyAdmin.Models;

public static class ServiceCodes
{
    public const string HomsCourse = "homs-course";
    public const string PatentRepCertification = "patent-rep-certification";
    public const string RegisterCertificate = "register-certificate";
    public const string VrisLicense = "vris-license";

    public static readonly string[] ImplementedCodes =
    {
        HomsCourse,
        PatentRepCertification,
        RegisterCertificate,
        VrisLicense
    };

    public static bool IsImplemented(string? code) =>
        code != null && ImplementedCodes.Contains(code);
}
