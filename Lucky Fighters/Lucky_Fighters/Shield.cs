namespace Lucky_Fighters
{
    public class Shield : Interactive
    {
        public Shield() : base("Shield", 0, false)
        {
        }

        internal override void ApplyEffect(Player player)
        {
            if (!IsEnabled) return;

            player.IsShielded = true;
            IsEnabled = false;
            player.AddTask(
                new Task(5, () => { player.IsShielded = false; })
            );
        }
    }
}