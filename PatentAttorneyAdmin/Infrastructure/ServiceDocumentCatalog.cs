using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Infrastructure;

public record DocumentRequirement(
    ServiceDocumentType Type,
    string LabelKey,
    string? HintKey = null,
    string? GroupKey = null);

public static class ServiceDocumentCatalog
{
    public static string? GetDescriptionKey(string? serviceCode) => serviceCode switch
    {
        ServiceCodes.HomsCourse => "HomsCourseDescription",
        ServiceCodes.PatentRepCertification => "PatentRepCertDescription",
        ServiceCodes.RegisterCertificate => "RegisterCertDescription",
        ServiceCodes.VrisLicense => "VrisLicenseDescription",
        _ => null
    };

    public static IReadOnlyList<DocumentRequirement>? GetRequirements(string? serviceCode) =>
        serviceCode switch
        {
            ServiceCodes.HomsCourse => HomsCourseDocs,
            ServiceCodes.PatentRepCertification => CertificationDocs,
            ServiceCodes.RegisterCertificate => CertificationDocs,
            ServiceCodes.VrisLicense => VrisLicenseDocs,
            _ => null
        };

    public static string GetLabelKey(string? serviceCode, ServiceDocumentType type) =>
        GetRequirements(serviceCode)?.FirstOrDefault(r => r.Type == type)?.LabelKey ?? type.ToString();

    private static readonly DocumentRequirement[] HomsCourseDocs =
    {
        new(ServiceDocumentType.ApplicationLetter, "DocApplicationLetter", "DocApplicationLetterHint"),
        new(ServiceDocumentType.PassportCopy, "DocPassportCopy", "DocPassportCopyHint"),
        new(ServiceDocumentType.DiplomaCopy, "DocDiplomaCopy", "DocDiplomaCopyHint"),
        new(ServiceDocumentType.PaymentProof, "DocPaymentProof", "DocPaymentProofHint")
    };

    private static readonly DocumentRequirement[] CertificationDocs =
    {
        new(ServiceDocumentType.PersonnelForm, "DocPersonnelForm", "DocPersonnelFormHint"),
        new(ServiceDocumentType.Photos3x4, "DocPhotos3x4", "DocPhotos3x4Hint"),
        new(ServiceDocumentType.DiplomaCopy, "DocDiplomaHigher", "DocDiplomaHigherHint"),
        new(ServiceDocumentType.PracticalWorkInfo, "DocPracticalWorkInfo", "DocPracticalWorkInfoHint"),
        new(ServiceDocumentType.WorkBookExtract, "DocWorkBookExtract", "DocWorkBookExtractHint"),
        new(ServiceDocumentType.CertificationPaymentProof, "DocCertificationPayment", "DocCertificationPaymentHint")
    };

    private static readonly DocumentRequirement[] VrisLicenseDocs =
    {
        new(ServiceDocumentType.DiplomaCopy, "DocVrisDiploma", "DocVrisDiplomaHint", "DocGroupMain"),
        new(ServiceDocumentType.AttestationCommissionDecision, "DocAttestationDecision", "DocAttestationDecisionHint", "DocGroupMain"),
        new(ServiceDocumentType.PassportCopy, "DocPassportCopy", "DocPassportCopyHint", "DocGroupAdditional"),
        new(ServiceDocumentType.SiRegistrationCertificate, "DocSiCertificate", "DocSiCertificateHint", "DocGroupAdditional"),
        new(ServiceDocumentType.RegisterCertificate, "DocRegisterCertificate", "DocRegisterCertificateHint", "DocGroupAdditional"),
        new(ServiceDocumentType.StateFeePaymentProof, "DocStateFeePayment", "DocStateFeePaymentHint", "DocGroupAdditional")
    };
}
