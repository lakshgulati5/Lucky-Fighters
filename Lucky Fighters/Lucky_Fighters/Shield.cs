namespace Lucky_Fighters
{
    public class Shield : Interactive
    {
        public Shield() : base("Shield", 0)
        {
        }

        internal override void ApplyEffect(Player player)
        {
            player.AddTask(
                new Task(0, () => { player.IsShielded = true; }).Then(5, () => { player.IsShielded = false; })
            );
        }
    }
}