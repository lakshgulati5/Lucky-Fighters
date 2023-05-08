namespace Lucky_Fighters
{
    public class Flower : Interactive
    {
        public Flower() : base("Flower", 0)
        {
        }


        internal override void ApplyEffect(Player player)
        {
            if (player.Health >= 100f) return;
            if (!IsEnabled) return;
            
            if (player.Health + 30f > 100f) player.Health = 100f;
            else player.Health += 30f;
            IsEnabled = false;

            player.Stats.InteractivesUsed++;
        }
    }
}