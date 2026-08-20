namespace ProjetoJogosDigitais.Gameplay
{
    public enum CareerRank
    {
        Intern,
        JuniorDeveloper,
        MidLevelDeveloper,
        SeniorDeveloper,
        TechLead,
        Cto,
        Owner
    }

    public enum OfficeActionType
    {
        Code,
        Meeting,
        Study,
        Coffee,
        Break
    }

    public enum OfficeTier
    {
        Intern,
        Developer,
        Leadership,
        Executive
    }

    public readonly struct OfficeActionResult
    {
        public OfficeActionResult(bool succeeded, string message, int deliveryGained)
        {
            Succeeded = succeeded;
            Message = message;
            DeliveryGained = deliveryGained;
        }

        public bool Succeeded { get; }
        public string Message { get; }
        public int DeliveryGained { get; }
    }
}
