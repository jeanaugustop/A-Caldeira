namespace ACaldeira.Progression
{
    public enum RunOfferKind { Weapon, Attribute, Accessory }

    public sealed class RunOffer
    {
        public readonly RunOfferKind Kind;
        public readonly string Title;
        public readonly string Description;
        internal readonly int Id;
        internal readonly bool IsNewEquipment;
        internal readonly float Value;
        internal readonly int Rarity;

        internal RunOffer(RunOfferKind kind, string title, string description, int id = -1,
            bool isNewEquipment = false, float value = 0f, int rarity = 0)
        {
            Kind = kind; Title = title; Description = description; Id = id;
            IsNewEquipment = isNewEquipment; Value = value; Rarity = rarity;
        }
    }
}
