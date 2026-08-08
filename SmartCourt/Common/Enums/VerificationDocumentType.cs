namespace SmartCourt.Common.Enums
{
    public enum VerificationDocumentType : byte
    {
        NationalIdFront = 1,
        NationalIdBack = 2,

        BarAssociationCardFront = 3,
        BarAssociationCardBack = 4,

        SelfieWithId = 5,
        Other = 6,
        OfficialProfilePicture = 7
    }
}
